import * as fs from "fs";
import * as ts from "typescript";
import {SourceFile, SyntaxKind} from "typescript";

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

interface TypeInfo {
    names: string[];
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

interface InterfaceInfo {
    name: string;
    extendsList: string[];
    properties: PropertyInfo[];
}

interface ParsedInfo {
    globalVariables: GlobalVariableInfo[];
    interfaces: InterfaceInfo[];
}

function extractTypeName(typeNode: ts.TypeNode): string {
    if (ts.isTypeReferenceNode(typeNode)
        && ts.isIdentifier(typeNode.typeName)) {
        return typeNode.typeName.text;
    }

    if (ts.isArrayTypeNode(typeNode)) {
        const typeName = extractTypeName(typeNode.elementType);
        return `${typeName}[]`;
    }

    if (typeNode.kind === SyntaxKind.NumberKeyword) {
        return "number";
    }

    if (typeNode.kind === SyntaxKind.StringKeyword) {
        return "string";
    }

    if (typeNode.kind === SyntaxKind.BooleanKeyword) {
        return "boolean";
    }

    if (ts.isLiteralTypeNode(typeNode)) {
        if (typeNode.literal.kind == SyntaxKind.NullKeyword) {
            return "null";
        }

        return "unhandled_literal"
    }

    return "unhandled";
}

function extractTypeInfo(typeNode: ts.TypeNode): TypeInfo {
    if (ts.isUnionTypeNode(typeNode)) {
        const typeNames: string[] = [];

        typeNode.types.forEach(unionTypeChild => {
            typeNames.push(extractTypeName(unionTypeChild));
        });

        return {
            names: typeNames,
        }
    }

    return {
        names: [extractTypeName(typeNode)],
    };
}

function isParameterOptional(parameterDeclaration: ts.ParameterDeclaration): boolean {
    return !!parameterDeclaration.questionToken
        && parameterDeclaration.questionToken.kind == SyntaxKind.QuestionToken
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
                if (modifier.kind === SyntaxKind.ReadonlyKeyword) {
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

inputTypeDefinitions.forEach(inputTypeDefinition => {
    const inputPath = `node_modules/typescript/lib/${inputTypeDefinition}.ts`;
    const outputPath = `output/${inputTypeDefinition}.json`;

    console.log(`Dumping AST for "${inputPath}" to "${outputPath}"...`);

    const sourceFile: SourceFile = ts.createSourceFile(
        'x.ts',
        fs.readFileSync(inputPath, {encoding:'utf8', flag:'r'}),
        ts.ScriptTarget.Latest
    );

    const parsedInfo: ParsedInfo = {
        globalVariables: [],
        interfaces: [],
    };

    if (isDebugMode) {
        (parsedInfo as any).raw = sourceFile;
    }

    sourceFile.statements.forEach(statement => {
        if (ts.isInterfaceDeclaration(statement)
            && ts.isIdentifier(statement.name)) {
            const extendsList: string[] = [];

            if (!!statement.heritageClauses) {
                statement.heritageClauses.forEach(heritageClause => {
                    if (heritageClause.token !== SyntaxKind.ExtendsKeyword) {
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

            const interfaceInfo: InterfaceInfo = {
                name: statement.name.text,
                extendsList: extendsList,
                properties: extractProperties(statement.members),
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
                        const parameters: ParameterInfo[] = [];

                        member.parameters.forEach(parameter => {
                            if (!ts.isIdentifier(parameter.name) || !parameter.type) {
                                return;
                            }

                            parameters.push({
                                name: parameter.name.text,
                                isOptional: isParameterOptional(parameter),
                                type: extractTypeInfo(parameter.type),
                            });
                        });


                        constructors.push({
                            returnType: extractTypeInfo(member.type),
                            parameters: parameters,
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

    fs.writeFileSync(outputPath, JSON.stringify(parsedInfo, getCircularReplacer(), isPrettyPrint ? 2 : undefined));
});
