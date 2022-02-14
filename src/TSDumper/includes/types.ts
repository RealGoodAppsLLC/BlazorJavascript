import * as ts from "typescript";
import { extractTypeParameters, ExtractTypeParametersResult } from "./typeparameters";
import { extractParameters, ParameterInfo } from "./parameters";
import { extractTypeArguments } from "./typearguments";

export interface StringLiteralInfo {
    value: string;
}

export interface BooleanLiteralInfo {
    value: boolean;
}

export interface NumberLiteralInfo {
    value: number;
}

export interface TypeInfo {
    union: UnionTypeInfo | null;
    intersection: IntersectionTypeInfo | null;
    parenthesized: TypeInfo | null;
    single: SingleTypeInfo | null;
    function: FunctionTypeInfo | null;
    array: TypeInfo | null;
}

export interface UnionTypeInfo {
    types: TypeInfo[];
}

export interface IntersectionTypeInfo {
    types: TypeInfo[];
}

export interface SingleTypeInfo {
    name: string | null;
    stringLiteral: StringLiteralInfo | null;
    booleanLiteral: BooleanLiteralInfo | null;
    numberLiteral: NumberLiteralInfo | null;
    typeArguments: TypeInfo[];
    isUnhandled: boolean;
}

export interface FunctionTypeInfo {
    extractTypeParametersResult: ExtractTypeParametersResult;
    parameters: ParameterInfo[];
    returnType: TypeInfo;
}

const extractSingleTypeInfo = (typeNode: ts.TypeNode): SingleTypeInfo => {
    if (ts.isTypeReferenceNode(typeNode)
        && ts.isIdentifier(typeNode.typeName)) {
        return {
            name: typeNode.typeName.text,
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: extractTypeArguments(typeNode),
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.NumberKeyword) {
        return {
            name: "number",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.StringKeyword) {
        return {
            name: "string",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.BooleanKeyword) {
        return {
            name: "boolean",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.AnyKeyword) {
        return {
            name: "any",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.UndefinedKeyword) {
        return {
            name: "undefined",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (typeNode.kind === ts.SyntaxKind.VoidKeyword) {
        return {
            name: "void",
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: false,
        };
    }

    if (ts.isLiteralTypeNode(typeNode)) {
        if (typeNode.literal.kind === ts.SyntaxKind.NullKeyword) {
            return {
                name: "null",
                stringLiteral: null,
                booleanLiteral: null,
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        if (typeNode.literal.kind === ts.SyntaxKind.TrueKeyword) {
            return {
                name: null,
                stringLiteral: null,
                booleanLiteral: {
                    value: true,
                },
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        if (typeNode.literal.kind === ts.SyntaxKind.FalseKeyword) {
            return {
                name: null,
                stringLiteral: null,
                booleanLiteral: {
                    value: false,
                },
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        if (ts.isStringLiteral(typeNode.literal)) {
            return {
                name: null,
                stringLiteral: {
                    value: typeNode.literal.text,
                },
                booleanLiteral: null,
                numberLiteral: null,
                typeArguments: [],
                isUnhandled: false,
            };
        }

        return {
            name: `unhandled_literal:${typeNode.literal.kind}`,
            stringLiteral: null,
            booleanLiteral: null,
            numberLiteral: null,
            typeArguments: [],
            isUnhandled: true,
        };
    }

    return {
        name: `unhandled:${typeNode.kind}`,
        stringLiteral: null,
        booleanLiteral: null,
        numberLiteral: null,
        typeArguments: [],
        isUnhandled: true,
    };
};

export const extractTypeInfo = (typeNode: ts.TypeNode): TypeInfo => {
    if (ts.isUnionTypeNode(typeNode)) {
        const unionTypes: TypeInfo[] = [];

        typeNode.types.forEach(unionChild => {
            unionTypes.push(extractTypeInfo(unionChild));
        });

        return {
            union: {
                types: unionTypes,
            },
            intersection: null,
            parenthesized: null,
            single: null,
            function: null,
            array: null,
        }
    }

    if (ts.isIntersectionTypeNode(typeNode)) {
        const intersectionTypes: TypeInfo[] = [];

        typeNode.types.forEach(intersectionChild => {
            intersectionTypes.push(extractTypeInfo(intersectionChild));
        });

        return {
            union: null,
            intersection: {
                types: intersectionTypes,
            },
            parenthesized: null,
            single: null,
            function: null,
            array: null,
        }
    }

    if (ts.isParenthesizedTypeNode(typeNode)) {
        return {
            union: null,
            intersection: null,
            parenthesized: extractTypeInfo(typeNode.type),
            single: null,
            function: null,
            array: null,
        }
    }

    if (ts.isFunctionTypeNode(typeNode)) {
        return {
            union: null,
            intersection: null,
            parenthesized: null,
            single: null,
            function: {
                parameters: extractParameters(typeNode.parameters),
                extractTypeParametersResult: extractTypeParameters(typeNode.typeParameters),
                returnType: extractTypeInfo(typeNode.type)
            },
            array: null,
        }
    }

    if (ts.isArrayTypeNode(typeNode)) {
        return {
            union: null,
            intersection: null,
            parenthesized: null,
            single: null,
            function: null,
            array: extractTypeInfo(typeNode.elementType),
        }
    }

    return {
        union: null,
        intersection: null,
        parenthesized: null,
        single: extractSingleTypeInfo(typeNode),
        function: null,
        array: null,
    };
};
