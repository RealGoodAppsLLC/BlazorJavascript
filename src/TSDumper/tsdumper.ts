import * as fs from "fs";
import * as ts from "typescript";

const inputTypeDefinitions = [
    'lib.dom.d',
];

const isPrettyPrint = process.argv.indexOf('--pretty') !== -1;
const isDebugMode = process.argv.indexOf('--debug') !== -1;

const getCircularReplacer = () => {
    const seen = new WeakSet();
    return (key: any, value: any) => {
        if (typeof value === "object" && value !== null) {
            if (seen.has(value)) {
                return;
            }
            seen.add(value);
        }
        return value;
    };
};

fs.mkdirSync('output', {recursive: true});

interface ConstructorInfo {
    returnType: TypeInfo;
    parameters: ParameterInfo[];
}

interface ParameterInfo {
    name: string;
    isOptional: boolean;
    type: TypeInfo;
}

interface StringLiteralInfo {
    value: string;
}

interface BooleanLiteralInfo {
    value: boolean;
}

interface NumberLiteralInfo {
    value: number;
}

interface TypeInfo {
    union: UnionTypeInfo | null;
    intersection: IntersectionTypeInfo | null;
    parenthesized: TypeInfo | null;
    single: SingleTypeInfo | null;
    function: FunctionTypeInfo | null;
}

interface UnionTypeInfo {
    types: TypeInfo[];
}

interface IntersectionTypeInfo {
    types: TypeInfo[];
}

interface SingleTypeInfo {
    name: string | null;
    stringLiteral: StringLiteralInfo | null;
    booleanLiteral: BooleanLiteralInfo | null;
    numberLiteral: NumberLiteralInfo | null;
    typeArguments: TypeInfo[];
    isUnhandled: boolean;
}

interface FunctionTypeInfo {
    extractTypeParametersResult: ExtractTypeParametersResult;
    parameters: ParameterInfo[];
    returnType: TypeInfo;
}

interface GlobalVariableInfo {
    name: string;
    hasPrototype: boolean;
    constructors: ConstructorInfo[];
    properties: PropertyInfo[];
}

interface PropertyInfo {
    name: string;
    isReadonly: boolean;
    type: TypeInfo;
}

interface TypeParameter {
    name: string;
    constraint: TypeInfo | null;
}

interface MethodInfo {
    name: string;
    extractTypeParametersResult: ExtractTypeParametersResult;
    returnType: TypeInfo;
    parameters: ParameterInfo[];
}

interface IndexerInfo {
    indexType: TypeInfo;
    returnType: TypeInfo;
}

interface GetAccessorInfo {
    name: string;
    returnType: TypeInfo;
}

interface SetAccessorInfo {
    name: string;
    parameters: ParameterInfo[];
}

interface InterfaceInfo {
    name: string;
    extractTypeParametersResult: ExtractTypeParametersResult;
    extendsList: string[];
    properties: PropertyInfo[];
    methods: MethodInfo[];
    indexers: IndexerInfo[];
    getAccessors: GetAccessorInfo[];
    setAccessors: SetAccessorInfo[];
}

interface ParsedInfo {
    globalVariables: GlobalVariableInfo[];
    interfaces: InterfaceInfo[];
}

interface ExtractTypeParametersResult {
    typeParameters: TypeParameter[];
    anyConstraintsAreNotSimple: boolean;
}

function extractTypeArguments(typeNode: ts.TypeReferenceNode): TypeInfo[] {
    const typeArguments: TypeInfo[] = [];

    if (!!typeNode.typeArguments) {
        typeNode.typeArguments.forEach(typeArgument => {
            typeArguments.push(extractTypeInfo(typeArgument));
        });
    }

    return typeArguments;
}
function extractSingleTypeInfo(typeNode: ts.TypeNode): SingleTypeInfo {
    if (ts.isTypeReferenceNode(typeNode)
        && ts.isIdentifier(typeNode.typeName)) {
        return {
            name: typeNode.typeName.text,
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: extractTypeArguments(typeNode),
            isUnhandled: false,
        };
    }

    if (ts.isArrayTypeNode(typeNode)) {
        const typeName = extractSingleTypeInfo(typeNode.elementType);
        return {
            name: `${typeName}[]`,
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.NumberKeyword) {
        return {
            name: "number",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.StringKeyword) {
        return {
            name: "string",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.BooleanKeyword) {
        return {
            name: "boolean",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.AnyKeyword) {
        return {
            name: "any",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.UndefinedKeyword) {
        return {
            name: "undefined",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.VoidKeyword) {
        return {
            name: "void",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (ts.isLiteralTypeNode(typeNode)) {
        if (typeNode.literal.kind === ts.SyntaxKind.NullKeyword) {
            return {
                name: "null",
                stringLiteral: null,
                booleanLiteral: null,
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        if (typeNode.literal.kind === ts.SyntaxKind.TrueKeyword) {
            return {
                name: null,
                stringLiteral: null,
                booleanLiteral: {
                    value: true,
                },
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        if (typeNode.literal.kind === ts.SyntaxKind.FalseKeyword) {
            return {
                name: null,
                stringLiteral: null,
                booleanLiteral: {
                    value: false,
                },
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        if (ts.isStringLiteral(typeNode.literal)) {
            return {
                name: null,
                stringLiteral: {
                    value: typeNode.literal.text,
                },
                booleanLiteral: null,
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        return {
            name: `unhandled_literal:${typeNode.literal.kind}`,
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: true,
        };
    }

    return {
        name: `unhandled:${typeNode.kind}`,
        stringLiteral: null,
        booleanLiteral: null,
        numberLiteral: null,
        typeArguments: [],
        isUnhandled: true,
    };
}

function extractTypeInfo(typeNode: ts.TypeNode): TypeInfo {
    if (ts.isUnionTypeNode(typeNode)) {
        const unionTypes: TypeInfo[] = [];

        typeNode.types.forEach(unionChild => {
            unionTypes.push(extractTypeInfo(unionChild));
        });

        return {
            union: {
                types: unionTypes,
            },
            intersection: null,
            parenthesized: null,
            single: null,
            function: null,
        }
    }

    if (ts.isIntersectionTypeNode(typeNode)) {
        const intersectionTypes: TypeInfo[] = [];

        typeNode.types.forEach(intersectionChild => {
            intersectionTypes.push(extractTypeInfo(intersectionChild));
        });

        return {
            union: null,
            intersection: {
                types: intersectionTypes,
            },
            parenthesized: null,
            single: null,
            function: null,
        }
    }

    if (ts.isParenthesizedTypeNode(typeNode)) {
        return {
            union: null,
            intersection: null,
            parenthesized: extractTypeInfo(typeNode.type),
            single: null,
            function: null,
        }
    }

    if (ts.isFunctionTypeNode(typeNode)) {
        return {
            union: null,
            intersection: null,
            parenthesized: null,
            single: null,
            function: {
                parameters: extractParameters(typeNode.parameters),
                extractTypeParametersResult: extractTypeParameters(typeNode.typeParameters),
                returnType: extractTypeInfo(typeNode.type)
            },
        }
    }

    return {
        union: null,
        intersection: null,
        parenthesized: null,
        single: extractSingleTypeInfo(typeNode),
        function: null,
    };
}

function isParameterOptional(parameterDeclaration: ts.ParameterDeclaration): boolean {
    return !!parameterDeclaration.questionToken
        && parameterDeclaration.questionToken.kind == ts.SyntaxKind.QuestionToken
}

function extractProperties(members: ts.NodeArray<ts.TypeElement>) {
    const properties: PropertyInfo[] = [];

    members.forEach(member => {
        if (!ts.isPropertySignature(member) || !ts.isIdentifier(member.name) || !member.type) {
            return;
        }

        // FIXME: Ignore properties that are named "prototype" to make our life easier.
        //        There might be a better way to do this.
        if (member.name.text === "prototype") {
            return;
        }

        let isReadonly = false;

        if (!!member.modifiers) {
            member.modifiers.forEach(modifier => {
                if (modifier.kind === ts.SyntaxKind.ReadonlyKeyword) {
                    isReadonly = true;
                }
            });
        }

        properties.push({
            name: member.name.text,
            type: extractTypeInfo(member.type),
            isReadonly: isReadonly,
        });
    });

    return properties;
}

function extractParameters(member: ts.NodeArray<ts.ParameterDeclaration>): ParameterInfo[] {
    const parameters: ParameterInfo[] = [];

    member.forEach(parameter => {
        if (!ts.isIdentifier(parameter.name) || !parameter.type) {
            return;
        }

        parameters.push({
            name: parameter.name.text,
            isOptional: isParameterOptional(parameter),
            type: extractTypeInfo(parameter.type),
        });
    });

    return parameters;
}

function extractTypeParameters(typeParameterDeclarations: ts.NodeArray<ts.TypeParameterDeclaration> | null | undefined): ExtractTypeParametersResult {
    const typeParameters: TypeParameter[] = [];

    if (!typeParameterDeclarations) {
        return {
            typeParameters: typeParameters,
            anyConstraintsAreNotSimple: false,
        };
    }

    // FIXME: Any function that has something like this is ignored for now:
    //        foo<K extends keyof Bar>(type: K)
    //        In the future, we should consider replacing the generic and parameters with a string.
    let anyConstraintsAreNotSimple = false;

    if (!!typeParameterDeclarations) {
        typeParameterDeclarations.forEach(typeParameter => {
            if (!ts.isIdentifier(typeParameter.name)) {
                return;
            }

            // FIXME: For now, we are ignoring defaults for type parameters.
            //        We might be able to emulate this with subclassing: https://stackoverflow.com/a/707788.
            let constraint: TypeInfo | null = null;

            if (!!typeParameter.constraint) {
                if (ts.isTypeOperatorNode(typeParameter.constraint)) {
                    anyConstraintsAreNotSimple = true;
                    return;
                }

                constraint = extractTypeInfo(typeParameter.constraint);
            }

            typeParameters.push({
                name: typeParameter.name.text,
                constraint: constraint,
            });
        });
    }

    return {
        typeParameters: typeParameters,
        anyConstraintsAreNotSimple: anyConstraintsAreNotSimple,
    }
}

function recursiveCheckForNonSimpleTypeArgument(result: TypeInfo): boolean {
    let anyNodeNotSimple = false;

    if (result.single !== null) {
        result.single.typeArguments.every(typeArgument => {
            if (recursiveCheckForNonSimpleTypeArgument(typeArgument)) {
                anyNodeNotSimple = true;
                return false;
            }

            return true;
        });
    }

    if (result.union !== null) {
        result.union.types.every(unionTypeChild => {
            if (recursiveCheckForNonSimpleTypeArgument(unionTypeChild)) {
                anyNodeNotSimple = true;
                return false;
            }

            return true;
        });
    }

    if (result.intersection !== null) {
        result.intersection.types.every(intersectionTypeChild => {
            if (recursiveCheckForNonSimpleTypeArgument(intersectionTypeChild)) {
                anyNodeNotSimple = true;
                return false;
            }

            return true;
        });
    }

    if (result.function !== null) {
        if (result.function.extractTypeParametersResult.anyConstraintsAreNotSimple
            || recursiveCheckForNonSimpleTypeArgument(result.function.returnType)) {
            anyNodeNotSimple = true;
        }
        else {
            result.function.parameters.every(parameterInfo => {
                if (recursiveCheckForNonSimpleTypeArgument(parameterInfo.type)) {
                    anyNodeNotSimple = true;
                    return false;
                }

                return true;
            });
        }
    }

    return anyNodeNotSimple;
}

function postProcessParsedInfo(parsedInfo: ParsedInfo): ParsedInfo {
    let postProcessedGlobalVariables: GlobalVariableInfo[] = [];
    let postProcessedInterfaces: InterfaceInfo[] = [];

    parsedInfo.globalVariables.forEach(globalVariable => {
        const postProcessedConstructors: ConstructorInfo[] = [];
        const postProcessedProperties: PropertyInfo[] = [];

        globalVariable.constructors.forEach(constructor => {
            let keepConstructor = true;

            constructor.parameters.forEach(constructorParameter => {
                if (recursiveCheckForNonSimpleTypeArgument(constructorParameter.type)) {
                    keepConstructor = false;
                }
            });

            if (keepConstructor) {
                postProcessedConstructors.push(constructor);
            }
        });

        globalVariable.properties.forEach(property => {
            if (recursiveCheckForNonSimpleTypeArgument(property.type)) {
                return;
            }

            postProcessedProperties.push(property);
        });

        const postProcessedGlobalVariable: GlobalVariableInfo = {
            name: globalVariable.name,
            hasPrototype: globalVariable.hasPrototype,
            constructors: postProcessedConstructors,
            properties: postProcessedProperties,
        };

        postProcessedGlobalVariables.push(postProcessedGlobalVariable);
    });

    parsedInfo.interfaces.forEach(interfaceInfo => {
        if (interfaceInfo.extractTypeParametersResult.anyConstraintsAreNotSimple) {
            return;
        }

        const postProcessedMethods: MethodInfo[] = [];
        const postProcessedProperties: PropertyInfo[] = [];

        interfaceInfo.methods.forEach(methodInfo => {
            let keepMethod = true;

            if (methodInfo.extractTypeParametersResult.anyConstraintsAreNotSimple) {
                keepMethod = false;
            }
            else if (recursiveCheckForNonSimpleTypeArgument(methodInfo.returnType)) {
                keepMethod = false;
            }
            else {
                methodInfo.parameters.forEach(parameterInfo => {
                    if (recursiveCheckForNonSimpleTypeArgument(parameterInfo.type)) {
                        keepMethod = false;
                    }
                });
            }

            if (keepMethod) {
                postProcessedMethods.push(methodInfo);
            }
        });

        interfaceInfo.properties.forEach(property => {
            if (recursiveCheckForNonSimpleTypeArgument(property.type)) {
                return;
            }

            postProcessedProperties.push(property);
        });

        const postProcessedIndexers: IndexerInfo[] = [];

        interfaceInfo.indexers.forEach(indexer => {
            if (recursiveCheckForNonSimpleTypeArgument(indexer.indexType)
                || recursiveCheckForNonSimpleTypeArgument(indexer.returnType)) {
                return;
            }

            postProcessedIndexers.push(indexer);
        });

        const postProcessedGetAccessors: GetAccessorInfo[] = [];

        interfaceInfo.getAccessors.forEach(getAccessor => {
            if (recursiveCheckForNonSimpleTypeArgument(getAccessor.returnType)) {
                return;
            }

            postProcessedGetAccessors.push(getAccessor);
        });

        const postProcessedSetAccessors: SetAccessorInfo[] = [];

        interfaceInfo.setAccessors.forEach(setAccessor => {
            let keepSetAccessor = true;

            setAccessor.parameters.forEach(parameter => {
                if (recursiveCheckForNonSimpleTypeArgument(parameter.type)) {
                    keepSetAccessor = false;
                    return;
                }
            });

            if (keepSetAccessor) {
                postProcessedSetAccessors.push(setAccessor);
            }
        });

        postProcessedInterfaces.push({
            name: interfaceInfo.name,
            methods: postProcessedMethods,
            extractTypeParametersResult: interfaceInfo.extractTypeParametersResult,
            extendsList: interfaceInfo.extendsList,
            properties: postProcessedProperties,
            indexers: postProcessedIndexers,
            getAccessors: postProcessedGetAccessors,
            setAccessors: postProcessedSetAccessors
        });
    });

    return {
        globalVariables: postProcessedGlobalVariables,
        interfaces: postProcessedInterfaces,
    };
}

inputTypeDefinitions.forEach(inputTypeDefinition => {
    const inputPath = `node_modules/typescript/lib/${inputTypeDefinition}.ts`;
    const outputPath = `output/${inputTypeDefinition}.json`;

    console.log(`Dumping AST for "${inputPath}" to "${outputPath}"...`);

    const sourceFile: ts.SourceFile = ts.createSourceFile(
        'x.ts',
        fs.readFileSync(inputPath, {encoding:'utf8', flag:'r'}),
        ts.ScriptTarget.Latest
    );

    const parsedInfo: ParsedInfo = {
        globalVariables: [],
        interfaces: [],
    };

    sourceFile.statements.forEach(statement => {
        if (ts.isInterfaceDeclaration(statement)
            && ts.isIdentifier(statement.name)) {
            const extendsList: string[] = [];

            if (!!statement.heritageClauses) {
                statement.heritageClauses.forEach(heritageClause => {
                    if (heritageClause.token !== ts.SyntaxKind.ExtendsKeyword) {
                        console.error("Heritage clause detected without extends keyword.");
                        return;
                    }

                    heritageClause.types.forEach(type => {
                        if (!ts.isIdentifier(type.expression)) {
                            return;
                        }

                        extendsList.push(type.expression.text);
                    });
                });
            }

            const methods: MethodInfo[] = [];

            statement.members.forEach(member => {
                if (!ts.isMethodSignature(member) || !ts.isIdentifier(member.name) || !member.type) {
                    return;
                }

                methods.push({
                    name: member.name.text,
                    parameters: extractParameters(member.parameters),
                    extractTypeParametersResult: extractTypeParameters(member.typeParameters),
                    returnType: extractTypeInfo(member.type),
                });
            });

            const indexers: IndexerInfo[] = [];

            statement.members.forEach(member => {
                if (!ts.isIndexSignatureDeclaration(member)
                    || member.parameters.length < 1
                    || !member.parameters[0].type) {
                    return;
                }

                indexers.push({
                    indexType: extractTypeInfo(member.parameters[0].type),
                    returnType: extractTypeInfo(member.type)
                });
            });

            const getAccessors: GetAccessorInfo[] = [];

            statement.members.forEach(member => {
                if (!ts.isGetAccessor(member) || !ts.isIdentifier(member.name) || !member.type) {
                    return;
                }

                getAccessors.push({
                    name: member.name.text,
                    returnType: extractTypeInfo(member.type)
                });
            })

            const setAccessors: SetAccessorInfo[] = [];

            statement.members.forEach(member => {
                if (!ts.isSetAccessor(member) || !ts.isIdentifier(member.name)) {
                    return;
                }

                setAccessors.push({
                    name: member.name.text,
                    parameters: extractParameters(member.parameters)
                });
            })

            const interfaceInfo: InterfaceInfo = {
                name: statement.name.text,
                extendsList: extendsList,
                extractTypeParametersResult: extractTypeParameters(statement.typeParameters),
                properties: extractProperties(statement.members),
                methods: methods,
                indexers: indexers,
                getAccessors: getAccessors,
                setAccessors: setAccessors
            };

            parsedInfo.interfaces.push(interfaceInfo);
            return;
        }

        if (ts.isVariableStatement(statement)) {
            statement.declarationList.declarations.forEach(declaration => {
                if (!ts.isVariableDeclaration(declaration)
                    || !declaration.type
                    || !ts.isTypeLiteralNode(declaration.type)
                    || !ts.isIdentifier(declaration.name)) {
                    return;
                }

                let hasPrototype = false;

                const declarationName = declaration.name.text;
                const constructors: ConstructorInfo[] = [];

                declaration.type.members.forEach(member => {
                    if (ts.isPropertySignature(member)
                        && ts.isIdentifier(member.name)
                        && member.name.text === "prototype"
                        && member.type
                        && ts.isTypeReferenceNode(member.type)
                        && ts.isIdentifier(member.type.typeName)
                        && member.type.typeName.text === declarationName) {
                        hasPrototype = true;
                        return;
                    }

                    if (ts.isConstructSignatureDeclaration(member) && member.type) {
                        constructors.push({
                            returnType: extractTypeInfo(member.type),
                            parameters: extractParameters(member.parameters),
                        });

                        return;
                    }
                });

                parsedInfo.globalVariables.push({
                    name: declarationName,
                    hasPrototype: hasPrototype,
                    constructors: constructors,
                    properties: extractProperties(declaration.type.members),
                });
            });
        }
    });

    const postProcessedInfo = postProcessParsedInfo(parsedInfo);

    if (isDebugMode) {
        (postProcessedInfo as any).raw = sourceFile;
    }

    fs.writeFileSync(outputPath, JSON.stringify(postProcessedInfo, getCircularReplacer(), isPrettyPrint ? 2 : undefined));
});
