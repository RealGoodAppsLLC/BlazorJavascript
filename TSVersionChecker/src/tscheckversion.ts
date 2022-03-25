import * as fs from "fs";

interface Version {
    Major: number;
    Minor: number;
    Patch: number;
}

enum VersionDiff {
    Equal,
    Patch,
    Minor,
    Major
}

function parseVersion(versionString: String): Version {
    const parts = versionString.split('.');
    return {
        Major: parseInt(parts[0], 10),
        Minor: parseInt(parts[1], 10),
        Patch: parseInt(parts[2], 10)
    };
}

function compareVersions(versionA: Version, versionB: Version): VersionDiff {
    if (versionA.Major !== versionB.Major) {
        return VersionDiff.Major;
    }

    if (versionA.Minor !== versionB.Minor) {
        return VersionDiff.Minor;
    }

    if (versionA.Patch !== versionB.Patch) {
        return VersionDiff.Patch;
    }
    
    return VersionDiff.Equal;
}

function bumpMinorVersion(interopVersion: Version): Version {
    return {
        Major: interopVersion.Major,
        Minor: interopVersion.Minor + 1,
        Patch: 0
    }
}

function bumpMajorVersion(interopVersion: Version): Version {
    return {
        Major: interopVersion.Major + 1,
        Minor: 0,
        Patch: 0
    }
}

function versionToString(version: Version): string {
    return `${version.Major}.${version.Minor}.${version.Patch}`;
}

function setOutput(currentInteropVersion: Version, newInteropVersion: Version, currentTypescriptVersion: Version, newTypescriptVersion: Version, bumpType: string) {
    console.log(`::set-output name=blazor_current_interop_version::${versionToString(currentInteropVersion)}`);
    console.log(`::set-output name=blazor_new_interop_version::${versionToString(newInteropVersion)}`);
    console.log(`::set-output name=blazor_current_typescript_version::${versionToString(currentTypescriptVersion)}`);
    console.log(`::set-output name=blazor_new_typescript_version::${versionToString(newTypescriptVersion)}`);
    console.log(`::set-output name=blazor_bump_type::${bumpType}`);
}

if (process.argv.length < 5) {
    console.log('Invalid arguments passed.');
    process.exit(1);
}

const tsDumperVersion = parseVersion(process.argv[2]);
const latestTsVersion = parseVersion(process.argv[3]);
const interopVersion = parseVersion(process.argv[4]);
const versionDiff = compareVersions(tsDumperVersion, latestTsVersion);

switch (versionDiff) {
    case VersionDiff.Major: {
        const bumpedVersion = bumpMajorVersion(interopVersion);
        setOutput(interopVersion, bumpedVersion, tsDumperVersion, latestTsVersion, VersionDiff[versionDiff].toLowerCase());
        break;
    }
    case VersionDiff.Minor {
        const bumpedVersion = bumpMinorVersion(interopVersion);
        setOutput(interopVersion, bumpedVersion, tsDumperVersion, latestTsVersion, VersionDiff[versionDiff].toLowerCase());
        break;
    }
    case VersionDiff.Patch: {
        const bumpedVersion = bumpMinorVersion(interopVersion);
        setOutput(interopVersion, bumpedVersion, tsDumperVersion, latestTsVersion, VersionDiff[versionDiff].toLowerCase());
        break;
    }
    case VersionDiff.Equal:
        setOutput(interopVersion, interopVersion, tsDumperVersion, latestTsVersion, VersionDiff[versionDiff].toLowerCase());
        break;
    default:
        console.log('An unknown error occurred.');
        process.exit(1);
}