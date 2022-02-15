import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";

export const extractTypeArguments = (typeNode: ts.TypeReferenceNode): TypeInfo[] => {
    const typeArguments: TypeInfo[] = [];

    if (!!typeNode.typeArguments) {
        typeNode.typeArguments.forEach(typeArgument => {
            typeArguments.push(extractTypeInfo(typeArgument));
        });
    }

    return typeArguments;
};