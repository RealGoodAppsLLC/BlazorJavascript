import {expect} from 'chai';
import 'mocha';
import {SemanticVersion, SemanticVersionDiff} from '../src/semanticversion';

describe('SemanticVersion', function() {
  const validSemanticVersionStrings = [
    '1.0.0',
    '0.0.1',
    '0.0.0',
    '1.2.3'
  ];

  describe("fromString", function () {
    const invalidSemanticVersionStrings = [
      '',
      'invalid',
      '1',
      '1.2',
      '1.2.3.4',
      'x.1.2',
      '1.x.2',
      '1.2.x'
    ];

    invalidSemanticVersionStrings.forEach(versionString => {
      it(`should throw from parsing invalid version string "${versionString}"`, function () {
        expect(function() {
          SemanticVersion.fromString(versionString);
        }).to.throw(TypeError, `The version string "${versionString}" is not a semantic version.`);
      });
    });

    validSemanticVersionStrings.forEach(versionString => {
      it(`should not throw from parsing valid version string "${versionString}"`, function () {
        expect(function() {
          SemanticVersion.fromString(versionString);
        }).to.not.throw();
      });
    });
  });

  describe('toString', function() {
    validSemanticVersionStrings.forEach(versionString => {
      it("should equal the original version string", function () {
        const semanticVersion = SemanticVersion.fromString(versionString);
        expect(semanticVersion.toString()).to.equal(versionString);
      });
    });
  });

  describe('getMajor', function() {
    it('should equal 2 given the version 2.5.1', function() {
      const semanticVersion = new SemanticVersion(2, 5, 1);
      expect(semanticVersion.getMajor()).to.equal(2);
    });
  });

  describe('getMinor', function() {
    it('should equal 5 given the version 2.5.1', function() {
      const semanticVersion = new SemanticVersion(2, 5, 1);
      expect(semanticVersion.getMinor()).to.equal(5);
    });
  });

  describe('getPatch', function() {
    it('should equal 1 given the version 2.5.1', function() {
      const semanticVersion = new SemanticVersion(2, 5, 1);
      expect(semanticVersion.getPatch()).to.equal(1);
    });
  });

  describe('bumpedMajorVersion', function() {
    it('should equal 3.0.0 given the version 2.5.1', function() {
      const semanticVersion = new SemanticVersion(2, 5, 1);
      const bumpedSemanticVersion = semanticVersion.bumpedMajorVersion();
      expect(bumpedSemanticVersion.toString()).to.equal('3.0.0');
    });
  });

  describe('bumpedMinorVersion', function() {
    it('should equal 2.6.0 given the version 2.5.1', function() {
      const semanticVersion = new SemanticVersion(2, 5, 1);
      const bumpedSemanticVersion = semanticVersion.bumpedMinorVersion();
      expect(bumpedSemanticVersion.toString()).to.equal('2.6.0');
    });
  });

  describe('bumpedPatchVersion', function() {
    it('should equal 2.5.2 given the version 2.5.1', function() {
      const semanticVersion = new SemanticVersion(2, 5, 1);
      const bumpedSemanticVersion = semanticVersion.bumpedPatchVersion();
      expect(bumpedSemanticVersion.toString()).to.equal('2.5.2');
    });
  });

  describe('compare', function() {
    it('should equal SemanticVersionDiff.Equal given two equal versions', function () {
      const semanticVersionA = new SemanticVersion(1, 2, 3);
      const semanticVersionB = new SemanticVersion(1, 2, 3);
      expect(semanticVersionA.compare(semanticVersionB)).to.equal(SemanticVersionDiff.Equal);
    });

    it('should equal SemanticVersionDiff.Major given different major versions', function () {
      const semanticVersionA = new SemanticVersion(1, 2, 3);
      const semanticVersionB = new SemanticVersion(0, 2, 3);
      expect(semanticVersionA.compare(semanticVersionB)).to.equal(SemanticVersionDiff.Major);
    });

    it('should equal SemanticVersionDiff.Major given different major, minor and patch versions', function () {
      const semanticVersionA = new SemanticVersion(0, 2, 1);
      const semanticVersionB = new SemanticVersion(23, 5, 3);
      expect(semanticVersionA.compare(semanticVersionB)).to.equal(SemanticVersionDiff.Major);
    });

    it('should equal SemanticVersionDiff.Minor given different minor versions', function () {
      const semanticVersionA = new SemanticVersion(5, 88, 133);
      const semanticVersionB = new SemanticVersion(5, 87, 133);
      expect(semanticVersionA.compare(semanticVersionB)).to.equal(SemanticVersionDiff.Minor);
    });

    it('should equal SemanticVersionDiff.Minor given different minor and patch versions', function () {
      const semanticVersionA = new SemanticVersion(5, 88, 12);
      const semanticVersionB = new SemanticVersion(5, 87, 133);
      expect(semanticVersionA.compare(semanticVersionB)).to.equal(SemanticVersionDiff.Minor);
    });

    it('should equal SemanticVersionDiff.Patch given different patch versions', function () {
      const semanticVersionA = new SemanticVersion(5, 88, 12);
      const semanticVersionB = new SemanticVersion(5, 88, 133);
      expect(semanticVersionA.compare(semanticVersionB)).to.equal(SemanticVersionDiff.Patch);
    });
  });
});
