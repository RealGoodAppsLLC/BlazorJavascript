import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";

export interface TypeParameter {
    name: string;
    default: TypeInfo | null;
    constraint: TypeInfo | null;
}

export interface ExtractTypeParametersResult {
    typeParameters: TypeParameter[];
    anyConstraintsAreNotSimple: boolean;
}

export const extractTypeParameters = (
    typeParameterDeclarations: ts.NodeArray<ts.TypeParameterDeclaration> | null | undefined
): ExtractTypeParametersResult => {
    const typeParameters: TypeParameter[] = [];

    if (!typeParameterDeclarations) {
        return {
            typeParameters: typeParameters,
            anyConstraintsAreNotSimple: false,
        };
    }

    // FIXME: Any function that has something like this is ignored for now:
    //        foo<K extends keyof Bar>(type: K)
    //        In the future, we should consider replacing the generic and parameters with a string.
    let anyConstraintsAreNotSimple = false;

    if (!!typeParameterDeclarations) {
        typeParameterDeclarations.forEach(typeParameter => {
            if (!ts.isIdentifier(typeParameter.name)) {
                return;
            }

            let constraint: TypeInfo | null = null;

            if (!!typeParameter.constraint) {
                if (ts.isTypeOperatorNode(typeParameter.constraint)) {
                    anyConstraintsAreNotSimple = true;
                    return;
                }

                constraint = extractTypeInfo(typeParameter.constraint);
            }

            let defaultTypeInfo: TypeInfo | null = null;

            if (!!typeParameter.default) {
                defaultTypeInfo = extractTypeInfo(typeParameter.default);
            }

            typeParameters.push({
                name: typeParameter.name.text,
                default: defaultTypeInfo,
                constraint: constraint,
            });
        });
    }

    return {
        typeParameters: typeParameters,
        anyConstraintsAreNotSimple: anyConstraintsAreNotSimple,
    };
};