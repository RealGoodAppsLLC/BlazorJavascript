import { TypeInfo } from "./types";
import { ParsedInfo } from "./parsed";
import { GlobalVariableInfo } from "./globalvariables";
import { InterfaceBodyInfo, InterfaceInfo } from "./interfaces";
import { ConstructorInfo } from "./constructors";
import { PropertyInfo } from "./properties";
import { MethodInfo } from "./methods";
import { IndexerInfo } from "./indexers";
import { GetAccessorInfo, SetAccessorInfo } from "./accessors";
import {TypeAliasInfo} from "./typealias";

const recursiveCheckForNonSimpleTypeArgument = (result: TypeInfo): boolean => {
    let anyNodeNotSimple = false;

    if (result.single !== null) {
        result.single.typeArguments.every(typeArgument => {
            if (recursiveCheckForNonSimpleTypeArgument(typeArgument)) {
                anyNodeNotSimple = true;
                return false;
            }

            return true;
        });
    }

    if (result.union !== null) {
        result.union.types.every(unionTypeChild => {
            if (recursiveCheckForNonSimpleTypeArgument(unionTypeChild)) {
                anyNodeNotSimple = true;
                return false;
            }

            return true;
        });
    }

    if (result.intersection !== null) {
        result.intersection.types.every(intersectionTypeChild => {
            if (recursiveCheckForNonSimpleTypeArgument(intersectionTypeChild)) {
                anyNodeNotSimple = true;
                return false;
            }

            return true;
        });
    }

    if (result.function !== null) {
        if (result.function.extractTypeParametersResult.anyConstraintsAreNotSimple
            || recursiveCheckForNonSimpleTypeArgument(result.function.returnType)) {
            anyNodeNotSimple = true;
        }
        else {
            result.function.parameters.every(parameterInfo => {
                if (recursiveCheckForNonSimpleTypeArgument(parameterInfo.type)) {
                    anyNodeNotSimple = true;
                    return false;
                }

                return true;
            });
        }
    }

    return anyNodeNotSimple;
};

const postProcessShouldKeepConstructor = (constructor: ConstructorInfo): boolean => {
    if (constructor.extractTypeParametersResult.anyConstraintsAreNotSimple) {
        return false;
    }

    return !constructor.parameters.some(constructorParameter => {
        return recursiveCheckForNonSimpleTypeArgument(constructorParameter.type);
    });
};

const postProcessShouldKeepMethod = (methodInfo: MethodInfo): boolean => {
    if (methodInfo.extractTypeParametersResult.anyConstraintsAreNotSimple) {
        return false;
    }

    if (recursiveCheckForNonSimpleTypeArgument(methodInfo.returnType)) {
        return false;
    }

    return !methodInfo.parameters.some(parameterInfo => {
        return recursiveCheckForNonSimpleTypeArgument(parameterInfo.type);
    });
};

const postProcessShouldKeepSetAccessor = (setAccessor: SetAccessorInfo): boolean => {
    return !setAccessor.parameters.some(parameter => {
        return recursiveCheckForNonSimpleTypeArgument(parameter.type);
    });
};

export const runPostProcessingInterfaceBody = (interfaceInfo: InterfaceBodyInfo): InterfaceBodyInfo => {
    const postProcessedMethods: MethodInfo[] = [];
    const postProcessedProperties: PropertyInfo[] = [];
    const postProcessedConstructors: ConstructorInfo[] = [];
    const postProcessedIndexers: IndexerInfo[] = [];
    const postProcessedGetAccessors: GetAccessorInfo[] = [];
    const postProcessedSetAccessors: SetAccessorInfo[] = [];

    interfaceInfo.constructors.forEach(constructor => {
        let keepConstructor = postProcessShouldKeepConstructor(constructor);

        if (keepConstructor) {
            postProcessedConstructors.push(constructor);
        }
    });

    interfaceInfo.methods.forEach(methodInfo => {
        let keepMethod = postProcessShouldKeepMethod(methodInfo);

        if (keepMethod) {
            postProcessedMethods.push(methodInfo);
        }
    });

    interfaceInfo.properties.forEach(property => {
        if (recursiveCheckForNonSimpleTypeArgument(property.type)) {
            return;
        }

        postProcessedProperties.push(property);
    });

    interfaceInfo.indexers.forEach(indexer => {
        if (recursiveCheckForNonSimpleTypeArgument(indexer.indexType)
            || recursiveCheckForNonSimpleTypeArgument(indexer.returnType)) {
            return;
        }

        postProcessedIndexers.push(indexer);
    });

    interfaceInfo.getAccessors.forEach(getAccessor => {
        if (recursiveCheckForNonSimpleTypeArgument(getAccessor.returnType)) {
            return;
        }

        postProcessedGetAccessors.push(getAccessor);
    });

    interfaceInfo.setAccessors.forEach(setAccessor => {
        let keepSetAccessor = postProcessShouldKeepSetAccessor(setAccessor);

        if (keepSetAccessor) {
            postProcessedSetAccessors.push(setAccessor);
        }
    });

    return {
        methods: postProcessedMethods,
        constructors: postProcessedConstructors,
        properties: postProcessedProperties,
        indexers: postProcessedIndexers,
        getAccessors: postProcessedGetAccessors,
        setAccessors: postProcessedSetAccessors
    };
};

export const runPostProcessing = (parsedInfo: ParsedInfo): ParsedInfo => {
    let postProcessedGlobalVariables: GlobalVariableInfo[] = [];
    let postProcessedInterfaces: InterfaceInfo[] = [];
    let postProcessedTypeAliases: TypeAliasInfo[] = [];

    parsedInfo.globalVariables.forEach(globalVariable => {
        if (globalVariable.type !== null && recursiveCheckForNonSimpleTypeArgument(globalVariable.type)) {
            return;
        }

        let postProcessedInlineInterface: InterfaceBodyInfo | null = null;

        if (globalVariable.inlineInterface !== null) {
            postProcessedInlineInterface = runPostProcessingInterfaceBody(globalVariable.inlineInterface);
        }

        postProcessedGlobalVariables.push({
            name: globalVariable.name,
            type: globalVariable.type,
            inlineInterface: postProcessedInlineInterface,
        });
    });

    parsedInfo.interfaces.forEach(interfaceInfo => {
        if (interfaceInfo.extractTypeParametersResult.anyConstraintsAreNotSimple) {
            return;
        }

        postProcessedInterfaces.push({
            name: interfaceInfo.name,
            extractTypeParametersResult: interfaceInfo.extractTypeParametersResult,
            extendsList: interfaceInfo.extendsList,
            body: runPostProcessingInterfaceBody(interfaceInfo.body)
        });
    });

    parsedInfo.typeAliases.forEach(typeAliasInfo => {
        if (typeAliasInfo.extractTypeParametersResult.anyConstraintsAreNotSimple) {
            return;
        }

        postProcessedTypeAliases.push(typeAliasInfo);
    });

    return {
        globalVariables: postProcessedGlobalVariables,
        interfaces: postProcessedInterfaces,
        typeAliases: postProcessedTypeAliases,
    };
};
