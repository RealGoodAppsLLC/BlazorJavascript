import { GlobalVariableInfo } from "./globalvariables";
import { InterfaceInfo } from "./interfaces";

export interface ParsedInfo {
    globalVariables: GlobalVariableInfo[];
    interfaces: InterfaceInfo[];
}