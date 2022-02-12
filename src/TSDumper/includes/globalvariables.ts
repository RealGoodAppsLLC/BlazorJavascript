import * as ts from "typescript";
import { ConstructorInfo } from "./constructors";
import { extractProperties, PropertyInfo } from "./properties";
import { SourceFile } from "typescript";
import { extractTypeInfo } from "./types";
import { extractParameters } from "./parameters";

export interface GlobalVariableInfo {
    name: string;
    hasPrototype: boolean;
    constructors: ConstructorInfo[];
    properties: PropertyInfo[];
}

export const extractGlobalVariables = (sourceFile: SourceFile): GlobalVariableInfo[] => {
    const globalVariables: GlobalVariableInfo[] = [];

    sourceFile.statements.forEach(statement => {
        if (!ts.isVariableStatement(statement)) {
            return;
        }

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

            globalVariables.push({
                name: declarationName,
                hasPrototype: hasPrototype,
                constructors: constructors,
                properties: extractProperties(declaration.type.members),
            });
        });
    });

    return globalVariables;
};