import { ExtractTypeParametersResult } from "./typeparameters";
import { TypeInfo } from "./types";
import { ParameterInfo } from "./parameters";

export interface ConstructorInfo {
    returnType: TypeInfo;
    extractTypeParametersResult: ExtractTypeParametersResult;
    parameters: ParameterInfo[];
}