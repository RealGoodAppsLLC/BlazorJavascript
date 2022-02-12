import { ParsedInfo } from "./parsed";

const getCircularReplacer = () => {
    const seen = new WeakSet();
    return (key: any, value: any) => {
        if (typeof value === "object" && value !== null) {
            if (seen.has(value)) {
                return;
            }
            seen.add(value);
        }
        return value;
    };
};

export const convertToJson = (postProcessedInfo: ParsedInfo, isPrettyMode: boolean): string => {
    return JSON.stringify(postProcessedInfo, getCircularReplacer(), isPrettyMode ? 2 : undefined);
};