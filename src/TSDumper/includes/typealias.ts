import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";
import { extractTypeParameters, ExtractTypeParametersResult } from "./typeparameters";

export interface TypeAliasInfo {
    name: string;
    extractTypeParametersResult: ExtractTypeParametersResult;
    aliasType: TypeInfo;
}

export const extractTypeAliases = (sourceFile: ts.SourceFile): TypeAliasInfo[] => {
    const typeAliases: TypeAliasInfo[] = [];

    sourceFile.statements.forEach(statement => {
        if (!ts.isTypeAliasDeclaration(statement)
            || !ts.isIdentifier(statement.name)) {
            return;
        }

        const typeAliasInfo: TypeAliasInfo = {
            name: statement.name.text,
            extractTypeParametersResult: extractTypeParameters(statement.typeParameters),
            aliasType: extractTypeInfo(statement.type)
        };

        typeAliases.push(typeAliasInfo);
    });

    return typeAliases;
};
