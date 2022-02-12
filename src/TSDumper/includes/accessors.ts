import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";
import { extractParameters, ParameterInfo } from "./parameters";

export interface GetAccessorInfo {
    name: string;
    returnType: TypeInfo;
}

export interface SetAccessorInfo {
    name: string;
    parameters: ParameterInfo[];
}

export const extractGetAccessors = (members: ts.NodeArray<ts.TypeElement>): GetAccessorInfo[] => {
    const getAccessors: GetAccessorInfo[] = [];

    members.forEach(member => {
        if (!ts.isGetAccessor(member) || !ts.isIdentifier(member.name) || !member.type) {
            return;
        }

        getAccessors.push({
            name: member.name.text,
            returnType: extractTypeInfo(member.type)
        });
    });

    return getAccessors;
}

export const extractSetAccessors = (members: ts.NodeArray<ts.TypeElement>): SetAccessorInfo[] => {
    const setAccessors: SetAccessorInfo[] = [];

    members.forEach(member => {
        if (!ts.isSetAccessor(member) || !ts.isIdentifier(member.name)) {
            return;
        }

        setAccessors.push({
            name: member.name.text,
            parameters: extractParameters(member.parameters)
        });
    });

    return setAccessors;
}