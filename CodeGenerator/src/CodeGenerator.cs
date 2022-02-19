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
                    interfaceInfo.Body,
                    interfaceInfo.Name == GetGlobalThisInterfaceName());

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
                        // TODO: Should we process type aliases here?
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
                    var prototypeContents = GeneratePrototypeFileContents(interfaceInfo, interfaceInfo.Name == GetGlobalThisInterfaceName());

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

            var globalsDefinedOutside = GetGlobalsDefinedOutsideOfGlobalThisInterface();

            foreach (var globalDefinedOutside in globalsDefinedOutside)
            {
                var contents = GenerateGlobalVariableFileContents(globalDefinedOutside);

                var globalVariableOutputPath = Path.Combine(
                    _outputDirectory,
                    "Globals",
                    $"{globalDefinedOutside.GlobalVariableInfo.Name}Global.cs");

                if (File.Exists(globalVariableOutputPath))
                {
                    throw new Exception($"File already exists: {globalVariableOutputPath}");
                }

                File.WriteAllText(globalVariableOutputPath, contents);

                if (globalDefinedOutside.GlobalVariableInfo.InlineInterface != null)
                {
                    var inlineInterfaceContents = GenerateInterfaceFileContents(
                        $"{globalDefinedOutside.GlobalVariableInfo.Name}Global",
                        null,
                        ImmutableList.Create<TypeInfo>(),
                        globalDefinedOutside.GlobalVariableInfo.InlineInterface,
                        false);

                    var inlineInterfaceOutputPath = Path.Combine(
                        _outputDirectory,
                        "Interfaces",
                        $"I{globalDefinedOutside.GlobalVariableInfo.Name}Global.cs");

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
            InterfaceBodyInfo interfaceBodyInfo,
            bool isGlobalThis)
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

            // We only want methods that come from the interface body, ignoring the extends list.
            var methods = GetMethodsFromInterfaceBody(
                interfaceBodyInfo,
                ImmutableList.Create<TypeInfo>(),
                ImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (_, methodInfo) in methods)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                RenderMethodBeginning(stringBuilder, methodInfo, string.Empty);
                stringBuilder.Append(';');
                stringBuilder.Append(Environment.NewLine);
            }

            // Similar to above, we only want properties that come from the interface body, ignoring the extends list.
            var properties = GetPropertiesFromInterfaceBody(
                interfaceBodyInfo,
                extractTypeParametersResult,
                ImmutableList.Create<TypeInfo>(),
                ImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (_, propertyInfo) in properties)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                RenderPropertyBeginning(stringBuilder, propertyInfo, string.Empty);
                stringBuilder.Append(" { get; ");
                if (!propertyInfo.IsReadonly)
                {
                    stringBuilder.Append("set; ");
                }

                stringBuilder.Append('}');
                stringBuilder.Append(Environment.NewLine);
            }

            if (isGlobalThis)
            {
                AppendGlobalsToInterface(stringBuilder);
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private void AppendGlobalsToInterface(StringBuilder stringBuilder)
        {
            var globalsDefinedOutside = GetGlobalsDefinedOutsideOfGlobalThisInterface();

            foreach (var globalDefinedOutside in globalsDefinedOutside)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                stringBuilder.Append(globalDefinedOutside.InterfaceTypeName);
                stringBuilder.Append(' ');
                stringBuilder.Append(globalDefinedOutside.GlobalVariableInfo.Name);
                stringBuilder.Append(" { get; }");
                stringBuilder.Append(Environment.NewLine);
            }
        }

        private void AppendGlobalsToPrototype(StringBuilder stringBuilder)
        {
            var globalsDefinedOutside = GetGlobalsDefinedOutsideOfGlobalThisInterface();

            foreach (var globalDefinedOutside in globalsDefinedOutside)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.AppendLine(Indent(2) + $"{globalDefinedOutside.InterfaceTypeName} IWindow.{globalDefinedOutside.GlobalVariableInfo.Name}");
                stringBuilder.AppendLine(Indent(2) + "{");

                GeneratePropertyGetter(
                    stringBuilder,
                    globalDefinedOutside.GlobalVariableInfo.Name,
                    globalDefinedOutside.InterfaceTypeName);

                stringBuilder.AppendLine(Indent(2) + "}");
            }
        }

        private static void GeneratePropertyGetter(
            StringBuilder stringBuilder,
            string propertyName,
            string propertyType)
        {
            stringBuilder.AppendLine(Indent(3) + "get");
            stringBuilder.AppendLine(Indent(3) + "{");

            stringBuilder.AppendLine(Indent(4) + $"var propertyObj = this.GetPropertyOfObject(\"{propertyName}\");");
            stringBuilder.AppendLine(Indent(4) + "if (propertyObj == null)");
            stringBuilder.AppendLine(Indent(4) + "{");
            stringBuilder.AppendLine(Indent(5) + "return null;");
            stringBuilder.AppendLine(Indent(4) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(4) + $"var propertyAsReturnType = propertyObj as {propertyType};");
            stringBuilder.AppendLine(Indent(4) + "if (propertyAsReturnType == null)");
            stringBuilder.AppendLine(Indent(4) + "{");
            stringBuilder.AppendLine(Indent(5) + "throw new InvalidCastException(\"Something went wrong!\");");
            stringBuilder.AppendLine(Indent(4) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(4) + "return propertyAsReturnType;");

            stringBuilder.AppendLine(Indent(3) + "}");
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

        private string GetPrefixTypeNameForInterfaceSymbolImplementations(InterfaceInfo interfaceInfo)
        {
            var stringBuilder = new StringBuilder();

            var typeParametersString = ExtractTypeParametersString(interfaceInfo.ExtractTypeParametersResult);
            stringBuilder.Append($"I{interfaceInfo.Name}{typeParametersString}");

            return stringBuilder.ToString();
        }

        private void RenderMethodBeginning(
            StringBuilder stringBuilder,
            MethodInfo methodInfo,
            string prefixTypeName)
        {
            stringBuilder.Append(GetRenderedTypeName(methodInfo.ReturnType));
            stringBuilder.Append(' ');

            if (!string.IsNullOrWhiteSpace(prefixTypeName))
            {
                stringBuilder.Append($"{prefixTypeName}.");
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
            string prefixTypeName)
        {
            stringBuilder.Append(GetRenderedTypeName(propertyInfo.Type));
            stringBuilder.Append(' ');

            if (!string.IsNullOrWhiteSpace(prefixTypeName))
            {
                stringBuilder.Append($"{prefixTypeName}.");
            }

            stringBuilder.Append(propertyInfo.GetNameForCSharp());
        }

        private ImmutableList<(InterfaceInfo? OwnerInterface, MethodInfo MethodInfo)> GetMethodsFromInterfaceBody(
            InterfaceBodyInfo interfaceBodyInfo,
            ImmutableList<TypeInfo> extendsList,
            ImmutableList<InterfaceInfo> alreadyProcessedInterfaces,
            InterfaceInfo? ownerInterface)
        {
            var methods = new List<(InterfaceInfo? InterfaceInfo, MethodInfo MethodInfo)>();

            if (extendsList.Any())
            {
                foreach (var extendTypeInfo in extendsList)
                {
                    // TODO: Should we process aliases here?
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

                    methods.AddRange(GetMethodsFromInterfaceBody(
                        extendInterfaceInfo.Body,
                        extendInterfaceInfo.ExtendsList,
                        alreadyProcessedInterfaces,
                        extendInterfaceInfo));
                }
            }

            foreach (var methodInfo in interfaceBodyInfo.Methods)
            {
                // FIXME: We are skipping any methods that are not simple enough for a 1 to 1 translation.
                //        For example, nothing with generics, union types, intersection types, or function parameters.
                if (methodInfo.ExtractTypeParametersResult.TypeParameters.Any())
                {
                    continue;
                }

                if (IsFinalTypeTooComplexToRender(methodInfo.ReturnType, out _))
                {
                    continue;
                }

                if (methodInfo.Parameters.Any(parameterInfo => IsFinalTypeTooComplexToRender(parameterInfo.Type, out _)))
                {
                    continue;
                }

                methods.Add((ownerInterface, methodInfo));
            }

            return methods.ToImmutableList();
        }

        private ImmutableList<(InterfaceInfo? OwnerInterface, PropertyInfo PropertyInfo)> GetPropertiesFromInterfaceBody(
            InterfaceBodyInfo interfaceBodyInfo,
            ExtractTypeParametersResult? extractTypeParametersResult,
            ImmutableList<TypeInfo> extendsList,
            ImmutableList<InterfaceInfo> alreadyProcessedInterfaces,
            InterfaceInfo? ownerInterface)
        {
            var properties = new List<(InterfaceInfo? OwnerInterface, PropertyInfo PropertyInfo)>();

            foreach (var extendTypeInfo in extendsList)
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

                properties.AddRange(GetPropertiesFromInterfaceBody(
                    extendInterfaceInfo.Body,
                    extendInterfaceInfo.ExtractTypeParametersResult,
                    extendInterfaceInfo.ExtendsList,
                    alreadyProcessedInterfaces,
                    extendInterfaceInfo));
            }

            if (extractTypeParametersResult != null && extractTypeParametersResult.TypeParameters.Any())
            {
                return properties.ToImmutableList();
            }

            foreach (var propertyInfo in interfaceBodyInfo.Properties)
            {
                // FIXME: We are skipping any properties that are not simple enough for a 1 to 1 translation.
                //        For example, nothing with generics, union types, intersection types, or function parameters.
                if (IsFinalTypeTooComplexToRender(propertyInfo.Type, out _))
                {
                    continue;
                }

                properties.Add((ownerInterface, propertyInfo));
            }

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

        private string GenerateGlobalVariableFileContents(GlobalDefinedOutsideOfGlobalThisInterface globalDefinedOutside)
        {
            var stringBuilder = new StringBuilder();

            var fullGlobalName = $"{globalDefinedOutside.GlobalVariableInfo.Name}Global";

            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using Microsoft.JSInterop;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.BuiltIns;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Extensions;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Interfaces;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Prototypes;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace RealGoodApps.BlazorJavascript.Interop.GlobalVariables");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(Indent(1) + $"public class {fullGlobalName} : {globalDefinedOutside.InterfaceTypeName}, IJSObject");
            stringBuilder.AppendLine(Indent(1) + "{");

            stringBuilder.Append(GenerateJSObjectSubtypeBoilerPlate(fullGlobalName));

            stringBuilder.Append(GenerateInterfaceImplementations(
                globalDefinedOutside.InterfaceTypeName,
                globalDefinedOutside.InterfaceBodyInfo,
                globalDefinedOutside.ExtractTypeParametersResult,
                globalDefinedOutside.ExtendsList));

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

        private string GeneratePrototypeFileContents(InterfaceInfo interfaceInfo, bool isGlobalThis)
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

            stringBuilder.Append(GenerateJSObjectSubtypeBoilerPlate($"{interfaceInfo.Name}Prototype"));

            stringBuilder.Append(GenerateInterfaceImplementations(
                GetPrefixTypeNameForInterfaceSymbolImplementations(interfaceInfo),
                interfaceInfo.Body,
                interfaceInfo.ExtractTypeParametersResult,
                interfaceInfo.ExtendsList));

            if (isGlobalThis)
            {
                AppendGlobalsToPrototype(stringBuilder);
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private string GenerateInterfaceImplementations(
            string defaultTypePrefix,
            InterfaceBodyInfo interfaceBodyInfo,
            ExtractTypeParametersResult? extractTypeParametersResult,
            ImmutableList<TypeInfo> extendsList)
        {
            var stringBuilder = new StringBuilder();

            var methods = GetMethodsFromInterfaceBody(
                interfaceBodyInfo,
                extendsList,
                ImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (methodInterfaceInfo, methodInfo) in methods)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));

                var prefix = methodInterfaceInfo == null
                    ? defaultTypePrefix
                    : GetPrefixTypeNameForInterfaceSymbolImplementations(methodInterfaceInfo);

                RenderMethodBeginning(stringBuilder, methodInfo, prefix);
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

            var properties = GetPropertiesFromInterfaceBody(
                interfaceBodyInfo,
                extractTypeParametersResult,
                extendsList,
                ImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (propertyInterfaceInfo, propertyInfo) in properties)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));

                var prefix = propertyInterfaceInfo == null
                    ? defaultTypePrefix
                    : GetPrefixTypeNameForInterfaceSymbolImplementations(propertyInterfaceInfo);

                RenderPropertyBeginning(stringBuilder, propertyInfo, prefix);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.AppendLine(Indent(2) + "{");

                var returnRenderedTypeName = GetRenderedTypeName(propertyInfo.Type);

                GeneratePropertyGetter(
                    stringBuilder,
                    propertyInfo.Name,
                    returnRenderedTypeName);

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

            return stringBuilder.ToString();
        }

        private static string GenerateJSObjectSubtypeBoilerPlate(string className)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(Indent(2) + $"public {className}(IJSInProcessRuntime jsInProcessRuntime, IJSObjectReference jsObjectReference)");
            stringBuilder.AppendLine(Indent(2) + "{");
            stringBuilder.AppendLine(Indent(3) + "Runtime = jsInProcessRuntime;");
            stringBuilder.AppendLine(Indent(3) + "ObjectReference = jsObjectReference;");
            stringBuilder.AppendLine(Indent(2) + "}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(2) + "public IJSInProcessRuntime Runtime { get; }");
            stringBuilder.AppendLine(Indent(2) + "public IJSObjectReference ObjectReference { get; }");

            return stringBuilder.ToString();
        }

        private bool IsFinalTypeTooComplexToRender(TypeInfo parameterInfoType, out TypeInfo finalTypeInfo)
        {
            // FIXME: Eventually, this method shouldn't really exist. It is just used to prevent us from having to handle complex type cases right now.
            finalTypeInfo = ProcessTypeAliases(parameterInfoType);

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

            var globalsDefinedOutside = GetGlobalsDefinedOutsideOfGlobalThisInterface();

            foreach (var globalDefinedOutside in globalsDefinedOutside)
            {
                stringBuilder.AppendLine($"window.BlazorJavascript.typeGlobal{globalDefinedOutside.GlobalVariableInfo.Name} = {JsonConvert.SerializeObject(prototypeTypeIdentifier)};");
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

            foreach (var globalDefinedOutside in globalsDefinedOutside)
            {
                stringBuilder.AppendLine(Indent(1) + $"if (typeof {globalDefinedOutside.GlobalVariableInfo.Name} !== 'undefined' && p === {globalDefinedOutside.GlobalVariableInfo.Name}) {{");
                stringBuilder.AppendLine(Indent(2) + $"return window.BlazorJavascript.typeGlobal{globalDefinedOutside.GlobalVariableInfo.Name};");
                stringBuilder.AppendLine(Indent(1) + "}");
                stringBuilder.AppendLine();
            }

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

        public string GenerateObjectFactoryFileContents(ImmutableList<(InterfaceInfo InterfaceInfo, GlobalVariableInfo GlobalVariableInfo)> prototypes)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using Microsoft.JSInterop;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.BuiltIns;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.GlobalVariables;");
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

            var globalsDefinedOutside = GetGlobalsDefinedOutsideOfGlobalThisInterface();

            foreach (var globalDefinedOutside in globalsDefinedOutside)
            {
                stringBuilder.AppendLine(Indent(4) + $"{prototypeTypeIdentifier} => new {globalDefinedOutside.GlobalVariableInfo.Name}Global(jsInProcessRuntime, objectReferenceNotNull),");
                prototypeTypeIdentifier++;
            }

            stringBuilder.AppendLine(Indent(4) + "_ => new JSObject(jsInProcessRuntime, objectReferenceNotNull),");
            stringBuilder.AppendLine(Indent(3) + "};");
            stringBuilder.AppendLine(Indent(2) + "}");
            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public sealed record GlobalDefinedOutsideOfGlobalThisInterface(
            GlobalVariableInfo GlobalVariableInfo,
            string InterfaceTypeName,
            InterfaceBodyInfo InterfaceBodyInfo,
            ExtractTypeParametersResult? ExtractTypeParametersResult,
            ImmutableList<TypeInfo> ExtendsList);

        private ImmutableList<GlobalDefinedOutsideOfGlobalThisInterface> GetGlobalsDefinedOutsideOfGlobalThisInterface()
        {
            var globalThisInterface = _parsedInfo.Interfaces.First(interfaceInfo => interfaceInfo.Name == GetGlobalThisInterfaceName());
            var allProperties = GetPropertiesFromInterfaceBody(
                globalThisInterface.Body,
                globalThisInterface.ExtractTypeParametersResult,
                globalThisInterface.ExtendsList,
                ImmutableList.Create<InterfaceInfo>(),
                null);
            var allWindowGetters = GetGetAccessorsFromInterfaceRecursively(globalThisInterface);

            var result = new List<GlobalDefinedOutsideOfGlobalThisInterface>();

            foreach (var globalVariableInfo in _parsedInfo.GlobalVariables)
            {
                // HACK: Let's exclude anything that was already defined in the `Window` interface.
                if (allProperties.Any(propertyDetails => propertyDetails.PropertyInfo.Name == globalVariableInfo.Name)
                    || allWindowGetters.Any(getAccessor => getAccessor.Name == globalVariableInfo.Name))
                {
                    continue;
                }

                if (globalVariableInfo.InlineInterface != null)
                {
                    result.Add(new GlobalDefinedOutsideOfGlobalThisInterface(
                        globalVariableInfo,
                        $"I{globalVariableInfo.Name}Global",
                        globalVariableInfo.InlineInterface,
                        null,
                        ImmutableList.Create<TypeInfo>()));

                    continue;
                }

                if (globalVariableInfo.Type == null
                    || IsFinalTypeTooComplexToRender(globalVariableInfo.Type, out var finalTypeInfo))
                {
                    continue;
                }

                var globalInterfaceType = _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == finalTypeInfo.Single?.Name);

                if (globalInterfaceType == null)
                {
                    continue;
                }

                result.Add(new GlobalDefinedOutsideOfGlobalThisInterface(
                    globalVariableInfo,
                    GetPrefixTypeNameForInterfaceSymbolImplementations(globalInterfaceType),
                    globalInterfaceType.Body,
                    globalInterfaceType.ExtractTypeParametersResult,
                    globalInterfaceType.ExtendsList));
            }

            return result.ToImmutableList();
        }

        private string GetGlobalThisInterfaceName()
        {
            // FIXME: Right now, we know the globalThis is a `Window`, but we might not want to assume this
            //        in the future, especially if this code is used to generate bindings for libraries.
            return "Window";
        }
    }
}
