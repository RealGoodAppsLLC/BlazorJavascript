import * as fs from "fs";
import {
    Block,
    InterfaceDeclaration,
    isTypeReferenceNode,
    SourceFile,
    SyntaxKind,
    VariableDeclaration,
    VariableStatement
} from "typescript";
import * as ts from "typescript";

const inputTypeDefinitions = [
    'lib.dom.d',
];

const isPrettyPrint = process.argv.indexOf('--pretty') !== -1;

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
    returnType: string;
}

interface ClassInfo {
    name: string;
    constructors: ConstructorInfo[];
}

interface ParsedInfo {
    classes: ClassInfo[];
    raw: any;
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
        classes: [],
        raw: sourceFile,
    };

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

                    if (ts.isConstructSignatureDeclaration(member)
                        && member.type
                        && ts.isTypeReferenceNode(member.type)
                        && ts.isIdentifier(member.type.typeName)) {
                        constructors.push({
                            returnType: member.type.typeName.text,
                        });

                        return;
                    }
                });

                if (hasPrototype) {
                    parsedInfo.classes.push({
                        name: declarationName,
                        constructors: constructors,
                    });
                }
            });
        }
    });

    fs.writeFileSync(outputPath, JSON.stringify(parsedInfo, getCircularReplacer(), isPrettyPrint ? 2 : undefined));
});
