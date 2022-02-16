import * as ts from "typescript";
import { SourceFile } from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";
import { InterfaceBodyInfo, extractInterfaceBody } from "./interfaces";

export interface GlobalVariableInfo {
    name: string;
    inlineInterface: InterfaceBodyInfo | null;
    type: TypeInfo | null;
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
                || !ts.isIdentifier(declaration.name)) {
                return;
            }

            const declarationName = declaration.name.text;

            let inlineInterface: InterfaceBodyInfo | null = null;

            if (ts.isTypeLiteralNode(declaration.type)) {
                inlineInterface = extractInterfaceBody(declaration.type.members);
            }

            let type: TypeInfo | null = null;

            if (ts.isTypeReferenceNode(declaration.type)) {
                type = extractTypeInfo(declaration.type);
            }

            if (inlineInterface === null && type === null) {
                return;
            }

            globalVariables.push({
                name: declarationName,
                inlineInterface: inlineInterface,
                type: type,
            });
        });
    });

    return globalVariables;
};