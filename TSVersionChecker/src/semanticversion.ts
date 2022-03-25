export enum SemanticVersionDiff {
  Equal,
  Patch,
  Minor,
  Major
}

export class SemanticVersion {
  private readonly major: number;
  private readonly minor: number;
  private readonly patch: number;

  constructor(major: number, minor: number, patch: number) {
    this.major = major;
    this.minor = minor;
    this.patch = patch;
  }

  getMajor(): number {
    return this.major;
  }

  getMinor(): number {
    return this.minor;
  }

  getPatch(): number {
    return this.patch;
  }

  static fromString(versionString: string): SemanticVersion {
    const parts = versionString.split('.');

    if (parts.length !== 3) {
      throw new TypeError(`The version string "${versionString}" is not a semantic version.`)
    }

    const major = parseInt(parts[0], 10);

    if (isNaN(major)) {
      throw new TypeError(`The version string "${versionString}" is not a semantic version.`)
    }

    const minor = parseInt(parts[1], 10);

    if (isNaN(minor)) {
      throw new TypeError(`The version string "${versionString}" is not a semantic version.`)
    }

    const patch = parseInt(parts[2], 10);

    if (isNaN(patch)) {
      throw new TypeError(`The version string "${versionString}" is not a semantic version.`)
    }

    return new SemanticVersion(major, minor, patch);
  }

  toString(): string {
    return `${this.major}.${this.minor}.${this.patch}`;
  }

  bumpedMajorVersion(): SemanticVersion {
    return new SemanticVersion(this.major + 1, 0, 0);
  }

  bumpedMinorVersion(): SemanticVersion {
    return new SemanticVersion(this.major, this.minor + 1, 0);
  }

  bumpedPatchVersion(): SemanticVersion {
    return new SemanticVersion(this.major, this.minor, this.patch + 1);
  }

  compare(other: SemanticVersion): SemanticVersionDiff {
    if (other.major !== this.major) {
      return SemanticVersionDiff.Major;
    }

    if (other.minor !== this.minor) {
      return SemanticVersionDiff.Minor;
    }

    if (other.patch !== this.patch) {
      return SemanticVersionDiff.Patch;
    }

    return SemanticVersionDiff.Equal;
  }
}
