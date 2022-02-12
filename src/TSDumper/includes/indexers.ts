import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";

export interface IndexerInfo {
    indexType: TypeInfo;
    returnType: TypeInfo;
}

export const extractIndexers = (members: ts.NodeArray<ts.TypeElement>): IndexerInfo[] => {
    const indexers: IndexerInfo[] = [];

    members.forEach(member => {
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

    return indexers;
};