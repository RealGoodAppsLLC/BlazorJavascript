using RealGoodApps.ValueImmutableCollections;
using System.Text;
using Newtonsoft.Json;
using RealGoodApps.BlazorJavascript.CodeGenerator.Models;

namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public class CodeGenerator
    {
        private const int MaxGenericParameterCount = 16;

        private readonly ParsedInfo _parsedInfo;
        private readonly string _outputDirectory;

        public CodeGenerator(
            ParsedInfo parsedInfo,
            string outputDirectory)
        {
            _parsedInfo = parsedInfo;
            _outputDirectory = outputDirectory;
        }

        public int InterfaceCount { get; private set; }
        public int GlobalCount { get; private set; }
        public int PrototypeCount { get; private set; }
        public int ConstructorImplementationCount { get; private set; }
        public int MethodImplementationCount { get; private set; }
        public int PropertyImplementationCount { get; private set; }
        public int InterfaceConstructorCount { get; private set; }
        public int InterfaceMethodCount { get; private set; }
        public int InterfacePropertyCount { get; private set; }
        public int AppendedGlobalsCount { get; private set; }

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

                    if (globalVariable.Type != null)
                    {
                        var processedFinalTypeInfo = ProcessTypeAliasesAndRewriteNulls(globalVariable.Type);

                        if (!IsFinalTypeTooComplexToRender(processedFinalTypeInfo))
                        {
                            var typeInterface = _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == processedFinalTypeInfo.TypeInfo.Single?.Name);

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
                        ValueImmutableList.Create<TypeInfo>(),
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

            var javascriptContents = GenerateJavascriptFileContents(prototypes.ToValueImmutableList());

            var javascriptOutputPath = Path.Combine(
                _outputDirectory,
                "Javascript",
                "script.js");

            if (File.Exists(javascriptOutputPath))
            {
                throw new Exception($"File already exists: {javascriptOutputPath}");
            }

            File.WriteAllText(javascriptOutputPath, javascriptContents);

            var objectFactoryContents = GenerateObjectFactoryFileContents(prototypes.ToValueImmutableList());

            var objectFactoryOutputPath = Path.Combine(
                _outputDirectory,
                "Factories",
                "JSObjectFactory.cs");

            if (File.Exists(objectFactoryOutputPath))
            {
                throw new Exception($"File already exists: {objectFactoryOutputPath}");
            }

            File.WriteAllText(objectFactoryOutputPath, objectFactoryContents);

            var jsFunctionContents = GenerateJSFunctionFileContents();

            var jsFunctionOutputPath = Path.Combine(
                _outputDirectory,
                "BuiltIns",
                "JSFunction.cs");

            if (File.Exists(jsFunctionOutputPath))
            {
                throw new Exception($"File already exists: {jsFunctionOutputPath}");
            }

            File.WriteAllText(jsFunctionOutputPath, jsFunctionContents);

            var jsObjectExtensionsContents = GenerateJSObjectExtensionsFileContents();

            var jsObjectExtensionsOutputPath = Path.Combine(
                _outputDirectory,
                "Extensions",
                "IJSObjectExtensions.cs");

            if (File.Exists(jsObjectExtensionsOutputPath))
            {
                throw new Exception($"File already exists: {jsObjectExtensionsOutputPath}");
            }

            File.WriteAllText(jsObjectExtensionsOutputPath, jsObjectExtensionsContents);
        }

        private static string GenerateJSObjectExtensionsFileContents()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using Microsoft.JSInterop;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.BuiltIns;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Extensions;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Factories;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.GlobalVariables;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Prototypes;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace RealGoodApps.BlazorJavascript.Interop.Extensions");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(Indent(1) + "public static partial class IJSObjectExtensions");
            stringBuilder.AppendLine(Indent(1) + "{");

            var isFirst = true;

            for (var typeArgumentCount = 0; typeArgumentCount <= MaxGenericParameterCount; typeArgumentCount++)
            {
                if (!isFirst)
                {
                    stringBuilder.AppendLine();
                }

                var typeArgumentsString = GenerateTypeArgumentsForGenericCalls(typeArgumentCount);

                stringBuilder.AppendLine(Indent(2) + $"public static IJSObject CallConstructor{typeArgumentsString}(this IJSObject self, params object[] args)");
                stringBuilder.Append(GenerateWhereClassesForGenericArguments(typeArgumentCount));
                stringBuilder.AppendLine(Indent(2) + "{");
                stringBuilder.AppendLine(Indent(3) + "if (self is JSUndefined)");
                stringBuilder.AppendLine(Indent(3) + "{");
                stringBuilder.AppendLine(Indent(4) + "return self;");
                stringBuilder.AppendLine(Indent(3) + "}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(3) + "var result = CallConstructorInternal(self, args);");
                stringBuilder.AppendLine(Indent(3) + $"return JSObjectFactory.FromRuntimeObjectReference{typeArgumentsString}(self.Runtime, result);");
                stringBuilder.AppendLine(Indent(2) + "}");
                isFirst = false;
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private string GenerateInterfaceFileContents(
            string interfaceName,
            ExtractTypeParametersResult? extractTypeParametersResult,
            ValueImmutableList<TypeInfo> interfaceExtendsList,
            InterfaceBodyInfo interfaceBodyInfo,
            bool isGlobalThis)
        {
            InterfaceCount++;

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
                .Select(extendedTypeInfo => GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(extendedTypeInfo)))
                .Append("IJSObject")
                .ToValueImmutableList();
            stringBuilder.Append($" : {string.Join(", ", extendsList)}");
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.AppendLine(Indent(1) + "{");

            // We only want constructors that come from the interface body, ignoring the extends list.
            var constructors = GetConstructorsFromInterfaceBody(
                interfaceBodyInfo,
                extractTypeParametersResult,
                ValueImmutableList.Create<TypeInfo>(),
                ValueImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (_, constructorInfo) in constructors)
            {
                InterfaceConstructorCount++;

                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                RenderConstructorBeginning(stringBuilder, constructorInfo, string.Empty);
                stringBuilder.Append(';');
                stringBuilder.Append(Environment.NewLine);
            }

            // We only want methods that come from the interface body, ignoring the extends list.
            var methods = GetMethodsFromInterfaceBody(
                interfaceBodyInfo,
                extractTypeParametersResult,
                ValueImmutableList.Create<TypeInfo>(),
                ValueImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (_, methodInfo) in methods)
            {
                InterfaceMethodCount++;

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
                ValueImmutableList.Create<TypeInfo>(),
                ValueImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (_, propertyInfo) in properties)
            {
                InterfacePropertyCount++;

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
                AppendedGlobalsCount++;

                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));
                stringBuilder.Append(globalDefinedOutside.InterfaceTypeName);
                stringBuilder.Append(' ');
                stringBuilder.Append(globalDefinedOutside.GlobalVariableInfo.Name);
                stringBuilder.Append(" { get; }");
                stringBuilder.Append(Environment.NewLine);
            }
        }

        private string GenerateJSFunctionFileContents()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("/// <auto-generated />");
            stringBuilder.AppendLine("using Microsoft.JSInterop;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.BuiltIns;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Factories;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.GlobalVariables;");
            stringBuilder.AppendLine("using RealGoodApps.BlazorJavascript.Interop.Prototypes;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(Indent(1) + "public sealed partial class JSFunction");
            stringBuilder.AppendLine(Indent(1) + "{");

            var isFirst = true;

            for (var typeArgumentCount = 0; typeArgumentCount <= MaxGenericParameterCount; typeArgumentCount++)
            {
                if (!isFirst)
                {
                    stringBuilder.AppendLine();
                }

                var typeArgumentsString = GenerateTypeArgumentsForGenericCalls(typeArgumentCount);

                stringBuilder.AppendLine(Indent(2) + $"public IJSObject Invoke{typeArgumentsString}(IJSObject thisObject, params object[] args)");
                stringBuilder.Append(GenerateWhereClassesForGenericArguments(typeArgumentCount));
                stringBuilder.AppendLine(Indent(2) + "{");
                stringBuilder.AppendLine(Indent(3) + "var result = InvokeInternal(thisObject, args);");
                stringBuilder.AppendLine(Indent(3) + $"return JSObjectFactory.FromRuntimeObjectReference{typeArgumentsString}(Runtime, result);");
                stringBuilder.AppendLine(Indent(2) + "}");
                isFirst = false;
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
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

        private void RenderConstructorBeginning(
            StringBuilder stringBuilder,
            ConstructorInfo constructorInfo,
            string prefixTypeName)
        {
            stringBuilder.Append(GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(constructorInfo.ReturnType)));
            stringBuilder.Append(' ');

            if (!string.IsNullOrWhiteSpace(prefixTypeName))
            {
                stringBuilder.Append($"{prefixTypeName}.");
            }

            stringBuilder.Append(constructorInfo.GetNameForCSharp());

            stringBuilder.Append('(');

            var isFirst = true;

            foreach (var parameterInfo in constructorInfo.Parameters)
            {
                if (!isFirst)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(parameterInfo.Type)));
                stringBuilder.Append(' ');
                stringBuilder.Append(parameterInfo.GetNameForCSharp());
                isFirst = false;
            }

            stringBuilder.Append(')');
        }

        private void RenderMethodBeginning(
            StringBuilder stringBuilder,
            MethodInfo methodInfo,
            string prefixTypeName)
        {
            stringBuilder.Append(GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(methodInfo.ReturnType)));
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

                stringBuilder.Append(GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(parameterInfo.Type)));
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
            stringBuilder.Append(GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(propertyInfo.Type)));
            stringBuilder.Append(' ');

            if (!string.IsNullOrWhiteSpace(prefixTypeName))
            {
                stringBuilder.Append($"{prefixTypeName}.");
            }

            stringBuilder.Append(propertyInfo.GetNameForCSharp());
        }

        private ValueImmutableList<(InterfaceInfo? OwnerInterface, MethodInfo MethodInfo)> GetMethodsFromInterfaceBody(
            InterfaceBodyInfo interfaceBodyInfo,
            ExtractTypeParametersResult? extractTypeParametersResult,
            ValueImmutableList<TypeInfo> extendsList,
            ValueImmutableList<InterfaceInfo> alreadyProcessedInterfaces,
            InterfaceInfo? ownerInterface)
        {
            var methods = new List<(InterfaceInfo? InterfaceInfo, MethodInfo MethodInfo)>();

            if (extendsList.Any())
            {
                foreach (var extendTypeInfo in extendsList)
                {
                    // FIXME: I am probably missing something, but when we process type aliases here it causes problems in detecting methods.
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
                        extendInterfaceInfo.ExtractTypeParametersResult,
                        extendInterfaceInfo.ExtendsList,
                        alreadyProcessedInterfaces,
                        extendInterfaceInfo));
                }
            }

            if (extractTypeParametersResult != null && extractTypeParametersResult.TypeParameters.Any())
            {
                return methods.ToValueImmutableList();
            }

            foreach (var methodInfo in interfaceBodyInfo.Methods)
            {
                // FIXME: We are skipping any methods that are not simple enough for a 1 to 1 translation.
                //        For example, nothing with generics, union types, intersection types, or function parameters.
                if (methodInfo.ExtractTypeParametersResult.TypeParameters.Any())
                {
                    continue;
                }

                if (IsFinalTypeTooComplexToRender(ProcessTypeAliasesAndRewriteNulls(methodInfo.ReturnType)))
                {
                    continue;
                }

                if (methodInfo.Parameters.Any(parameterInfo => IsFinalTypeTooComplexToRender(ProcessTypeAliasesAndRewriteNulls(parameterInfo.Type))))
                {
                    continue;
                }

                methods.Add((ownerInterface, methodInfo));
            }

            return methods.ToValueImmutableList();
        }

        private ValueImmutableList<(InterfaceInfo? OwnerInterface, ConstructorInfo ConstructorInfo)> GetConstructorsFromInterfaceBody(
            InterfaceBodyInfo interfaceBodyInfo,
            ExtractTypeParametersResult? extractTypeParametersResult,
            ValueImmutableList<TypeInfo> extendsList,
            ValueImmutableList<InterfaceInfo> alreadyProcessedInterfaces,
            InterfaceInfo? ownerInterface)
        {
            var constructors = new List<(InterfaceInfo? InterfaceInfo, ConstructorInfo ConstructorInfo)>();

            if (extendsList.Any())
            {
                foreach (var extendTypeInfo in extendsList)
                {
                    // FIXME: I am probably missing something, but when we process type aliases here it causes problems in detecting methods.
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

                    constructors.AddRange(GetConstructorsFromInterfaceBody(
                        extendInterfaceInfo.Body,
                        extendInterfaceInfo.ExtractTypeParametersResult,
                        extendInterfaceInfo.ExtendsList,
                        alreadyProcessedInterfaces,
                        extendInterfaceInfo));
                }
            }

            if (extractTypeParametersResult != null && extractTypeParametersResult.TypeParameters.Any())
            {
                return constructors.ToValueImmutableList();
            }

            foreach (var constructorInfo in interfaceBodyInfo.Constructors)
            {
                // FIXME: We are skipping any constructors that are not simple enough for a 1 to 1 translation.
                //        For example, nothing with generics, union types, intersection types, or function parameters.
                if (constructorInfo.ExtractTypeParametersResult.TypeParameters.Any())
                {
                    continue;
                }

                if (IsFinalTypeTooComplexToRender(ProcessTypeAliasesAndRewriteNulls(constructorInfo.ReturnType)))
                {
                    continue;
                }

                if (constructorInfo.Parameters.Any(parameterInfo => IsFinalTypeTooComplexToRender(ProcessTypeAliasesAndRewriteNulls(parameterInfo.Type))))
                {
                    continue;
                }

                constructors.Add((ownerInterface, constructorInfo));
            }

            return constructors.ToValueImmutableList();
        }

        private ValueImmutableList<(InterfaceInfo? OwnerInterface, PropertyInfo PropertyInfo)> GetPropertiesFromInterfaceBody(
            InterfaceBodyInfo interfaceBodyInfo,
            ExtractTypeParametersResult? extractTypeParametersResult,
            ValueImmutableList<TypeInfo> extendsList,
            ValueImmutableList<InterfaceInfo> alreadyProcessedInterfaces,
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
                return properties.ToValueImmutableList();
            }

            foreach (var propertyInfo in interfaceBodyInfo.Properties)
            {
                // FIXME: We are skipping any properties that are not simple enough for a 1 to 1 translation.
                //        For example, nothing with generics, union types, intersection types, or function parameters.
                if (IsFinalTypeTooComplexToRender(ProcessTypeAliasesAndRewriteNulls(propertyInfo.Type)))
                {
                    continue;
                }

                properties.Add((ownerInterface, propertyInfo));
            }

            return properties.ToValueImmutableList();
        }

        private ValueImmutableList<GetAccessorInfo> GetGetAccessorsFromInterfaceRecursively(InterfaceInfo interfaceInfo)
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
            return allGetAccessors.ToValueImmutableList();
        }

        private string GenerateGlobalVariableFileContents(GlobalDefinedOutsideOfGlobalThisInterface globalDefinedOutside)
        {
            GlobalCount++;

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
            PrototypeCount++;

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
            ValueImmutableList<TypeInfo> extendsList)
        {
            var stringBuilder = new StringBuilder();

            var constructors = GetConstructorsFromInterfaceBody(
                interfaceBodyInfo,
                extractTypeParametersResult,
                extendsList,
                ValueImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (constructorInterfaceInfo, constructorInfo) in constructors)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));

                var prefix = constructorInterfaceInfo == null
                    ? defaultTypePrefix
                    : GetPrefixTypeNameForInterfaceSymbolImplementations(constructorInterfaceInfo);

                ConstructorImplementationCount++;

                RenderConstructorBeginning(stringBuilder, constructorInfo, prefix);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Indent(2) + "{");
                stringBuilder.Append(Environment.NewLine);

                var parametersString = string.Join(", ", constructorInfo.Parameters.Select(p => p.GetNameForCSharp()));
                var processedReturnType = ProcessTypeAliasesAndRewriteNulls(constructorInfo.ReturnType);
                var returnRenderedTypeName = GetRenderedTypeName(processedReturnType);

                var invokeGenericArguments = GenerateInvokeGenericArguments(processedReturnType);

                stringBuilder.AppendLine(Indent(3) + $"var resultObj = this.CallConstructor{invokeGenericArguments}({parametersString});");

                stringBuilder.AppendLine(Indent(3) + $"var resultAsType = resultObj as {returnRenderedTypeName};");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(3) + "if (resultAsType == null)");
                stringBuilder.AppendLine(Indent(3) + "{");
                stringBuilder.AppendLine(Indent(4) + "throw new InvalidCastException(\"Return value is no good.\");");
                stringBuilder.AppendLine(Indent(3) + "}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(3) + "return resultAsType;");

                stringBuilder.Append(Indent(2) + "}");
                stringBuilder.Append(Environment.NewLine);
            }

            var methods = GetMethodsFromInterfaceBody(
                interfaceBodyInfo,
                extractTypeParametersResult,
                extendsList,
                ValueImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (methodInterfaceInfo, methodInfo) in methods)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));

                var prefix = methodInterfaceInfo == null
                    ? defaultTypePrefix
                    : GetPrefixTypeNameForInterfaceSymbolImplementations(methodInterfaceInfo);

                MethodImplementationCount++;

                RenderMethodBeginning(stringBuilder, methodInfo, prefix);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Indent(2) + "{");
                stringBuilder.Append(Environment.NewLine);

                var parametersString = string.Join(", ", methodInfo.Parameters.Select(p => p.GetNameForCSharp()));
                var parametersPrefix = string.IsNullOrWhiteSpace(parametersString) ? string.Empty : ", ";
                var processedReturnType = ProcessTypeAliasesAndRewriteNulls(methodInfo.ReturnType);
                var returnRenderedTypeName = GetRenderedTypeName(processedReturnType);

                var invokeGenericArguments = GenerateInvokeGenericArguments(processedReturnType);

                stringBuilder.AppendLine(Indent(3) + $"var propertyObj = this.GetPropertyOfObject(\"{methodInfo.Name}\");");
                stringBuilder.AppendLine(Indent(3) + "var propertyAsFunction = propertyObj as JSFunction;");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(3) + "if (propertyAsFunction == null)");
                stringBuilder.AppendLine(Indent(3) + "{");
                stringBuilder.AppendLine(Indent(4) + "throw new InvalidCastException(\"Something went wrong!\");");
                stringBuilder.AppendLine(Indent(3) + "}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Indent(3) + $"var resultObj = propertyAsFunction.Invoke{invokeGenericArguments}(this{parametersPrefix}{parametersString});");

                if (returnRenderedTypeName != "void")
                {
                    stringBuilder.AppendLine(Indent(3) + $"var resultAsType = resultObj as {returnRenderedTypeName};");
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
                ValueImmutableList.Create<InterfaceInfo>(),
                null);

            foreach (var (propertyInterfaceInfo, propertyInfo) in properties)
            {
                // FIXME: It would be nice to carry over any comments from the TypeScript definitions.
                stringBuilder.Append(Indent(2));

                var prefix = propertyInterfaceInfo == null
                    ? defaultTypePrefix
                    : GetPrefixTypeNameForInterfaceSymbolImplementations(propertyInterfaceInfo);

                PropertyImplementationCount++;

                RenderPropertyBeginning(stringBuilder, propertyInfo, prefix);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.AppendLine(Indent(2) + "{");

                var returnRenderedTypeName = GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(propertyInfo.Type));

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

        private string GenerateInvokeGenericArguments(ProcessedTypeInfo processedReturnType)
        {
            var invokeGenericArgumentsStringBuilder = new StringBuilder();

            if (processedReturnType.TypeInfo.Array != null)
            {
                invokeGenericArgumentsStringBuilder.Append('<');
                invokeGenericArgumentsStringBuilder.Append(
                    GetRenderedTypeName(new ProcessedTypeInfo(processedReturnType.TypeInfo.Array)));
                invokeGenericArgumentsStringBuilder.Append('>');
            }

            var invokeGenericArguments = invokeGenericArgumentsStringBuilder.ToString();
            return invokeGenericArguments;
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

        private bool IsFinalTypeTooComplexToRender(ProcessedTypeInfo parameterInfoType)
        {
            // FIXME: Eventually, this method shouldn't really exist. It is just used to prevent us from having to handle complex type cases right now.
            if (parameterInfoType.TypeInfo.Array != null)
            {
                // We know that the array must have been processed too, so this is safe.
                return IsFinalTypeTooComplexToRender(new ProcessedTypeInfo(parameterInfoType.TypeInfo.Array));
            }

            return parameterInfoType.TypeInfo.Single == null
                   || parameterInfoType.TypeInfo.Single.IsUnhandled
                   || parameterInfoType.TypeInfo.Single.TypeArguments.Any()
                   || string.IsNullOrWhiteSpace(parameterInfoType.TypeInfo.Single.Name);
        }

        private string GetRenderedTypeName(ProcessedTypeInfo processedTypeInfo)
        {
            if (processedTypeInfo.TypeInfo.Array != null)
            {
                var fullArrayName = new StringBuilder();
                fullArrayName.Append("IJSArray");

                fullArrayName.Append('<');
                fullArrayName.Append(GetRenderedTypeName(new ProcessedTypeInfo(processedTypeInfo.TypeInfo.Array)));
                fullArrayName.Append('>');

                return fullArrayName.ToString();
            }

            var singleTypeInfo = processedTypeInfo.TypeInfo.Single;

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
                .ToValueImmutableList();

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
                            : GetRenderedTypeName(ProcessTypeAliasesAndRewriteNulls(typeParameter.Default)));

                        isFirst = false;
                    }

                    fullName.Append('>');
                }
            }

            return fullName.ToString();
        }

        ProcessedTypeInfo ProcessTypeAliasesAndRewriteNulls(TypeInfo typeInfo)
        {
            typeInfo = RewriteNullsForUnion(typeInfo);

            // Process arrays!
            if (typeInfo.Array != null)
            {
                // FIXME: Does it make sense to convert `ProcessTypeAliasesAndRewriteNulls` into a public and
                //        internal version to prevent this awkwardness?
                var processedArrayItemType = ProcessTypeAliasesAndRewriteNulls(typeInfo.Array);

                typeInfo = typeInfo with
                {
                    Array = processedArrayItemType.TypeInfo,
                };
            }

            // Anything that looks like a single type is a candidate for being an alias.
            if (typeInfo.Single == null)
            {
                return new ProcessedTypeInfo(typeInfo);
            }

            var anyTypeAliases = false;
            while (true)
            {
                // FIXME: We are assuming that you can only type alias the most simple case for now.
                var typeAlias = _parsedInfo.TypeAliases
                    .FirstOrDefault(typeAlias => typeInfo.Single != null && typeAlias.Name == typeInfo.Single.Name);

                if (typeAlias == null)
                {
                    return anyTypeAliases
                        ? ProcessTypeAliasesAndRewriteNulls(typeInfo)
                        : new ProcessedTypeInfo(typeInfo);
                }

                typeInfo = typeAlias.AliasType;
                anyTypeAliases = true;
            }
        }

        private TypeInfo RewriteNullsForUnion(TypeInfo typeInfo)
        {
            if (typeInfo.Union == null)
            {
                return typeInfo;
            }

            var finalTypeList = new List<TypeInfo>();

            foreach (var typeWithinUnion in typeInfo.Union.Types)
            {
                if (typeWithinUnion.Single == null
                    || typeWithinUnion.Single.Name != "null")
                {
                    finalTypeList.Add(typeWithinUnion);
                }
            }

            if (finalTypeList.Count == 1)
            {
                return finalTypeList.First();
            }

            return new TypeInfo(
                new UnionTypeInfo(finalTypeList.ToValueImmutableList()),
                null,
                null,
                null,
                null,
                null);
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

        private string GenerateJavascriptFileContents(ValueImmutableList<(InterfaceInfo InterfaceInfo, GlobalVariableInfo GlobalVariableInfo)> prototypes)
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
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Indent(2) + "if (chainPrototype === Array.prototype) {");
            stringBuilder.AppendLine(Indent(3) + "return window.BlazorJavascript.typeBuiltInArray;");
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

        public string GenerateObjectFactoryFileContents(ValueImmutableList<(InterfaceInfo InterfaceInfo, GlobalVariableInfo GlobalVariableInfo)> prototypes)
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

            for (var argCount = 0; argCount <= MaxGenericParameterCount; argCount++)
            {
                stringBuilder.Append(GenerateObjectFactoryOverload(prototypes, argCount));
            }

            stringBuilder.AppendLine(Indent(1) + "}");
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        private string GenerateObjectFactoryOverload(
            ValueImmutableList<(InterfaceInfo InterfaceInfo, GlobalVariableInfo GlobalVariableInfo)> prototypes,
            int genericArgumentCount)
        {
            var stringBuilder = new StringBuilder();

            var genericString = GenerateTypeArgumentsForGenericCalls(genericArgumentCount);

            stringBuilder.AppendLine(Indent(2) + $"public static IJSObject FromRuntimeObjectReference{genericString}(IJSInProcessRuntime jsInProcessRuntime, IJSObjectReference objectReference)");

            stringBuilder.Append(GenerateWhereClassesForGenericArguments(genericArgumentCount));

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

            if (genericArgumentCount == 0)
            {
                stringBuilder.AppendLine(Indent(4) + $"{TypeIdentifiers.TypeIdentifier.Array.ToInteger()} => new JSArray(jsInProcessRuntime, objectReferenceNotNull),");
            }
            else if (genericArgumentCount == 1)
            {
                stringBuilder.AppendLine(Indent(4) + $"{TypeIdentifiers.TypeIdentifier.Array.ToInteger()} => new JSArray{genericString}(jsInProcessRuntime, objectReferenceNotNull),");
            }

            var prototypeTypeIdentifier = TypeIdentifiers.GetPredefinedTypeIdentifiers().Last().ToInteger() + 1;

            foreach (var prototypeInfo in prototypes)
            {
                var typeParametersString = genericArgumentCount == 0
                    ? ExtractTypeParametersStringForPrototypeConstructorDispatch(prototypeInfo.InterfaceInfo)
                    : genericString;

                // FIXME: We are cheating here because `ExtractTypeParametersStringForPrototypeConstructorDispatch` is too tightly coupled.
                //        Ideally, we would actually inline that logic and only run it if there was 0 generic arguments.
                if (genericArgumentCount < 1
                    || (prototypeInfo.InterfaceInfo.ExtractTypeParametersResult.TypeParameters.Count == genericArgumentCount))
                {
                    stringBuilder.AppendLine(Indent(4) + $"{prototypeTypeIdentifier} => new {prototypeInfo.InterfaceInfo.Name}Prototype{typeParametersString}(jsInProcessRuntime, objectReferenceNotNull),");
                }

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

            return stringBuilder.ToString();
        }

        private static string GenerateWhereClassesForGenericArguments(int genericArgumentCount)
        {
            if (genericArgumentCount < 1)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            for (var genericArgumentIndex = 0; genericArgumentIndex < genericArgumentCount; genericArgumentIndex++)
            {
                stringBuilder.AppendLine(Indent(3) + $"where TJSObject{genericArgumentIndex} : class, IJSObject");
            }

            return stringBuilder.ToString();
        }

        private static string GenerateTypeArgumentsForGenericCalls(int genericArgumentCount)
        {
            var genericStringBuilder = new StringBuilder();

            if (genericArgumentCount > 0)
            {
                genericStringBuilder.Append('<');
                var isFirst = true;

                for (var genericArgumentIndex = 0; genericArgumentIndex < genericArgumentCount; genericArgumentIndex++)
                {
                    if (!isFirst)
                    {
                        genericStringBuilder.Append(',');
                    }

                    genericStringBuilder.Append($"TJSObject{genericArgumentIndex}");
                    isFirst = false;
                }

                genericStringBuilder.Append('>');
            }

            return genericStringBuilder.ToString();
        }

        public sealed record GlobalDefinedOutsideOfGlobalThisInterface(
            GlobalVariableInfo GlobalVariableInfo,
            string InterfaceTypeName,
            InterfaceBodyInfo InterfaceBodyInfo,
            ExtractTypeParametersResult? ExtractTypeParametersResult,
            ValueImmutableList<TypeInfo> ExtendsList);

        private ValueImmutableList<GlobalDefinedOutsideOfGlobalThisInterface> GetGlobalsDefinedOutsideOfGlobalThisInterface()
        {
            var globalThisInterface = _parsedInfo.Interfaces.First(interfaceInfo => interfaceInfo.Name == GetGlobalThisInterfaceName());
            var allProperties = GetPropertiesFromInterfaceBody(
                globalThisInterface.Body,
                globalThisInterface.ExtractTypeParametersResult,
                globalThisInterface.ExtendsList,
                ValueImmutableList.Create<InterfaceInfo>(),
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
                        ValueImmutableList.Create<TypeInfo>()));

                    continue;
                }

                if (globalVariableInfo.Type == null)
                {
                    continue;
                }

                var processedFinalTypeInfo = ProcessTypeAliasesAndRewriteNulls(globalVariableInfo.Type);

                if (IsFinalTypeTooComplexToRender(processedFinalTypeInfo))
                {
                    continue;
                }

                var globalInterfaceType = _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == processedFinalTypeInfo.TypeInfo.Single?.Name);

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

            return result.ToValueImmutableList();
        }

        private string GetGlobalThisInterfaceName()
        {
            // FIXME: Right now, we know the globalThis is a `Window`, but we might not want to assume this
            //        in the future, especially if this code is used to generate bindings for libraries.
            return "Window";
        }
    }
}
