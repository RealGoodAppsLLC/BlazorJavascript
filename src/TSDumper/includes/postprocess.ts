import { TypeInfo } from "./types";
import { ParsedInfo } from "./parsed";
import { GlobalVariableInfo } from "./globalvariables";
import { InterfaceInfo } from "./interfaces";
import { ConstructorInfo } from "./constructors";
import { PropertyInfo } from "./properties";
import { MethodInfo } from "./methods";
import { IndexerInfo } from "./indexers";
import { GetAccessorInfo, SetAccessorInfo } from "./accessors";

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

export const runPostProcessing = (parsedInfo: ParsedInfo): ParsedInfo => {
    let postProcessedGlobalVariables: GlobalVariableInfo[] = [];
    let postProcessedInterfaces: InterfaceInfo[] = [];

    parsedInfo.globalVariables.forEach(globalVariable => {
        const postProcessedConstructors: ConstructorInfo[] = [];
        const postProcessedProperties: PropertyInfo[] = [];

        globalVariable.constructors.forEach(constructor => {
            let keepConstructor = true;

            constructor.parameters.forEach(constructorParameter => {
                if (recursiveCheckForNonSimpleTypeArgument(constructorParameter.type)) {
                    keepConstructor = false;
                }
            });

            if (keepConstructor) {
                postProcessedConstructors.push(constructor);
            }
        });

        globalVariable.properties.forEach(property => {
            if (recursiveCheckForNonSimpleTypeArgument(property.type)) {
                return;
            }

            postProcessedProperties.push(property);
        });

        const postProcessedGlobalVariable: GlobalVariableInfo = {
            name: globalVariable.name,
            hasPrototype: globalVariable.hasPrototype,
            constructors: postProcessedConstructors,
            properties: postProcessedProperties,
        };

        postProcessedGlobalVariables.push(postProcessedGlobalVariable);
    });

    parsedInfo.interfaces.forEach(interfaceInfo => {
        if (interfaceInfo.extractTypeParametersResult.anyConstraintsAreNotSimple) {
            return;
        }

        const postProcessedMethods: MethodInfo[] = [];
        const postProcessedProperties: PropertyInfo[] = [];

        interfaceInfo.methods.forEach(methodInfo => {
            let keepMethod = true;

            if (methodInfo.extractTypeParametersResult.anyConstraintsAreNotSimple) {
                keepMethod = false;
            }
            else if (recursiveCheckForNonSimpleTypeArgument(methodInfo.returnType)) {
                keepMethod = false;
            }
            else {
                methodInfo.parameters.forEach(parameterInfo => {
                    if (recursiveCheckForNonSimpleTypeArgument(parameterInfo.type)) {
                        keepMethod = false;
                    }
                });
            }

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

        const postProcessedIndexers: IndexerInfo[] = [];

        interfaceInfo.indexers.forEach(indexer => {
            if (recursiveCheckForNonSimpleTypeArgument(indexer.indexType)
                || recursiveCheckForNonSimpleTypeArgument(indexer.returnType)) {
                return;
            }

            postProcessedIndexers.push(indexer);
        });

        const postProcessedGetAccessors: GetAccessorInfo[] = [];

        interfaceInfo.getAccessors.forEach(getAccessor => {
            if (recursiveCheckForNonSimpleTypeArgument(getAccessor.returnType)) {
                return;
            }

            postProcessedGetAccessors.push(getAccessor);
        });

        const postProcessedSetAccessors: SetAccessorInfo[] = [];

        interfaceInfo.setAccessors.forEach(setAccessor => {
            let keepSetAccessor = true;

            setAccessor.parameters.forEach(parameter => {
                if (recursiveCheckForNonSimpleTypeArgument(parameter.type)) {
                    keepSetAccessor = false;
                    return;
                }
            });

            if (keepSetAccessor) {
                postProcessedSetAccessors.push(setAccessor);
            }
        });

        postProcessedInterfaces.push({
            name: interfaceInfo.name,
            methods: postProcessedMethods,
            extractTypeParametersResult: interfaceInfo.extractTypeParametersResult,
            extendsList: interfaceInfo.extendsList,
            properties: postProcessedProperties,
            indexers: postProcessedIndexers,
            getAccessors: postProcessedGetAccessors,
            setAccessors: postProcessedSetAccessors
        });
    });

    return {
        globalVariables: postProcessedGlobalVariables,
        interfaces: postProcessedInterfaces,
    };
};