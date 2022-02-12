import * as ts from "typescript";
import { extractTypeParameters, ExtractTypeParametersResult } from "./typeparameters";
import { extractProperties, PropertyInfo } from "./properties";
import { extractMethods, MethodInfo } from "./methods";
import { extractIndexers, IndexerInfo } from "./indexers";
import { extractGetAccessors, extractSetAccessors, GetAccessorInfo, SetAccessorInfo } from "./accessors";
import { SourceFile } from "typescript";

export interface InterfaceInfo {
    name: string;
    extractTypeParametersResult: ExtractTypeParametersResult;
    extendsList: string[];
    properties: PropertyInfo[];
    methods: MethodInfo[];
    indexers: IndexerInfo[];
    getAccessors: GetAccessorInfo[];
    setAccessors: SetAccessorInfo[];
}

export const extractInterfaces = (sourceFile: SourceFile): InterfaceInfo[] => {
    const interfaces: InterfaceInfo[] = [];

    sourceFile.statements.forEach(statement => {
        if (!ts.isInterfaceDeclaration(statement)
            || !ts.isIdentifier(statement.name)) {
            return;
        }

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

        const interfaceInfo: InterfaceInfo = {
            name: statement.name.text,
            extendsList: extendsList,
            extractTypeParametersResult: extractTypeParameters(statement.typeParameters),
            properties: extractProperties(statement.members),
            methods: extractMethods(statement.members),
            indexers: extractIndexers(statement.members),
            getAccessors: extractGetAccessors(statement.members),
            setAccessors: extractSetAccessors(statement.members)
        };

        interfaces.push(interfaceInfo);
    });

    return interfaces;
};