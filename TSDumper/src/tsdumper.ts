import * as fs from "fs";
import * as ts from "typescript";
import { ParsedInfo} from "./includes/parsed";
import { extractGlobalVariables } from "./includes/globalvariables";
import { extractInterfaces } from "./includes/interfaces";
import { extractTypeAliases } from "./includes/typealias";
import { runPostProcessing } from "./includes/postprocess";
import { convertToJson } from "./includes/json";

const inputTypeDefinitions = [
    'lib.dom.d',
    'lib.es5.d',
    'lib.es2015.promise.d',
];

const isPrettyPrint = process.argv.indexOf('--pretty') !== -1;
const isDebugMode = process.argv.indexOf('--debug') !== -1;

fs.mkdirSync('output', {recursive: true});

inputTypeDefinitions.forEach(inputTypeDefinition => {
    const inputPath = `node_modules/typescript/lib/${inputTypeDefinition}.ts`;
    const outputPath = `output/${inputTypeDefinition}.json`;

    console.log(`Dumping AST for "${inputPath}" to "${outputPath}"...`);

    const sourceFile: ts.SourceFile = ts.createSourceFile(
        'x.ts',
        fs.readFileSync(inputPath, {encoding:'utf8', flag:'r'}),
        ts.ScriptTarget.Latest
    );

    const parsedInfo: ParsedInfo = {
        globalVariables: extractGlobalVariables(sourceFile),
        interfaces: extractInterfaces(sourceFile),
        typeAliases: extractTypeAliases(sourceFile),
    };

    const postProcessedInfo = runPostProcessing(parsedInfo);

    if (isDebugMode) {
        (postProcessedInfo as any).raw = sourceFile;
    }

    fs.writeFileSync(outputPath, convertToJson(postProcessedInfo, isPrettyPrint));
});
