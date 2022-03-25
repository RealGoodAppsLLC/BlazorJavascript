import {SemanticVersion, SemanticVersionDiff} from './semanticversion';

function setOutput(
  currentInteropVersion: SemanticVersion,
  newInteropVersion: SemanticVersion,
  currentTypescriptVersion: SemanticVersion,
  newTypescriptVersion: SemanticVersion, bumpType: string) {
    console.log(`::set-output name=blazor_current_interop_version::${currentInteropVersion}`);
    console.log(`::set-output name=blazor_new_interop_version::${newInteropVersion}`);
    console.log(`::set-output name=blazor_current_typescript_version::${currentTypescriptVersion}`);
    console.log(`::set-output name=blazor_new_typescript_version::${newTypescriptVersion}`);
    console.log(`::set-output name=blazor_bump_type::${bumpType}`);
}

if (process.argv.length < 5) {
    console.log('Invalid arguments passed.');
    process.exit(1);
}

const tsDumperVersion = SemanticVersion.fromString(process.argv[2]);
const latestTsVersion = SemanticVersion.fromString(process.argv[3]);
const interopVersion = SemanticVersion.fromString(process.argv[4]);
const versionDiff = tsDumperVersion.compare(latestTsVersion);

let bumpedVersion: SemanticVersion | null = null;
switch (versionDiff) {
    case SemanticVersionDiff.Major: {
        bumpedVersion = interopVersion.bumpedMajorVersion();
        break;
    }
    case SemanticVersionDiff.Minor: {
        bumpedVersion = interopVersion.bumpedMinorVersion();
        break;
    }
    case SemanticVersionDiff.Patch: {
        bumpedVersion = interopVersion.bumpedMinorVersion();
        break;
    }
    case SemanticVersionDiff.Equal:
        bumpedVersion = interopVersion;
        break;
    default:
        console.log('An unknown error occurred.');
        process.exit(1);
}

if (bumpedVersion !== null) {
    setOutput(interopVersion, bumpedVersion, tsDumperVersion, latestTsVersion, SemanticVersionDiff[versionDiff].toLowerCase());
}
