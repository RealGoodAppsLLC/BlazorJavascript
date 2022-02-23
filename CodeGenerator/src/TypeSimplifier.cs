using RealGoodApps.BlazorJavascript.CodeGenerator.Models;
using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator
{
    public sealed class TypeSimplifier
    {
        private readonly ParsedInfo _parsedInfo;

        public TypeSimplifier(ParsedInfo parsedInfo)
        {
            _parsedInfo = parsedInfo;
        }

        /// <summary>
        /// Simplify all the types inside the parsed information to make it simpler to process.
        /// After doing this, you really don't need the type aliases anymore.
        ///
        /// It might make sense to return a different model here (instead of ParsedInfo) to strongly encode that
        /// we have effectively simplified types. This would be an interesting improvement for someone that wants
        /// to contribute.
        /// </summary>
        /// <returns>A copy of the parsed info with all types simplified.</returns>
        public ParsedInfo Simplify()
        {
            return new ParsedInfo(
                _parsedInfo.GlobalVariables
                    .Select(SimplifyGlobalVariableInfo)
                    .ToValueImmutableList(),
                _parsedInfo.Interfaces
                    .Select(SimplifyInterfaceInfo)
                    .ToValueImmutableList(),
                ValueImmutableList.Create<TypeAliasInfo>());
        }

        private InterfaceInfo SimplifyInterfaceInfo(InterfaceInfo interfaceInfo)
        {
            return new InterfaceInfo(
                interfaceInfo.Name,
                SimplifyExtractTypeParametersResult(interfaceInfo.ExtractTypeParametersResult),
                interfaceInfo.ExtendsList
                    .Select(SimplifyTypeInfo)
                    .ToValueImmutableList(),
                SimplifyInterfaceBodyInfo(interfaceInfo.Body));
        }

        private GlobalVariableInfo SimplifyGlobalVariableInfo(GlobalVariableInfo globalVariableInfo)
        {
            return new GlobalVariableInfo(
                globalVariableInfo.Name,
                globalVariableInfo.InlineInterface == null
                    ? null
                    : SimplifyInterfaceBodyInfo(globalVariableInfo.InlineInterface),
                globalVariableInfo.Type == null
                    ? null
                    : SimplifyTypeInfo(globalVariableInfo.Type));
        }

        private ExtractTypeParametersResult SimplifyExtractTypeParametersResult(ExtractTypeParametersResult extractTypeParametersResult)
        {
            return new ExtractTypeParametersResult(
                extractTypeParametersResult.TypeParameters
                    .Select(SimplifyTypeParameter)
                    .ToValueImmutableList(),
                extractTypeParametersResult.AnyConstraintsAreNotSimple);
        }

        private InterfaceBodyInfo SimplifyInterfaceBodyInfo(InterfaceBodyInfo interfaceBodyInfo)
        {
            return new InterfaceBodyInfo(
                interfaceBodyInfo.Constructors
                    .Select(SimplifyConstructorInfo)
                    .ToValueImmutableList(),
                interfaceBodyInfo.Properties
                    .Select(SimplifyPropertyInfo)
                    .ToValueImmutableList(),
                interfaceBodyInfo.Methods
                    .Select(SimplifyMethodInfo)
                    .ToValueImmutableList(),
                interfaceBodyInfo.Indexers
                    .Select(SimplifyIndexerInfo)
                    .ToValueImmutableList(),
                interfaceBodyInfo.GetAccessors
                    .Select(SimplifyGetAccessorInfo)
                    .ToValueImmutableList(),
                interfaceBodyInfo.SetAccessors
                    .Select(SimplifySetAccessorInfo)
                    .ToValueImmutableList());
        }

        private TypeInfo SimplifyUnionTypeInfo(UnionTypeInfo unionTypeInfo)
        {
            // If we are working with a union that contains `any`, we can just unwrap it into the any type all together.
            var containsAny = unionTypeInfo.Types
                .Any(typeInfo => typeInfo.Single != null && typeInfo.Single.Name == "any");

            if (containsAny)
            {
                return TypeInfo.AnyType;
            }

            var finalTypeList = new List<TypeInfo>();

            // Remove nulls!
            foreach (var typeWithinUnion in unionTypeInfo.Types)
            {
                if (typeWithinUnion.Single == null
                    || typeWithinUnion.Single.Name != "null")
                {
                    finalTypeList.Add(typeWithinUnion);
                }
            }

            // Deduplicate.
            finalTypeList = finalTypeList.DistinctSafeSlow().ToList();

            // Unwrap and simplify if there is only 1 thing left.
            if (finalTypeList.Count == 1)
            {
                return SimplifyTypeInfo(finalTypeList.First());
            }

            // Return a new type representing the union with each type simplified.
            return new TypeInfo(
                new UnionTypeInfo(finalTypeList
                    .Select(SimplifyTypeInfo)
                    .ToValueImmutableList()),
                null,
                null,
                null,
                null,
                null);
        }

        private TypeInfo SimplifyIntersectionTypeInfo(IntersectionTypeInfo intersectionTypeInfo)
        {
            var finalTypeList = intersectionTypeInfo.Types.ToList();

            // Deduplicate.
            finalTypeList = finalTypeList.DistinctSafeSlow().ToList();

            // Unwrap and simplify if there is only 1 thing left.
            if (finalTypeList.Count == 1)
            {
                return SimplifyTypeInfo(finalTypeList.First());
            }

            // Return a new type representing the intersection with each type simplified.
            return new TypeInfo(
                null,
                new IntersectionTypeInfo(finalTypeList
                    .Select(SimplifyTypeInfo)
                    .ToValueImmutableList()),
                null,
                null,
                null,
                null);
        }

        private TypeInfo SimplifyFunctionTypeInfo(FunctionTypeInfo functionTypeInfo)
        {
            return new TypeInfo(
                null,
                null,
                null,
                null,
                new FunctionTypeInfo(
                    SimplifyExtractTypeParametersResult(functionTypeInfo.ExtractTypeParametersResult),
                    functionTypeInfo.Parameters
                        .Select(SimplifyParameterInfo)
                        .ToValueImmutableList(),
                    SimplifyTypeInfo(functionTypeInfo.ReturnType)),
                null);
        }

        private TypeInfo ExpandExtendDefaultTypeArguments(TypeInfo extendTypeInfo)
        {
            if (extendTypeInfo.Single == null || extendTypeInfo.Single.TypeArguments.Any())
            {
                return extendTypeInfo;
            }

            var matchingInterface = _parsedInfo.Interfaces.FirstOrDefault(i => i.Name == extendTypeInfo.Single.Name);

            if (matchingInterface == null || !matchingInterface.ExtractTypeParametersResult.TypeParameters.Any())
            {
                return extendTypeInfo;
            }

            var newType = new TypeInfo(
                null,
                null,
                null,
                new SingleTypeInfo(
                    extendTypeInfo.Single.Name,
                    extendTypeInfo.Single.StringLiteral,
                    extendTypeInfo.Single.BooleanLiteral,
                    extendTypeInfo.Single.NumberLiteral,
                    matchingInterface.ExtractTypeParametersResult
                        .TypeParameters
                        .Select(typeParameter => typeParameter.Default ?? TypeInfo.AnyType)
                        .ToValueImmutableList(),
                    extendTypeInfo.Single.IsUnhandled),
                null,
                null);

            return newType;
        }

        private TypeInfo SimplifyTypeInfo(TypeInfo typeInfo)
        {
            while (true)
            {
                // Unwrap parenthesized types.
                if (typeInfo.Parenthesized != null)
                {
                    typeInfo = typeInfo.Parenthesized;
                    continue;
                }

                // If we are dealing with a union, let's try to remove nulls, deduplicate, and unwrap the union if there is only 1 left.
                if (typeInfo.Union != null)
                {
                    var simplifiedUnionType = SimplifyUnionTypeInfo(typeInfo.Union);

                    if (simplifiedUnionType != typeInfo)
                    {
                        typeInfo = simplifiedUnionType;
                        continue;
                    }
                }

                // If we are dealing with an intersection type, remove duplicates and unwrap the intersection if there is only 1 left.
                if (typeInfo.Intersection != null)
                {
                    var simplifiedIntersectionType = SimplifyIntersectionTypeInfo(typeInfo.Intersection);

                    if (simplifiedIntersectionType != typeInfo)
                    {
                        typeInfo = simplifiedIntersectionType;
                        continue;
                    }
                }

                // If we are dealing with a function type, simplify it and re-enter if things are different at all.
                if (typeInfo.Function != null)
                {
                    var simplifiedFunctionType = SimplifyFunctionTypeInfo(typeInfo.Function);

                    if (simplifiedFunctionType != typeInfo)
                    {
                        typeInfo = simplifiedFunctionType;
                        continue;
                    }
                }

                // If we are dealing with an array type, simplify it and re-enter if things are different at all.
                if (typeInfo.Array != null)
                {
                    var simplifiedArrayInfo = typeInfo with
                    {
                        Array = SimplifyTypeInfo(typeInfo.Array),
                    };

                    if (simplifiedArrayInfo != typeInfo)
                    {
                        typeInfo = simplifiedArrayInfo;
                        continue;
                    }
                }

                // If we are dealing with a single type, we might be at the point where we can re-write aliases!
                if (typeInfo.Single != null)
                {
                    var simplifiedSingleInfo = SimplifySingleTypeInfo(typeInfo.Single);

                    if (simplifiedSingleInfo != typeInfo)
                    {
                        typeInfo = simplifiedSingleInfo;
                        continue;
                    }

                    var expandedSingleTypeInfo = ExpandExtendDefaultTypeArguments(typeInfo);

                    if (expandedSingleTypeInfo != typeInfo)
                    {
                        typeInfo = expandedSingleTypeInfo;
                        continue;
                    }
                }

                return typeInfo;
            }
        }

        private TypeInfo SimplifySingleTypeInfo(SingleTypeInfo singleTypeInfo)
        {
            if (singleTypeInfo.IsUnhandled)
            {
                return TypeInfo.AnyType;
            }

            var simplifiedSingleType = new SingleTypeInfo(
                singleTypeInfo.Name,
                singleTypeInfo.StringLiteral,
                singleTypeInfo.BooleanLiteral,
                singleTypeInfo.NumberLiteral,
                singleTypeInfo.TypeArguments
                    .Select(SimplifyTypeInfo)
                    .ToValueImmutableList(),
                singleTypeInfo.IsUnhandled);

            var typeAlias = _parsedInfo.TypeAliases
                .FirstOrDefault(typeAlias => typeAlias.Name == singleTypeInfo.Name);

            if (typeAlias == null)
            {
                return new TypeInfo(
                    null,
                    null,
                    null,
                    simplifiedSingleType,
                    null,
                    null);
            }

            return ReplaceAliasType(simplifiedSingleType, typeAlias);
        }

        private TypeInfo ReplaceAliasType(SingleTypeInfo singleTypeInfo, TypeAliasInfo typeAlias)
        {
            // There is a weird case where you can actually self reference an alias, like so:
            // type Foo = number | string | Foo[];
            //
            // When this happens, we really don't have a great way of handling it, since we want to simplify
            // type expressions as much as possible at this point. Even if we somehow understood this expression
            // better, we pretty much always convert super complex cases to IJSObject, which lets the caller
            // take over and deal with properly typing stuff.
            //
            // Here we will actually try to detect these self-referencing cases, and simply return a type that represents
            // `any` instead, which will have the same effect downstream.
            if (ContainsSelfReference(typeAlias.Name, typeAlias.AliasType))
            {
                return TypeInfo.AnyType;
            }

            // If there are no type parameters, we can just return the type alias directly.
            if (!typeAlias.ExtractTypeParametersResult.TypeParameters.Any())
            {
                return typeAlias.AliasType;
            }

            // First, we should build a map that goes from alias type parameter name to the corresponding type parameter index.
            var typeParameterIndexMap = new Dictionary<string, int>();

            for (var aliasTypeParameterIndex = 0;
                 aliasTypeParameterIndex < typeAlias.ExtractTypeParametersResult.TypeParameters.Count;
                 aliasTypeParameterIndex++)
            {
                var aliasTypeParameter = typeAlias.ExtractTypeParametersResult.TypeParameters[aliasTypeParameterIndex];
                typeParameterIndexMap[aliasTypeParameter.Name] = aliasTypeParameterIndex;
            }

            return ReplaceInstancesOfAliasTypeArguments(
                typeAlias.AliasType,
                singleTypeInfo.TypeArguments,
                typeParameterIndexMap.ToValueImmutableDictionary());
        }

        private static TypeInfo ReplaceInstancesOfAliasTypeArguments(
            TypeInfo subjectType,
            ValueImmutableList<TypeInfo> typeArguments,
            ValueImmutableDictionary<string, int> typeParameterIndexMap)
        {
            if (subjectType.Union != null)
            {
                var rewrittenType = new TypeInfo(
                    new UnionTypeInfo(subjectType.Union
                        .Types
                        .Select(itemType => ReplaceInstancesOfAliasTypeArguments(
                            itemType,
                            typeArguments,
                            typeParameterIndexMap))
                        .ToValueImmutableList()),
                    null,
                    null,
                    null,
                    null,
                    null);

                if (rewrittenType != subjectType)
                {
                    return ReplaceInstancesOfAliasTypeArguments(
                        rewrittenType,
                        typeArguments,
                        typeParameterIndexMap);
                }
            }

            if (subjectType.Intersection != null)
            {
                var rewrittenType = new TypeInfo(
                    null,
                    new IntersectionTypeInfo(subjectType.Intersection
                        .Types
                        .Select(itemType => ReplaceInstancesOfAliasTypeArguments(
                            itemType,
                            typeArguments,
                            typeParameterIndexMap))
                        .ToValueImmutableList()),
                    null,
                    null,
                    null,
                    null);

                if (rewrittenType != subjectType)
                {
                    return ReplaceInstancesOfAliasTypeArguments(
                        rewrittenType,
                        typeArguments,
                        typeParameterIndexMap);
                }
            }

            if (subjectType.Array != null)
            {
                var rewrittenType = new TypeInfo(
                    null,
                    null,
                    null,
                    null,
                    null,
                    ReplaceInstancesOfAliasTypeArguments(
                        subjectType.Array,
                        typeArguments,
                        typeParameterIndexMap));

                if (rewrittenType != subjectType)
                {
                    return ReplaceInstancesOfAliasTypeArguments(
                        rewrittenType,
                        typeArguments,
                        typeParameterIndexMap);
                }
            }

            if (subjectType.Parenthesized != null)
            {
                var rewrittenType = new TypeInfo(
                    null,
                    null,
                    ReplaceInstancesOfAliasTypeArguments(
                        subjectType.Parenthesized,
                        typeArguments,
                        typeParameterIndexMap),
                    null,
                    null,
                    null);

                if (rewrittenType != subjectType)
                {
                    return ReplaceInstancesOfAliasTypeArguments(
                        rewrittenType,
                        typeArguments,
                        typeParameterIndexMap);
                }
            }

            if (subjectType.Function != null)
            {
                // FIXME: I feel like this is probably correct, but I wonder if the function extract type arguments need any replace magic?
                var rewrittenType = new TypeInfo(
                    null,
                    null,
                    null,
                    null,
                    new FunctionTypeInfo(
                        subjectType.Function.ExtractTypeParametersResult,
                        subjectType.Function.Parameters
                            .Select(parameterInfo => new ParameterInfo(
                                parameterInfo.Name,
                                parameterInfo.IsOptional,
                                ReplaceInstancesOfAliasTypeArguments(
                                    parameterInfo.Type,
                                    typeArguments,
                                    typeParameterIndexMap)))
                            .ToValueImmutableList(),
                        ReplaceInstancesOfAliasTypeArguments(
                            subjectType.Function.ReturnType,
                            typeArguments,
                            typeParameterIndexMap)),
                    null);

                if (rewrittenType != subjectType)
                {
                    return ReplaceInstancesOfAliasTypeArguments(
                        rewrittenType,
                        typeArguments,
                        typeParameterIndexMap);
                }
            }

            if (subjectType.Single != null)
            {
                if (subjectType.Single.Name != null && typeParameterIndexMap.ContainsKey(subjectType.Single.Name))
                {
                    return typeArguments[typeParameterIndexMap[subjectType.Single.Name]];
                }

                var typeArgumentsRewritten = subjectType.Single
                    .TypeArguments
                    .Select(typeArgument => ReplaceInstancesOfAliasTypeArguments(
                        typeArgument,
                        typeArguments,
                        typeParameterIndexMap))
                    .ToValueImmutableList();

                return new TypeInfo(
                    null,
                    null,
                    null,
                    new SingleTypeInfo(
                        subjectType.Single.Name,
                        subjectType.Single.StringLiteral,
                        subjectType.Single.BooleanLiteral,
                        subjectType.Single.NumberLiteral,
                        typeArgumentsRewritten,
                        subjectType.Single.IsUnhandled),
                    null,
                    null);
            }

            return subjectType;
        }

        private static bool ContainsSelfReference(string typeAliasName, TypeInfo aliasTypeNode)
        {
            if (aliasTypeNode.Array != null)
            {
                return ContainsSelfReference(typeAliasName, aliasTypeNode.Array);
            }

            if (aliasTypeNode.Function != null)
            {
                if (ContainsSelfReference(
                    typeAliasName,
                    aliasTypeNode.Function.ReturnType))
                {
                    return true;
                }

                var typeParametersContainSelfReference = aliasTypeNode.Function.ExtractTypeParametersResult
                    .TypeParameters
                    .Any(typeParameter =>
                    {
                        if (typeParameter.Default != null
                            && ContainsSelfReference(typeAliasName, typeParameter.Default))
                        {
                            return true;
                        }

                        if (typeParameter.Constraint != null &&
                            ContainsSelfReference(typeAliasName, typeParameter.Constraint))
                        {
                            return true;
                        }

                        return false;
                    });

                if (typeParametersContainSelfReference)
                {
                    return true;
                }

                var parametersContainSelfReference = aliasTypeNode.Function.Parameters
                    .Any(parameter => ContainsSelfReference(typeAliasName, parameter.Type));

                if (parametersContainSelfReference)
                {
                    return true;
                }

                return false;
            }

            if (aliasTypeNode.Intersection != null)
            {
                return aliasTypeNode.Intersection.Types
                    .Any(innerType => ContainsSelfReference(typeAliasName, innerType));
            }

            if (aliasTypeNode.Union != null)
            {
                return aliasTypeNode.Union.Types
                    .Any(innerType => ContainsSelfReference(typeAliasName, innerType));
            }

            if (aliasTypeNode.Parenthesized != null)
            {
                return ContainsSelfReference(typeAliasName, aliasTypeNode.Parenthesized);
            }

            if (aliasTypeNode.Single != null)
            {
                if (aliasTypeNode.Single.Name == typeAliasName)
                {
                    return true;
                }

                return aliasTypeNode.Single.TypeArguments
                    .Any(typeArgument => ContainsSelfReference(typeAliasName, typeArgument));
            }

            return false;
        }

        private TypeParameter SimplifyTypeParameter(TypeParameter typeParameter)
        {
            return new TypeParameter(
                typeParameter.Name,
                typeParameter.Default == null
                    ? null
                    : SimplifyTypeInfo(typeParameter.Default),
                typeParameter.Constraint == null
                    ? null
                    : SimplifyTypeInfo(typeParameter.Constraint));
        }

        private ConstructorInfo SimplifyConstructorInfo(ConstructorInfo constructorInfo)
        {
            return new ConstructorInfo(
                SimplifyTypeInfo(constructorInfo.ReturnType),
                SimplifyExtractTypeParametersResult(constructorInfo.ExtractTypeParametersResult),
                constructorInfo.Parameters
                    .Select(SimplifyParameterInfo)
                    .ToValueImmutableList());
        }

        private ParameterInfo SimplifyParameterInfo(ParameterInfo parameterInfo)
        {
            return new ParameterInfo(
                parameterInfo.Name,
                parameterInfo.IsOptional,
                SimplifyTypeInfo(parameterInfo.Type));
        }

        private PropertyInfo SimplifyPropertyInfo(PropertyInfo propertyInfo)
        {
            return new PropertyInfo(
                propertyInfo.Name,
                propertyInfo.IsReadonly,
                SimplifyTypeInfo(propertyInfo.Type));
        }

        private MethodInfo SimplifyMethodInfo(MethodInfo methodInfo)
        {
            return new MethodInfo(
                methodInfo.Name,
                SimplifyExtractTypeParametersResult(methodInfo.ExtractTypeParametersResult),
                SimplifyTypeInfo(methodInfo.ReturnType),
                methodInfo.Parameters
                    .Select(SimplifyParameterInfo)
                    .ToValueImmutableList());
        }

        private IndexerInfo SimplifyIndexerInfo(IndexerInfo indexerInfo)
        {
            return new IndexerInfo(
                SimplifyTypeInfo(indexerInfo.IndexType),
                indexerInfo.IndexName,
                SimplifyTypeInfo(indexerInfo.ReturnType),
                indexerInfo.IsReadonly);
        }

        private GetAccessorInfo SimplifyGetAccessorInfo(GetAccessorInfo getAccessorInfo)
        {
            return new GetAccessorInfo(
                getAccessorInfo.Name,
                SimplifyTypeInfo(getAccessorInfo.ReturnType));
        }

        private SetAccessorInfo SimplifySetAccessorInfo(SetAccessorInfo setAccessorInfo)
        {
            return new SetAccessorInfo(
                setAccessorInfo.Name,
                setAccessorInfo.Parameters
                    .Select(SimplifyParameterInfo)
                    .ToValueImmutableList());
        }
    }
}
