import { ParameterInfo, TypeInfo } from "./interfaces";

export interface ConstructorInfo {
    returnType: TypeInfo;
    parameters: ParameterInfo[];
}