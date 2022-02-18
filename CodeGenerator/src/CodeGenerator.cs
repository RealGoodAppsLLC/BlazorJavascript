using System.Collections.Immutable;
using System.Text;
using Newtonsoft.Json;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models;

namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public class CodeGenerator
    {
        private readonly ParsedInfo _parsedInfo;
        private readonly string _outputDirectory;

        public CodeGenerator(
            ParsedInfo parsedInfo,
            string outputDirectory)
        {
            _parsedInfo = parsedInfo;
            _outputDirectory = outputDirectory;
        }

        public void Generate()
        {
            var prototypes = new List<(InterfaceInfo InterfaceInfo, GlobalVariableInfo GlobalVariableInfo)>();

            foreach (var interfaceInfo in _parsedInfo.Interfaces)
            {
                var contents = GenerateInterfaceFileContents(
                    interfaceInfo.Name,
                    interfaceInfo.ExtractTypeParametersResult,
                    interfaceInfo.ExtendsList,
                    interfaceInfo.Body);

                var interfaceOutputPath = Path.Combine(
                    _outputDirectory,
                    "Interfaces",
                    $"I{interfaceInfo.Name}.cs");

                if (File.Exists(interfaceOutputPath))
                {
                    throw new Exception($"File already exists: {interfaceOutputPath}");
                }

                File.WriteAllText(interfaceOutputPath, contents);

                GlobalVariableInfo? prototypeGlobalVariable = null;

                foreach (var globalVariable in _parsedInfo.GlobalVariables)
                {
                    if (globalVariable.InlineInterface != null)
                    {
                        if (DoesInterfaceBodyHavePrototype(globalVariable.InlineInterface, interfaceInfo))
                        {
                            prototypeGlobalVariable = globalVariable;
                            break;
                        }

                        continue;
                    }

                    if (globalVariable.Type != null && globalVariable.Type.Single != null)
                    {
                        var typeInterface =
                            _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == globalVariable.Type.Single.Name);

                        if (typeInterface == null)
                        {
                            continue;
                        }

                        if (DoesInterfaceBodyHavePrototype(typeInterface.Body, interfaceInfo))
                        {
                            prototypeGlobalVariable = globalVariable;
                            break;
                        }
                    }
                }

                if (prototypeGlobalVariable != null)
                {
                    var prototypeContents = GeneratePrototypeFileContents(interfaceInfo);

                    var prototypeOutputPath = Path.Combine(
                        _outputDirectory,
                        "Prototypes",
                        $"{interfaceInfo.Name}Prototype.cs");

                    if (File.Exists(prototypeOutputPath))
                    {
                        throw new Exception($"File already exists: {prototypeOutputPath}");
                    }

                    prototypes.Add((interfaceInfo, prototypeGlobalVariable));
                    File.WriteAllText(prototypeOutputPath, prototypeContents);
                }
            }

            // FIXME: Right now, we know the globalThis is a `Window`, but we might not want to assume this
            //        in the future, especially if this code is used to generate bindings for libraries.
            var windowInterface = _parsedInfo.Interfaces.First(interfaceInfo => interfaceInfo.Name == "Window");
            var allWindowPropertyDetails = GetPropertiesFromInterface(
                windowInterface,
                true,
                ImmutableList.Create<InterfaceInfo>());
            var allWindowGetters = GetGetAccessorsFromInterfaceRecursively(windowInterface);

            foreach (var globalVariableInfo in _parsedInfo.GlobalVariables)
            {
                // HACK: Let's exclude anything that was already defined in the `Window` interface.
                if (allWindowPropertyDetails.Any(propertyDetails => propertyDetails.PropertyInfo.Name == globalVariableInfo.Name)
                    || allWindowGetters.Any(getAccessor => getAccessor.Name == globalVariableInfo.Name))
                {
                    continue;
                }

                var contents = GenerateGlobalVariableFileContents(globalVariableInfo);

                var globalVariableOutputPath = Path.Combine(
                    _outputDirectory,
                    "Globals",
                    $"{globalVariableInfo.Name}Global.cs");

                if (File.Exists(globalVariableOutputPath))
                {
                    throw new Exception($"File already exists: {globalVariableOutputPath}");
                }

                File.WriteAllText(globalVariableOutputPath, contents);

                if (globalVariableInfo.InlineInterface != null)
                {
                    var inlineInterfaceContents = GenerateInterfaceFileContents(
                        $"InlineInterfaceFor{globalVariableInfo.Name}",
                        null,
                        ImmutableList.Create<TypeInfo>(),
                        globalVariableInfo.InlineInterface);

                    var inlineInterfaceOutputPath = Path.Combine(
                        _outputDirectory,
                        "Interfaces",
                        $"IInlineInterfaceFor{globalVariableInfo.Name}.cs");

                    if (File.Exists(inlineInterfaceOutputPath))
                    {
                        throw new Exception($"File already exists: {inlineInterfaceOutputPath}");
                    }

                    File.WriteAllText(inlineInterfaceOutputPath, inlineInterfaceContents);
                }
            }

            var javascriptContents = GenerateJavascriptFileContents(prototypes.ToImmutableList());

            var javascriptOutputPath = Path.Combine(
                _outputDirectory,
                "Javascript",
                "script.js");

            if (File.Exists(javascriptOutputPath))
            {
                throw new Exception($"File already exists: {javascriptOutputPath}");
            }

            File.WriteAllText(javascriptOutputPath, javascriptContents);

            var objectFactoryContents = GenerateObjectFactoryFileContents(prototypes.ToImmutableList());

            var objectFactoryOutputPath = Path.Combine(
                _outputDirectory,
                "Factories",
                "JSObjectFactory.cs");

            if (File.Exists(objectFactoryOutputPath))
            {
                throw new Exception($"File already exists: {objectFactoryOutputPath}");
            }

            File.WriteAllText(objectFactoryOutputPath, objectFactoryContents);
        }

        private string GenerateInterfaceFileContents(
            string interfaceName,
            ExtractTypeParametersResult? extractTypeParametersResult,
            ImmutableList<TypeInfo> interfaceExtendsList,
            InterfaceBodyInfo interfaceBodyInfo)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.BuiltIns;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.GlobalVariables;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Prototypes;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace RealGoodApps.BlazorJavascript.Interop.Interfaces");
            stringBuilder.AppendLine("{");

            stringBuilder.Append($"{Indent(1)}public interface I{interfaceName}");

            if (extractTypeParametersResult != null)
            {
                stringBuilder.Append(ExtractTypeParametersString(extractTypeParametersResult));
            }

            var extendsList = interfaceExtendsList
                .Select(extendTypeInfo => extendTypeInfo)
                .Where(extendTypeInfo => extendTypeInfo.Single != null)
                .Select(GetRenderedTypeName)
                .Append("IJSObject")
                .ToImmutableList();
            stringBuilder.Append($" : {string.Join(", ", extendsList)}");
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.AppendLine(Indent(1) + "{");

            var methods = GetMethodsFromInterfaceBody(interfaceBodyInfo);

            foreach (var methodInfo in methods)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                RenderMethodBeginning(stringBuilder, methodInfo, null);
                stringBuilder.Append(';');
                stringBuilder.Append(Environment.NewLine);
            }

            var properties = GetPropertiesFromInterfaceBody(
                interfaceBodyInfo,
                extractTypeParametersResult);

            foreach (var propertyInfo in properties)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                RenderPropertyBeginning(stringBuilder, propertyInfo, null);
                stringBuilder.Append(" { get; ");
                if (!propertyInfo.IsReadonly)
                {
                    stringBuilder.Append("set; ");
                }

                stringBuilder.Append('}');
                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private static string ExtractTypeParametersString(ExtractTypeParametersResult extractTypeParametersResult)
        {
            var stringBuilder = new StringBuilder();

            if (extractTypeParametersResult.TypeParameters.Any())
            {
                stringBuilder.Append('<');

                stringBuilder.Append(string.Join(", ", extractTypeParametersResult.TypeParameters
                    .Select(typeParameter => typeParameter.Name)));

                stringBuilder.Append('>');
            }

            return stringBuilder.ToString();
        }

        private static string ExtractTypeParametersStringForPrototypeConstructorDispatch(InterfaceInfo interfaceInfo)
        {
            var stringBuilder = new StringBuilder();

            if (interfaceInfo.ExtractTypeParametersResult.TypeParameters.Any())
            {
                stringBuilder.Append('<');

                stringBuilder.Append(string.Join(", ", interfaceInfo.ExtractTypeParametersResult.TypeParameters
                    .Select(_ => "IJSObject")));

                stringBuilder.Append('>');
            }

            return stringBuilder.ToString();
        }

        private void RenderMethodBeginning(
            StringBuilder stringBuilder,
            MethodInfo methodInfo,
            InterfaceInfo? prefixInterfaceInfo)
        {
            stringBuilder.Append(GetRenderedTypeName(methodInfo.ReturnType));
            stringBuilder.Append(' ');

            if (prefixInterfaceInfo != null)
            {
                var typeParametersString = ExtractTypeParametersString(prefixInterfaceInfo.ExtractTypeParametersResult);
                stringBuilder.Append($"I{prefixInterfaceInfo.Name}{typeParametersString}");
                stringBuilder.Append('.');
            }

            stringBuilder.Append(methodInfo.GetNameForCSharp());

            stringBuilder.Append('(');

            var isFirst = true;

            foreach (var parameterInfo in methodInfo.Parameters)
            {
                if (!isFirst)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(GetRenderedTypeName(parameterInfo.Type));
                stringBuilder.Append(' ');
                stringBuilder.Append(parameterInfo.GetNameForCSharp());
                isFirst = false;
            }

            stringBuilder.Append(')');
        }

        private void RenderPropertyBeginning(
            StringBuilder stringBuilder,
            PropertyInfo propertyInfo,
            InterfaceInfo? prefixInterfaceInfo)
        {
            stringBuilder.Append(GetRenderedTypeName(propertyInfo.Type));
            stringBuilder.Append(' ');

            if (prefixInterfaceInfo != null)
            {
                var typeParametersString = ExtractTypeParametersString(prefixInterfaceInfo.ExtractTypeParametersResult);
                stringBuilder.Append($"I{prefixInterfaceInfo.Name}{typeParametersString}");
                stringBuilder.Append('.');
            }

            stringBuilder.Append(propertyInfo.GetNameForCSharp());
        }

        private ImmutableList<(InterfaceInfo interfaceInfo, MethodInfo MethodInfo)> GetMethodsFromInterface(
            InterfaceInfo interfaceInfo,
            bool recursive,
            ImmutableList<InterfaceInfo> alreadyProcessedInterfaces)
        {
            var methods = new List<(InterfaceInfo interfaceInfo, MethodInfo MethodInfo)>();

            if (recursive)
            {
                foreach (var extendTypeInfo in interfaceInfo.ExtendsList)
                {
                    if (extendTypeInfo.Single == null)
                    {
                        continue;
                    }

                    var extendInterfaceInfo = _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == extendTypeInfo.Single.Name);

                    if (extendInterfaceInfo == null
                        || alreadyProcessedInterfaces.Any(i => i.Name == extendInterfaceInfo.Name))
                    {
                        continue;
                    }

                    alreadyProcessedInterfaces = alreadyProcessedInterfaces.Add(extendInterfaceInfo);
                    methods.AddRange(GetMethodsFromInterface(extendInterfaceInfo, true, alreadyProcessedInterfaces));
                }
            }

            var bodyMethods = GetMethodsFromInterfaceBody(interfaceInfo.Body);

            methods.AddRange(bodyMethods
                .Select(methodInfo => (interfaceInfo, methodInfo))
                .ToImmutableList());

            return methods.ToImmutableList();
        }

        private ImmutableList<(InterfaceInfo interfaceInfo, PropertyInfo PropertyInfo)> GetPropertiesFromInterface(
            InterfaceInfo interfaceInfo,
            bool recursive,
            ImmutableList<InterfaceInfo> alreadyProcessedInterfaces)
        {
            var properties = new List<(InterfaceInfo interfaceInfo, PropertyInfo PropertyInfo)>();

            if (recursive)
            {
                foreach (var extendTypeInfo in interfaceInfo.ExtendsList)
                {
                    if (extendTypeInfo.Single == null)
                    {
                        continue;
                    }

                    var extendInterfaceInfo = _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == extendTypeInfo.Single.Name);

                    if (extendInterfaceInfo == null
                        || alreadyProcessedInterfaces.Any(i => i.Name == extendInterfaceInfo.Name))
                    {
                        continue;
                    }

                    alreadyProcessedInterfaces = alreadyProcessedInterfaces.Add(extendInterfaceInfo);
                    properties.AddRange(GetPropertiesFromInterface(extendInterfaceInfo, true, alreadyProcessedInterfaces));
                }
            }


            var bodyProperties = GetPropertiesFromInterfaceBody(
                interfaceInfo.Body,
                interfaceInfo.ExtractTypeParametersResult);

            properties.AddRange(bodyProperties
                .Select(propertyInfo => (interfaceInfo, propertyInfo))
                .ToImmutableList());

            return properties.ToImmutableList();
        }

        private ImmutableList<GetAccessorInfo> GetGetAccessorsFromInterfaceRecursively(InterfaceInfo interfaceInfo)
        {
            var allGetAccessors = new List<GetAccessorInfo>();

            foreach (var extendInfo in interfaceInfo.ExtendsList)
            {
                if (extendInfo.Single == null)
                {
                    continue;
                }

                var extendInterfaceInfo = _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == extendInfo.Single.Name);

                if (extendInterfaceInfo == null)
                {
                    continue;
                }

                allGetAccessors.AddRange(GetGetAccessorsFromInterfaceRecursively(extendInterfaceInfo));
            }

            allGetAccessors.AddRange(interfaceInfo.Body.GetAccessors);
            return allGetAccessors.ToImmutableList();
        }

        private string GenerateGlobalVariableFileContents(GlobalVariableInfo globalVariableInfo)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Interfaces;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Prototypes;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace RealGoodApps.BlazorJavascript.Interop.GlobalVariables");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(Indent(1) + $"public class {globalVariableInfo.Name}Global");
            stringBuilder.AppendLine(Indent(1) + "{");
            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private static string Indent(int levels)
        {
            var indentationBuilder = new StringBuilder();

            for (int level = 1; level <= levels; level++)
            {
                indentationBuilder.Append("    ");
            }

            return indentationBuilder.ToString();
        }

        private string GeneratePrototypeFileContents(InterfaceInfo interfaceInfo)
        {
            var stringBuilder = new StringBuilder();

            var typeParametersString = ExtractTypeParametersString(interfaceInfo.ExtractTypeParametersResult);

            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using Microsoft.JSInterop;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.BuiltIns;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Extensions;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Interfaces;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace RealGoodApps.BlazorJavascript.Interop.Prototypes");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"{Indent(1)}public class {interfaceInfo.Name}Prototype{typeParametersString} : I{interfaceInfo.Name}{typeParametersString}, IJSObject");
            stringBuilder.AppendLine(Indent(1) + "{");

            stringBuilder.AppendLine(Indent(2) + $"public {interfaceInfo.Name}Prototype(IJSInProcessRuntime jsInProcessRuntime, IJSObjectReference jsObjectReference)");
            stringBuilder.AppendLine(Indent(2) + "{");
            stringBuilder.AppendLine(Indent(3) + "Runtime = jsInProcessRuntime;");
            stringBuilder.AppendLine(Indent(3) + "ObjectReference = jsObjectReference;");
            stringBuilder.AppendLine(Indent(2) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(2) + "public IJSInProcessRuntime Runtime { get; }");
            stringBuilder.AppendLine(Indent(2) + "public IJSObjectReference ObjectReference { get; }");

            var methods = GetMethodsFromInterface(interfaceInfo, true, ImmutableList<InterfaceInfo>.Empty);

            foreach (var (methodInterfaceInfo, methodInfo) in methods)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                RenderMethodBeginning(stringBuilder, methodInfo, methodInterfaceInfo);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Indent(2) + "{");
                stringBuilder.Append(Environment.NewLine);

                var parametersString = string.Join(", ", methodInfo.Parameters.Select(p => p.GetNameForCSharp()));
                var parametersPrefix = string.IsNullOrWhiteSpace(parametersString) ? string.Empty : ", ";
                var returnRenderedTypeName = GetRenderedTypeName(methodInfo.ReturnType);

                stringBuilder.AppendLine(Indent(3) + $"var propertyObj = this.GetPropertyOfObject(\"{methodInfo.Name}\");");
                stringBuilder.AppendLine(Indent(3) + "var propertyAsFunction = propertyObj as JSFunction;");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(3) + "if (propertyAsFunction == null)");
                stringBuilder.AppendLine(Indent(3) + "{");
                stringBuilder.AppendLine(Indent(4) + "throw new InvalidCastException(\"Something went wrong!\");");
                stringBuilder.AppendLine(Indent(3) + "}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(3) + $"var result = propertyAsFunction.Invoke(this{parametersPrefix}{parametersString});");

                if (returnRenderedTypeName != "void")
                {
                    stringBuilder.AppendLine(Indent(3) + $"var resultAsType = result as {returnRenderedTypeName};");
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(Indent(3) + "if (resultAsType == null)");
                    stringBuilder.AppendLine(Indent(3) + "{");
                    stringBuilder.AppendLine(Indent(4) + "throw new InvalidCastException(\"Return value is no good.\");");
                    stringBuilder.AppendLine(Indent(3) + "}");
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(Indent(3) + "return resultAsType;");
                }

                stringBuilder.Append(Indent(2) + "}");
                stringBuilder.Append(Environment.NewLine);
            }

            var properties = GetPropertiesFromInterface(interfaceInfo, true, ImmutableList.Create<InterfaceInfo>());

            foreach (var (propertyInterfaceInfo, propertyInfo) in properties)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                RenderPropertyBeginning(stringBuilder, propertyInfo, propertyInterfaceInfo);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.AppendLine(Indent(2) + "{");
                stringBuilder.AppendLine(Indent(3) + "get");
                stringBuilder.AppendLine(Indent(3) + "{");

                var returnRenderedTypeName = GetRenderedTypeName(propertyInfo.Type);

                stringBuilder.AppendLine(Indent(4) + $"var propertyObj = this.GetPropertyOfObject(\"{propertyInfo.Name}\");");
                stringBuilder.AppendLine(Indent(4) + "if (propertyObj == null)");
                stringBuilder.AppendLine(Indent(4) + "{");
                stringBuilder.AppendLine(Indent(5) + "return null;");
                stringBuilder.AppendLine(Indent(4) + "}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(4) + $"var propertyAsReturnType = propertyObj as {returnRenderedTypeName};");
                stringBuilder.AppendLine(Indent(4) + "if (propertyAsReturnType == null)");
                stringBuilder.AppendLine(Indent(4) + "{");
                stringBuilder.AppendLine(Indent(5) + "throw new InvalidCastException(\"Something went wrong!\");");
                stringBuilder.AppendLine(Indent(4) + "}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(4) + "return propertyAsReturnType;");
                stringBuilder.AppendLine(Indent(3) + "}");

                if (!propertyInfo.IsReadonly)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(Indent(3) + "set");
                    stringBuilder.AppendLine(Indent(3) + "{");
                    stringBuilder.AppendLine(Indent(4) + $"this.SetPropertyOfObject(\"{propertyInfo.Name}\", value);");
                    stringBuilder.AppendLine(Indent(3) + "}");
                }

                stringBuilder.AppendLine(Indent(2) + "}");
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private bool IsFinalTypeSimpleEnoughToRender(TypeInfo parameterInfoType)
        {
            // FIXME: Eventually, this method shouldn't really exist. It is just used to prevent us from having to handle complex type cases right now.
            var finalTypeInfo = ProcessTypeAliases(parameterInfoType);

            return finalTypeInfo.Single == null
                   || finalTypeInfo.Single.IsUnhandled
                   || finalTypeInfo.Single.TypeArguments.Any()
                   || string.IsNullOrWhiteSpace(finalTypeInfo.Single.Name);
        }

        private string GetRenderedTypeName(TypeInfo typeInfo)
        {
            var finalTypeInfo = ProcessTypeAliases(typeInfo);

            var singleTypeInfo = finalTypeInfo.Single;

            if (singleTypeInfo == null)
            {
                throw new Exception("The type name can not be rendered properly due to complexity.");
            }

            var fullName = new StringBuilder();
            fullName.Append(singleTypeInfo.GetNameForCSharp(_parsedInfo.Interfaces));

            var typeArguments = singleTypeInfo
                .TypeArguments
                .Select(typeArgument => typeArgument.Single)
                .WhereNotNull()
                .ToImmutableList();

            if (typeArguments.Any())
            {
                fullName.Append('<');
                fullName.Append(string.Join(",", typeArguments.Select(typeArgument => typeArgument.GetNameForCSharp(_parsedInfo.Interfaces))));
                fullName.Append('>');
            }
            else
            {
                // There is a possibility that the type we are rendering is actually an interface that has one or more default type parameters.
                var typeAsInterface =
                    _parsedInfo.Interfaces.FirstOrDefault(interfaceInfo => interfaceInfo.Name == singleTypeInfo.Name);

                if (typeAsInterface != null && typeAsInterface.ExtractTypeParametersResult.TypeParameters.Any())
                {
                    fullName.Append('<');

                    var isFirst = true;

                    foreach (var typeParameter in typeAsInterface.ExtractTypeParametersResult.TypeParameters)
                    {
                        if (!isFirst)
                        {
                            fullName.Append(", ");
                        }

                        fullName.Append(typeParameter.Default == null
                            ? "IJSObject"
                            : GetRenderedTypeName(typeParameter.Default));

                        isFirst = false;
                    }

                    fullName.Append('>');
                }
            }

            return fullName.ToString();
        }

        private TypeInfo ProcessTypeAliases(TypeInfo typeInfo)
        {
            // Anything that looks like a single type is a candidate for being an alias.
            if (typeInfo.Single == null)
            {
                return typeInfo;
            }

            while (true)
            {
                // FIXME: We are assuming that you can only type alias the most simple case for now.
                var typeAlias = _parsedInfo.TypeAliases
                    .FirstOrDefault(typeAlias => typeInfo.Single != null && typeAlias.Name == typeInfo.Single.Name);

                if (typeAlias == null)
                {
                    return typeInfo;
                }

                typeInfo = typeAlias.AliasType;
            }
        }

        private static bool DoesInterfaceBodyHavePrototype(
            InterfaceBodyInfo interfaceBody,
            InterfaceInfo interfaceInfo)
        {
            foreach (var property in interfaceBody.Properties)
            {
                if (property.Name != "prototype"
                    || property.Type.Single == null
                    || property.Type.Single.Name != interfaceInfo.Name)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private string GenerateJavascriptFileContents(ImmutableList<(InterfaceInfo InterfaceInfo, GlobalVariableInfo GlobalVariableInfo)> prototypes)
        {
            var stringBuilder = new StringBuilder();

            var predefinedTypeIdentifiers = TypeIdentifiers.GetPredefinedTypeIdentifiers();

            foreach (var predefinedTypeIdentifier in predefinedTypeIdentifiers)
            {
                stringBuilder.AppendLine($"window.BlazorJavascript.typeBuiltIn{predefinedTypeIdentifier.ToString()} = {JsonConvert.SerializeObject(predefinedTypeIdentifier.ToInteger())};");
            }

            var prototypeTypeIdentifier = predefinedTypeIdentifiers.Last().ToInteger() + 1;

            foreach (var prototypeInfo in prototypes)
            {
                stringBuilder.AppendLine($"window.BlazorJavascript.typePrototype{prototypeInfo.InterfaceInfo.Name} = {JsonConvert.SerializeObject(prototypeTypeIdentifier)};");
                prototypeTypeIdentifier++;
            }

            stringBuilder.AppendLine();

            stringBuilder.AppendLine("window.BlazorJavascript.obtainPrototype = function(o) {");
            stringBuilder.AppendLine(Indent(1) + "let p = window.BlazorJavascript.unwrap(o);");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(1) + "if (p === null) {");
            stringBuilder.AppendLine(Indent(2) + "return window.BlazorJavascript.typeBuiltInNull;");
            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(1) + "if (p === undefined) {");
            stringBuilder.AppendLine(Indent(2) + "return window.BlazorJavascript.typeBuiltInUndefined;");
            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(1) + "const chain = window.BlazorJavascript.getPrototypeChain(p);");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(1) + "for (let prototypeIndex = 0; prototypeIndex < chain.length; prototypeIndex++) {");
            stringBuilder.AppendLine(Indent(2) + "let chainPrototype = chain[prototypeIndex];");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(2) + "if (chainPrototype === Boolean.prototype) {");
            stringBuilder.AppendLine(Indent(3) + "return window.BlazorJavascript.typeBuiltInBoolean;");
            stringBuilder.AppendLine(Indent(2) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(2) + "if (chainPrototype === Function.prototype) {");
            stringBuilder.AppendLine(Indent(3) + "return window.BlazorJavascript.typeBuiltInFunction;");
            stringBuilder.AppendLine(Indent(2) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(2) + "if (chainPrototype === Number.prototype) {");
            stringBuilder.AppendLine(Indent(3) + "return window.BlazorJavascript.typeBuiltInNumber;");
            stringBuilder.AppendLine(Indent(2) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(2) + "if (chainPrototype === String.prototype) {");
            stringBuilder.AppendLine(Indent(3) + "return window.BlazorJavascript.typeBuiltInString;");
            stringBuilder.AppendLine(Indent(2) + "}");

            foreach (var prototypeInfo in prototypes)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(2) + $"if (typeof {prototypeInfo.GlobalVariableInfo.Name} !== 'undefined' && chainPrototype === {prototypeInfo.GlobalVariableInfo.Name}.prototype) {{");
                stringBuilder.AppendLine(Indent(3) + $"return window.BlazorJavascript.typePrototype{prototypeInfo.InterfaceInfo.Name};");
                stringBuilder.AppendLine(Indent(2) + "}");
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(1) + "return window.BlazorJavascript.typeBuiltInObject;"); ;

            stringBuilder.AppendLine("};");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("window['__blazorJavascript_obtainPrototype'] = window.BlazorJavascript.obtainPrototype;");

            return stringBuilder.ToString();
        }

        private ImmutableList<MethodInfo> GetMethodsFromInterfaceBody(InterfaceBodyInfo interfaceBodyInfo)
        {
            var methods = new List<MethodInfo>();

            foreach (var methodInfo in interfaceBodyInfo.Methods)
            {
                // FIXME: We are skipping any methods that are not simple enough for a 1 to 1 translation.
                //        For example, nothing with generics, union types, intersection types, or function parameters.
                if (methodInfo.ExtractTypeParametersResult.TypeParameters.Any())
                {
                    continue;
                }

                if (IsFinalTypeSimpleEnoughToRender(methodInfo.ReturnType))
                {
                    continue;
                }

                if (methodInfo.Parameters.Any(parameterInfo => IsFinalTypeSimpleEnoughToRender(parameterInfo.Type)))
                {
                    continue;
                }

                methods.Add(methodInfo);
            }

            return methods.ToImmutableList();
        }

        public string GenerateObjectFactoryFileContents(ImmutableList<(InterfaceInfo InterfaceInfo, GlobalVariableInfo GlobalVariableInfo)> prototypes)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using Microsoft.JSInterop;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.BuiltIns;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Prototypes;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace RealGoodApps.BlazorJavascript.Interop.Factories");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(Indent(1) + "public static class JSObjectFactory");
            stringBuilder.AppendLine(Indent(1) + "{");
            stringBuilder.AppendLine(Indent(2) + "public static IJSObject FromRuntimeObjectReference(IJSInProcessRuntime jsInProcessRuntime, IJSObjectReference objectReference)");
            stringBuilder.AppendLine(Indent(2) + "{");
            stringBuilder.AppendLine(Indent(3) + "var prototype = jsInProcessRuntime.Invoke<int>(\"__blazorJavascript_obtainPrototype\", objectReference);");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(3) + "switch (prototype)");
            stringBuilder.AppendLine(Indent(3) + "{");
            stringBuilder.AppendLine(Indent(4) + "case 0:");
            stringBuilder.AppendLine(Indent(5) + "return null;");
            stringBuilder.AppendLine(Indent(4) + "case 1:");
            stringBuilder.AppendLine(Indent(5) + "return new JSUndefined(jsInProcessRuntime);");
            stringBuilder.AppendLine(Indent(3) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(3) + "var objectReferenceNotNull = objectReference!;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(3) + "return prototype switch");
            stringBuilder.AppendLine(Indent(3) + "{");
            stringBuilder.AppendLine(Indent(4) + $"{TypeIdentifiers.TypeIdentifier.Number.ToInteger()} => new JSNumber(jsInProcessRuntime, objectReferenceNotNull),");
            stringBuilder.AppendLine(Indent(4) + $"{TypeIdentifiers.TypeIdentifier.String.ToInteger()} => new JSString(jsInProcessRuntime, objectReferenceNotNull),");
            stringBuilder.AppendLine(Indent(4) + $"{TypeIdentifiers.TypeIdentifier.Function.ToInteger()} => new JSFunction(jsInProcessRuntime, objectReferenceNotNull),");
            stringBuilder.AppendLine(Indent(4) + $"{TypeIdentifiers.TypeIdentifier.Boolean.ToInteger()} => new JSBoolean(jsInProcessRuntime, objectReferenceNotNull),");

            var prototypeTypeIdentifier = TypeIdentifiers.GetPredefinedTypeIdentifiers().Last().ToInteger() + 1;

            foreach (var prototypeInfo in prototypes)
            {
                // FIXME: This makes me sad, but I don't think we'll be able to infer any better than this :(
                var typeParametersString = ExtractTypeParametersStringForPrototypeConstructorDispatch(prototypeInfo.InterfaceInfo);
                stringBuilder.AppendLine(Indent(4) + $"{prototypeTypeIdentifier} => new {prototypeInfo.InterfaceInfo.Name}Prototype{typeParametersString}(jsInProcessRuntime, objectReferenceNotNull),");
                prototypeTypeIdentifier++;
            }

            stringBuilder.AppendLine(Indent(4) + "_ => new JSObject(jsInProcessRuntime, objectReferenceNotNull),");
            stringBuilder.AppendLine(Indent(3) + "};");
            stringBuilder.AppendLine(Indent(2) + "}");
            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private ImmutableList<PropertyInfo> GetPropertiesFromInterfaceBody(
            InterfaceBodyInfo interfaceBodyInfo,
            ExtractTypeParametersResult? extractTypeParametersResult)
        {
            if (extractTypeParametersResult != null && extractTypeParametersResult.TypeParameters.Any())
            {
                return ImmutableList.Create<PropertyInfo>();
            }

            var properties = new List<PropertyInfo>();

            foreach (var propertyInfo in interfaceBodyInfo.Properties)
            {
                // FIXME: We are skipping any properties that are not simple enough for a 1 to 1 translation.
                //        For example, nothing with generics, union types, intersection types, or function parameters.
                if (IsFinalTypeSimpleEnoughToRender(propertyInfo.Type))
                {
                    continue;
                }

                properties.Add(propertyInfo);
            }

            return properties.ToImmutableList();
        }
    }
}
