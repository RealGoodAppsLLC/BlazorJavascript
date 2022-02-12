import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";

export interface ParameterInfo {
    name: string;
    isOptional: boolean;
    type: TypeInfo;
}

const isParameterOptional = (parameterDeclaration: ts.ParameterDeclaration): boolean => {
    return !!parameterDeclaration.questionToken
        && parameterDeclaration.questionToken.kind == ts.SyntaxKind.QuestionToken
};

export const extractParameters = (member: ts.NodeArray<ts.ParameterDeclaration>): ParameterInfo[] => {
    const parameters: ParameterInfo[] = [];

    member.forEach(parameter => {
        if (!ts.isIdentifier(parameter.name) || !parameter.type) {
            return;
        }

        parameters.push({
            name: parameter.name.text,
            isOptional: isParameterOptional(parameter),
            type: extractTypeInfo(parameter.type),
        });
    });

    return parameters;
};