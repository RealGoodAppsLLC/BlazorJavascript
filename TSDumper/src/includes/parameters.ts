import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";

export interface ParameterInfo {
    name: string;
    isOptional: boolean;
    isDotDotDot: boolean;
    type: TypeInfo;
}

const isParameterOptional = (parameterDeclaration: ts.ParameterDeclaration): boolean => {
    return !!parameterDeclaration.questionToken
        && parameterDeclaration.questionToken.kind == ts.SyntaxKind.QuestionToken
};

const isParameterDotDotDot = (parameterDeclaration: ts.ParameterDeclaration): boolean => {
    return !!parameterDeclaration.dotDotDotToken
        && parameterDeclaration.dotDotDotToken.kind == ts.SyntaxKind.DotDotDotToken
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
            isDotDotDot: isParameterDotDotDot(parameter),
            type: extractTypeInfo(parameter.type),
        });
    });

    return parameters;
};