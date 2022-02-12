import * as fs from "fs";
const ts = require('typescript');

const inputTypeDefinitions = [
    'lib.dom.d',
];

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

fs.mkdirSync('output', {recursive: true});

inputTypeDefinitions.forEach(inputTypeDefinition => {
    const inputPath = `node_modules/typescript/lib/${inputTypeDefinition}.ts`;
    const outputPath = `output/${inputTypeDefinition}.json`;

    console.log(`Dumping AST for "${inputPath}" to "${outputPath}"...`);

    const node = ts.createSourceFile(
        'x.ts',
        fs.readFileSync(inputPath, {encoding:'utf8', flag:'r'}),
        ts.ScriptTarget.Latest
    );

    fs.writeFileSync(outputPath, JSON.stringify(node, getCircularReplacer()));
});
