import * as ts from "typescript";
import { extractTypeParameters, ExtractTypeParametersResult } from "./typeparameters";
import { extractTypeInfo, TypeInfo } from "./types";
import { extractParameters, ParameterInfo } from "./parameters";

export interface MethodInfo {
    name: string;
    extractTypeParametersResult: ExtractTypeParametersResult;
    returnType: TypeInfo;
    parameters: ParameterInfo[];
}

export const extractMethods = (members: ts.NodeArray<ts.TypeElement>): MethodInfo[] => {
    const methods: MethodInfo[] = [];

    members.forEach(member => {
        if (!ts.isMethodSignature(member) || !ts.isIdentifier(member.name) || !member.type) {
            return;
        }

        methods.push({
            name: member.name.text,
            parameters: extractParameters(member.parameters),
            extractTypeParametersResult: extractTypeParameters(member.typeParameters),
            returnType: extractTypeInfo(member.type),
        });
    });

    return methods;
};