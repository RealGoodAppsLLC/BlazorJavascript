import { ParameterInfo, TypeInfo } from "./interfaces";
import {ExtractTypeParametersResult} from "./typeparameters";

export interface ConstructorInfo {
    returnType: TypeInfo;
    extractTypeParametersResult: ExtractTypeParametersResult;
    parameters: ParameterInfo[];
}