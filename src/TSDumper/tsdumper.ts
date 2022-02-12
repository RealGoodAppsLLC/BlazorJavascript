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
}

interface ParsedInfo {
    globalVariables: GlobalVariableInfo[];
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
    };

    if (isDebugMode) {
        (parsedInfo as any).raw = sourceFile;
    }

    sourceFile.statements.forEach(statement => {
        if (ts.isInterfaceDeclaration(statement)) {
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
                });
            });
        }
    });

    fs.writeFileSync(outputPath, JSON.stringify(parsedInfo, getCircularReplacer(), isPrettyPrint ? 2 : undefined));
});
