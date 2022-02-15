import { GlobalVariableInfo } from "./globalvariables";
import { InterfaceInfo } from "./interfaces";
import { TypeAliasInfo } from "./typealias";

export interface ParsedInfo {
    globalVariables: GlobalVariableInfo[];
    interfaces: InterfaceInfo[];
    typeAliases: TypeAliasInfo[];
}
