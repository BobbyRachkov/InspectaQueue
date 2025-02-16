namespace Rachkov.InspectaQueue.AutoUpdater.Core;

using System.Text.RegularExpressions;

public sealed class Version : IComparable<Version>
{
    private static readonly Regex VersionPattern = new(
        @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(\.(?<build>\d+))?(-(?<pre>[^-]+))?$",
        RegexOptions.Compiled);

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public int Build { get; }
    public string? Pre { get; }

    public Version(int major, int minor, int patch, int build = 0)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Build = build;
        Pre = null;
    }

    public Version(string version)
    {
        var match = VersionPattern.Match(version);
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid version format: {version}. Expected format: major.minor.patch[.build][-pre]", nameof(version));
        }

        Major = int.Parse(match.Groups["major"].Value);
        Minor = int.Parse(match.Groups["minor"].Value);
        Patch = int.Parse(match.Groups["patch"].Value);
        Build = match.Groups["build"].Success ? int.Parse(match.Groups["build"].Value) : 0;
        Pre = match.Groups["pre"].Success ? match.Groups["pre"].Value : null;
    }

    public static bool operator >(Version? left, Version? right)
    {
        if (left is null) return false;
        if (right is null) return true;
        return left.CompareTo(right) > 0;
    }

    public static bool operator <(Version? left, Version? right)
    {
        if (left is null) return right is not null;
        if (right is null) return false;
        return left.CompareTo(right) < 0;
    }

    public static bool operator >=(Version? left, Version? right)
    {
        if (left is null) return right is null;
        if (right is null) return true;
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <=(Version? left, Version? right)
    {
        if (left is null) return true;
        if (right is null) return false;
        return left.CompareTo(right) <= 0;
    }

    public static bool operator ==(Version? left, Version? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Version? left, Version? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return ToString(includeBuildNumber: false);
    }

    public string ToString(bool includeBuildNumber)
    {
        var baseVersion = includeBuildNumber || Build > 0
            ? $"{Major}.{Minor}.{Patch}.{Build}"
            : $"{Major}.{Minor}.{Patch}";

        if (Pre is null)
        {
            return baseVersion;
        }

        return $"{baseVersion}-{Pre}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is Version other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, Build, Pre);
    }

    public int CompareTo(Version? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;

        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;

        var patchComparison = Patch.CompareTo(other.Patch);
        if (patchComparison != 0) return patchComparison;

        var buildComparison = Build.CompareTo(other.Build);
        if (buildComparison != 0) return buildComparison;

        // Pre-release versions are lower than the release version
        if (Pre is null && other.Pre is not null) return 1;
        if (Pre is not null && other.Pre is null) return -1;
        if (Pre is null && other.Pre is null) return 0;

        return string.Compare(Pre, other.Pre, StringComparison.Ordinal);
    }

    private bool Equals(Version? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch &&
               Build == other.Build &&
               Pre == other.Pre;
    }
}