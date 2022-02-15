import * as ts from "typescript";
import { extractTypeInfo, TypeInfo } from "./types";

export interface PropertyInfo {
    name: string;
    isReadonly: boolean;
    type: TypeInfo;
}

export const extractProperties = (members: ts.NodeArray<ts.TypeElement>): PropertyInfo[] => {
    const properties: PropertyInfo[] = [];

    members.forEach(member => {
        if (!ts.isPropertySignature(member) || !ts.isIdentifier(member.name) || !member.type) {
            return;
        }

        // FIXME: Ignore properties that are named "prototype" to make our life easier.
        //        There might be a better way to do this.
        if (member.name.text === "prototype") {
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

        properties.push({
            name: member.name.text,
            type: extractTypeInfo(member.type),
            isReadonly: isReadonly,
        });
    });

    return properties;
};