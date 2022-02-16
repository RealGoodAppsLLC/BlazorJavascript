import * as ts from "typescript";
import { ConstructorInfo } from "./constructors";
import { extractTypeParameters, ExtractTypeParametersResult } from "./typeparameters";
import { extractProperties, PropertyInfo } from "./properties";
import { extractMethods, MethodInfo } from "./methods";
import { extractIndexers, IndexerInfo } from "./indexers";
import { extractGetAccessors, extractSetAccessors, GetAccessorInfo, SetAccessorInfo } from "./accessors";
import { extractParameters } from "./parameters";
import { extractTypeInfo, TypeInfo } from "./types";
import { SourceFile } from "typescript";

export interface InterfaceInfo {
    name: string;
    extractTypeParametersResult: ExtractTypeParametersResult;
    extendsList: TypeInfo[];
    body: InterfaceBodyInfo;
}

export interface InterfaceBodyInfo {
    constructors: ConstructorInfo[];
    properties: PropertyInfo[];
    methods: MethodInfo[];
    indexers: IndexerInfo[];
    getAccessors: GetAccessorInfo[];
    setAccessors: SetAccessorInfo[];
}

export const extractInterfaceBody = (
    members: ts.NodeArray<ts.TypeElement>
): InterfaceBodyInfo => {
    const constructors: ConstructorInfo[] = [];

    members.forEach(member => {
        if (ts.isConstructSignatureDeclaration(member) && member.type) {
            constructors.push({
                returnType: extractTypeInfo(member.type),
                parameters: extractParameters(member.parameters),
            });

            return;
        }
    });

    return {
        constructors: constructors,
        properties: extractProperties(members),
        methods: extractMethods(members),
        indexers: extractIndexers(members),
        getAccessors: extractGetAccessors(members),
        setAccessors: extractSetAccessors(members)
    };
};

export const extractInterfaces = (sourceFile: SourceFile): InterfaceInfo[] => {
    const interfaces: InterfaceInfo[] = [];

    sourceFile.statements.forEach(statement => {
        if (!ts.isInterfaceDeclaration(statement)
            || !ts.isIdentifier(statement.name)) {
            return;
        }

        const extendsList: TypeInfo[] = [];

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

                    const extendType = extractTypeInfo(type);
                    extendsList.push(extendType);
                });
            });
        }

        const interfaceInfo: InterfaceInfo = {
            name: statement.name.text,
            extendsList: extendsList,
            extractTypeParametersResult: extractTypeParameters(statement.typeParameters),
            body: extractInterfaceBody(statement.members)
        };

        interfaces.push(interfaceInfo);
    });

    return interfaces;
};
