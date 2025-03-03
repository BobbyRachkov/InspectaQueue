using Version = Rachkov.InspectaQueue.AutoUpdater.Core.Version;

namespace AutoUpdater.Tests;

[TestFixture]
public class VersionTests
{
    [TestCase("1.0.0", "2.0.0", true)]
    [TestCase("2.0.0", "1.0.0", false)]
    [TestCase("1.0.0", "1.1.0", true)]
    [TestCase("1.1.0", "1.0.0", false)]
    [TestCase("1.0.0", "1.0.1", true)]
    [TestCase("1.0.1", "1.0.0", false)]
    [TestCase("1.0.0", "1.0.0", false)]
    public void GivenTwoVersions_WhenComparingLessThan_ThenReturnsExpectedResult(string version1, string version2, bool expected)
    {
        // Given
        var v1 = new Version(version1);
        var v2 = new Version(version2);

        // When
        var result = v1 < v2;

        // Then
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("2.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0", false)]
    [TestCase("1.1.0", "1.0.0", true)]
    [TestCase("1.0.0", "1.1.0", false)]
    [TestCase("1.0.1", "1.0.0", true)]
    [TestCase("1.0.0", "1.0.1", false)]
    [TestCase("1.0.0", "1.0.0", false)]
    public void GivenTwoVersions_WhenComparingGreaterThan_ThenReturnsExpectedResult(string version1, string version2, bool expected)
    {
        // Given
        var v1 = new Version(version1);
        var v2 = new Version(version2);

        // When
        var result = v1 > v2;

        // Then
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("1.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0", false)]
    [TestCase("2.0.0", "1.0.0", false)]
    [TestCase("1.0.0-alpha", "1.0.0", false)]
    [TestCase("1.0.0-alpha", "1.0.0-alpha", true)]
    [TestCase("1.0.0-alpha.1", "1.0.0-alpha.2", false)]
    public void GivenTwoVersions_WhenComparingEquality_ThenReturnsExpectedResult(string version1, string version2, bool expected)
    {
        // Given
        var v1 = new Version(version1);
        var v2 = new Version(version2);

        // When
        var result = v1 == v2;

        // Then
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GivenNullVersions_WhenComparingEquality_ThenReturnsTrue()
    {
        // Given
        Version? v1 = null;
        Version? v2 = null;

        // When
        var result = v1 == v2;

        // Then
        Assert.That(result, Is.True);
    }

    [Test]
    public void GivenNullAndNonNullVersions_WhenComparingEquality_ThenReturnsFalse()
    {
        // Given
        Version? v1 = null;
        var v2 = new Version("1.0.0");

        // When
        var resultLeft = v1 == v2;
        var resultRight = v2 == v1;

        // Then
        Assert.That(resultLeft, Is.False);
        Assert.That(resultRight, Is.False);
    }

    [TestCase("1.0.0", "1.0.0-alpha", 1)]
    [TestCase("1.0.0-beta", "1.0.0-alpha", 1)]
    [TestCase("1.0.0-alpha", "1.0.0-beta", -1)]
    [TestCase("1.0.0-alpha.1", "1.0.0-alpha.2", -1)]
    [TestCase("1.0.0-alpha.2", "1.0.0-alpha.1", 1)]
    public void GivenTwoVersionsWithPrerelease_WhenComparing_ThenReturnsExpectedOrder(string version1, string version2, int expected)
    {
        // Given
        var v1 = new Version(version1);
        var v2 = new Version(version2);

        // When
        var result = v1.CompareTo(v2);

        // Then
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("1.0.0")]
    [TestCase("2.1.0")]
    [TestCase("3.2.1")]
    [TestCase("1.0.0-alpha")]
    [TestCase("1.0.0-beta.1")]
    public void GivenVersion_WhenConvertingToString_ThenReturnsOriginalFormat(string versionString)
    {
        // Given
        var version = new Version(versionString);

        // When
        var result = version.ToString();

        // Then
        Assert.That(result, Is.EqualTo(versionString));
    }

    [TestCase("1.0.0", "1.0.0-alpha", true)]
    [TestCase("1.0.0-beta", "1.0.0-alpha", true)]
    [TestCase("1.0.0-alpha", "1.0.0-beta", false)]
    [TestCase("2.0.0-alpha", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0-alpha", false)]
    public void GivenVersionsWithPrerelease_WhenComparingGreaterThan_ThenReturnsExpectedResult(string version1, string version2, bool expected)
    {
        // Given
        var v1 = new Version(version1);
        var v2 = new Version(version2);

        // When
        var result = v1 > v2;

        // Then
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GivenNullVersion_WhenComparingGreaterThan_ThenReturnsTrue()
    {
        // Given
        var version = new Version("1.0.0");
        Version? nullVersion = null;

        // When
        var result = version > nullVersion;

        // Then
        Assert.That(result, Is.True);
    }

    [Test]
    public void GivenNullVersion_WhenComparingLessThan_ThenReturnsFalse()
    {
        // Given
        var version = new Version("1.0.0");
        Version? nullVersion = null;

        // When
        var result = version < nullVersion;

        // Then
        Assert.That(result, Is.False);
    }

    [Test]
    public void GivenVersionCollection_WhenSorting_ThenOrdersCorrectly()
    {
        // Given
        var versions = new List<Version>
        {
            new("2.0.0"),
            new("1.0.0-alpha"),
            new("1.0.0"),
            new("1.0.0-beta"),
            new("3.0.0-alpha"),
            new("2.1.0")
        };

        var expectedOrder = new[]
        {
            "1.0.0-alpha",
            "1.0.0-beta",
            "1.0.0",
            "2.0.0",
            "2.1.0",
            "3.0.0-alpha"
        };

        // When
        var sortedVersions = versions.OrderBy(v => v).ToList();

        // Then
        Assert.That(sortedVersions.Select(v => v.ToString()), Is.EqualTo(expectedOrder));
    }

    [Test]
    public void GivenVersionCollectionWithNulls_WhenSorting_ThenOrdersCorrectly()
    {
        // Given
        var versions = new List<Version?>
        {
            new("2.0.0"),
            null,
            new("1.0.0"),
            null,
            new("3.0.0")
        };

        var expectedOrder = new Version?[]
        {
            null,
            null,
            new("1.0.0"),
            new("2.0.0"),
            new("3.0.0")
        };

        // When
        var sortedVersions = versions.OrderBy(v => v).ToList();

        // Then
        Assert.That(sortedVersions.Select(v => v?.ToString()),
            Is.EqualTo(expectedOrder.Select(v => v?.ToString())));
    }

    [Test]
    public void Given_FourPartVersions_When_SortingCollection_Then_SortsCorrectly()
    {
        // Arrange
        var versions = new[]
        {
            new Version("1.2.4.3"),
            new Version("2.3.1.5"),
            new Version("1.2.4.10"),
            new Version("2.0.0.0"),
            new Version("1.2.4.3-pre"),
            new Version("1.2.4.4"),
        };

        var expectedOrder = new[]
        {
            new Version("1.2.4.3-pre"),
            new Version("1.2.4.3"),
            new Version("1.2.4.4"),
            new Version("1.2.4.10"),
            new Version("2.0.0.0"),
            new Version("2.3.1.5"),
        };

        // Act
        var sortedVersions = versions.OrderBy(v => v).ToList();

        // Then
        Assert.That(sortedVersions.Select(v => v.ToString()),
            Is.EqualTo(expectedOrder.Select(v => v.ToString())));
    }

    [Test]
    public void Given_FourPartVersions_When_ComparingVersions_Then_ComparesCorrectly()
    {
        // Arrange
        var v1 = new Version("1.2.4.3");
        var v2 = new Version("1.2.4.10");
        var v3 = new Version("1.2.4.3-pre");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(v1 < v2, Is.True, "1.2.4.3 should be less than 1.2.4.10");
            Assert.That(v2 > v1, Is.True, "1.2.4.10 should be greater than 1.2.4.3");
            Assert.That(v3 < v1, Is.True, "1.2.4.3-pre should be less than 1.2.4.3");
            Assert.That(v1 > v3, Is.True, "1.2.4.3 should be greater than 1.2.4.3-pre");
        });
    }

    [Test]
    public void Given_FourPartVersion_When_CreatingFromString_Then_ParsesCorrectly()
    {
        // Arrange & Act
        var version = new Version("1.2.3.4");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(2));
            Assert.That(version.Patch, Is.EqualTo(3));
            Assert.That(version.Build, Is.EqualTo(4));
            Assert.That(version.Pre, Is.Null);
        });
    }

    [Test]
    public void Given_FourPartVersionWithPreRelease_When_CreatingFromString_Then_ParsesCorrectly()
    {
        // Arrange & Act
        var version = new Version("1.2.3.4-alpha.1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(2));
            Assert.That(version.Patch, Is.EqualTo(3));
            Assert.That(version.Build, Is.EqualTo(4));
            Assert.That(version.Pre, Is.EqualTo("alpha.1"));
        });
    }

    [Test]
    public void Given_MixedThreeAndFourPartVersions_When_SortingCollection_Then_SortsCorrectly()
    {
        // Arrange
        var versions = new[]
        {
            new Version("1.2.4.3"),
            new Version("2.3.1"),
            new Version("1.2.4"),
            new Version("2.0.0.1"),
            new Version("1.2.4-pre"),
            new Version("1.2.4.0"),
        };

        var expectedOrder = new[]
        {
            new Version("1.2.4-pre"),
            new Version("1.2.4"),      // Equal to 1.2.4.0
            new Version("1.2.4.0"),    // Equal to 1.2.4
            new Version("1.2.4.3"),
            new Version("2.0.0.1"),
            new Version("2.3.1"),      // Equal to 2.3.1.0
        };

        // Act
        var sortedVersions = versions.OrderBy(v => v).ToList();

        // Then
        Assert.That(sortedVersions.Select(v => v.ToString()),
            Is.EqualTo(expectedOrder.Select(v => v.ToString())));
    }

    [Test]
    public void Given_MixedThreeAndFourPartVersions_When_ComparingVersions_Then_ComparesCorrectly()
    {
        // Arrange
        var v1 = new Version("1.2.4");     // Equivalent to 1.2.4.0
        var v2 = new Version("1.2.4.0");   // Equivalent to 1.2.4
        var v3 = new Version("1.2.4.1");   // Greater than both
        var v4 = new Version("1.2.3.9");   // Less than all above
        var v5 = new Version("1.2.4-pre"); // Less than all non-pre-release

        // Assert
        Assert.Multiple(() =>
        {
            // Three-part vs four-part equality
            Assert.That(v1.Equals(v2), Is.True, "1.2.4 should equal 1.2.4.0");
            Assert.That(v2.Equals(v1), Is.True, "1.2.4.0 should equal 1.2.4");
            Assert.That(v1 == v2, Is.True, "1.2.4 should == 1.2.4.0");
            Assert.That(v2 == v1, Is.True, "1.2.4.0 should == 1.2.4");

            // Three-part vs four-part comparisons
            Assert.That(v1 < v3, Is.True, "1.2.4 should be less than 1.2.4.1");
            Assert.That(v3 > v2, Is.True, "1.2.4.1 should be greater than 1.2.4.0");
            Assert.That(v4 < v1, Is.True, "1.2.3.9 should be less than 1.2.4");
            Assert.That(v1 > v4, Is.True, "1.2.4 should be greater than 1.2.3.9");

            // Pre-release comparisons with mixed versions
            Assert.That(v5 < v1, Is.True, "1.2.4-pre should be less than 1.2.4");
            Assert.That(v5 < v2, Is.True, "1.2.4-pre should be less than 1.2.4.0");
            Assert.That(v1 > v5, Is.True, "1.2.4 should be greater than 1.2.4-pre");
            Assert.That(v2 > v5, Is.True, "1.2.4.0 should be greater than 1.2.4-pre");
        });
    }

    [Test]
    public void Given_MixedThreeAndFourPartVersions_When_ToString_Then_FormatsCorrectly()
    {
        // Arrange & Act
        var v1 = new Version("1.2.3");     // Three-part
        var v2 = new Version("1.2.3.0");   // Four-part with zero build
        var v3 = new Version("1.2.3.4");   // Four-part with non-zero build

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(v1.ToString(), Is.EqualTo("1.2.3"), "Three-part version should format without build number");
            Assert.That(v2.ToString(), Is.EqualTo("1.2.3"), "Four-part version with zero build should format as three-part");
            Assert.That(v3.ToString(), Is.EqualTo("1.2.3.4"), "Four-part version with non-zero build should show build number");
        });
    }

    [Test]
    public void GivenVersionString_WhenParsingWithRegex_ThenHandlesAllFormats()
    {
        // Valid formats - should parse successfully
        Assert.Multiple(() =>
        {
            // Three-part versions
            Assert.That(() => new Version("1.2.3"), Throws.Nothing);
            Assert.That(() => new Version("10.20.30"), Throws.Nothing);
            Assert.That(() => new Version("0.0.1"), Throws.Nothing);

            // Four-part versions
            Assert.That(() => new Version("1.2.3.4"), Throws.Nothing);
            Assert.That(() => new Version("10.20.30.40"), Throws.Nothing);
            Assert.That(() => new Version("0.0.1.0"), Throws.Nothing);

            // Pre-release versions
            Assert.That(() => new Version("1.2.3-alpha"), Throws.Nothing);
            Assert.That(() => new Version("1.2.3-alpha.1"), Throws.Nothing);
            Assert.That(() => new Version("1.2.3.4-beta"), Throws.Nothing);
            Assert.That(() => new Version("1.2.3.4-rc.1"), Throws.Nothing);

            // Multiple hyphen segments
            Assert.That(() => new Version("1.2.3-alpha-beta"), Throws.Nothing);
            Assert.That(() => new Version("1.2.3.4-pre-extra"), Throws.Nothing);
        });

        // Invalid formats - should throw ArgumentException
        Assert.Multiple(() =>
        {
            // Missing parts
            Assert.That(() => new Version("1.2"), Throws.ArgumentException);
            Assert.That(() => new Version("1"), Throws.ArgumentException);
            Assert.That(() => new Version(""), Throws.ArgumentException);

            // Invalid separators
            Assert.That(() => new Version("1,2,3"), Throws.ArgumentException);
            Assert.That(() => new Version("1_2_3"), Throws.ArgumentException);
            Assert.That(() => new Version("v1.2.3"), Throws.ArgumentException);

            // Invalid characters
            Assert.That(() => new Version("1.2.3.a"), Throws.ArgumentException);
            Assert.That(() => new Version("a.b.c"), Throws.ArgumentException);

            // Extra parts
            Assert.That(() => new Version("1.2.3.4.5"), Throws.ArgumentException);
        });
    }

    [Test]
    public void GivenVersionString_WhenParsing_ThenExtractsCorrectValues()
    {
        // Three-part version
        var v1 = new Version("1.2.3");
        Assert.Multiple(() =>
        {
            Assert.That(v1.Major, Is.EqualTo(1));
            Assert.That(v1.Minor, Is.EqualTo(2));
            Assert.That(v1.Patch, Is.EqualTo(3));
            Assert.That(v1.Build, Is.EqualTo(0));
            Assert.That(v1.Pre, Is.Null);
        });

        // Four-part version
        var v2 = new Version("1.2.3.4");
        Assert.Multiple(() =>
        {
            Assert.That(v2.Major, Is.EqualTo(1));
            Assert.That(v2.Minor, Is.EqualTo(2));
            Assert.That(v2.Patch, Is.EqualTo(3));
            Assert.That(v2.Build, Is.EqualTo(4));
            Assert.That(v2.Pre, Is.Null);
        });

        // Pre-release version with dot
        var v3 = new Version("1.2.3-alpha.1");
        Assert.Multiple(() =>
        {
            Assert.That(v3.Major, Is.EqualTo(1));
            Assert.That(v3.Minor, Is.EqualTo(2));
            Assert.That(v3.Patch, Is.EqualTo(3));
            Assert.That(v3.Build, Is.EqualTo(0));
            Assert.That(v3.Pre, Is.EqualTo("alpha.1"));
        });

        // Four-part with pre-release
        var v4 = new Version("1.2.3.4-beta.2");
        Assert.Multiple(() =>
        {
            Assert.That(v4.Major, Is.EqualTo(1));
            Assert.That(v4.Minor, Is.EqualTo(2));
            Assert.That(v4.Patch, Is.EqualTo(3));
            Assert.That(v4.Build, Is.EqualTo(4));
            Assert.That(v4.Pre, Is.EqualTo("beta.2"));
        });
    }

    [Test]
    public void GivenVersion_WhenToString_ThenPreservesOriginalFormat()
    {
        // Three-part versions
        Assert.Multiple(() =>
        {
            // Basic three-part
            var v1 = new Version("1.2.3");
            Assert.That(v1.ToString(), Is.EqualTo("1.2.3"));

            // Three-part with pre-release
            var v2 = new Version("1.2.3-alpha");
            Assert.That(v2.ToString(), Is.EqualTo("1.2.3-alpha"));

            var v3 = new Version("1.2.3-alpha.1");
            Assert.That(v3.ToString(), Is.EqualTo("1.2.3-alpha.1"));
        });

        // Four-part versions
        Assert.Multiple(() =>
        {
            // With non-zero build
            var v1 = new Version("1.2.3.4");
            Assert.That(v1.ToString(), Is.EqualTo("1.2.3.4"));

            // With zero build - should omit build number
            var v2 = new Version("1.2.3.0");
            Assert.That(v2.ToString(), Is.EqualTo("1.2.3"));

            // With pre-release and non-zero build
            var v3 = new Version("1.2.3.4-beta");
            Assert.That(v3.ToString(), Is.EqualTo("1.2.3.4-beta"));

            // With pre-release and zero build
            var v4 = new Version("1.2.3.0-beta");
            Assert.That(v4.ToString(), Is.EqualTo("1.2.3-beta"));
        });

        // Constructor with explicit parameters
        Assert.Multiple(() =>
        {
            // Three-part constructor (build defaults to 0)
            var v1 = new Version(1, 2, 3);
            Assert.That(v1.ToString(), Is.EqualTo("1.2.3"));

            // Four-part constructor with non-zero build
            var v2 = new Version(1, 2, 3, 4);
            Assert.That(v2.ToString(), Is.EqualTo("1.2.3.4"));

            // Four-part constructor with zero build
            var v3 = new Version(1, 2, 3, 0);
            Assert.That(v3.ToString(), Is.EqualTo("1.2.3"));
        });
    }

    [Test]
    public void GivenVersion_WhenToStringWithIncludeBuildNumber_ThenFormatsCorrectly()
    {
        // Three-part versions
        Assert.Multiple(() =>
        {
            var v1 = new Version("1.2.3");
            Assert.That(v1.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3"));
            Assert.That(v1.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.0"));

            var v2 = new Version("1.2.3-alpha");
            Assert.That(v2.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3-alpha"));
            Assert.That(v2.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.0-alpha"));
        });

        // Four-part versions
        Assert.Multiple(() =>
        {
            // With non-zero build - should always show build
            var v1 = new Version("1.2.3.4");
            Assert.That(v1.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3.4"));
            Assert.That(v1.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.4"));

            // With zero build - should show build only when requested
            var v2 = new Version("1.2.3.0");
            Assert.That(v2.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3"));
            Assert.That(v2.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.0"));

            // With pre-release and non-zero build
            var v3 = new Version("1.2.3.4-beta");
            Assert.That(v3.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3.4-beta"));
            Assert.That(v3.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.4-beta"));

            // With pre-release and zero build
            var v4 = new Version("1.2.3.0-beta");
            Assert.That(v4.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3-beta"));
            Assert.That(v4.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.0-beta"));
        });

        // Constructor with explicit parameters
        Assert.Multiple(() =>
        {
            // Three-part constructor (build defaults to 0)
            var v1 = new Version(1, 2, 3);
            Assert.That(v1.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3"));
            Assert.That(v1.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.0"));

            // Four-part constructor with non-zero build
            var v2 = new Version(1, 2, 3, 4);
            Assert.That(v2.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3.4"));
            Assert.That(v2.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.4"));

            // Four-part constructor with zero build
            var v3 = new Version(1, 2, 3, 0);
            Assert.That(v3.ToString(includeBuildNumber: false), Is.EqualTo("1.2.3"));
            Assert.That(v3.ToString(includeBuildNumber: true), Is.EqualTo("1.2.3.0"));
        });
    }
}
