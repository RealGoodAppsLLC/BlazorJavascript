import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";

export interface IndexerInfo {
    indexType: TypeInfo;
    indexName: string;
    returnType: TypeInfo;
    isReadonly: boolean;
}

export const extractIndexers = (members: ts.NodeArray<ts.TypeElement>): IndexerInfo[] => {
    const indexers: IndexerInfo[] = [];

    members.forEach(member => {
        if (!ts.isIndexSignatureDeclaration(member)
            || member.parameters.length < 1
            || !member.parameters[0].type
            || !ts.isIdentifier(member.parameters[0].name)) {
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

        indexers.push({
            indexType: extractTypeInfo(member.parameters[0].type),
            indexName: member.parameters[0].name.text,
            returnType: extractTypeInfo(member.type),
            isReadonly: isReadonly
        });
    });

    return indexers;
};