namespace Rachkov.InspectaQueue.Abstractions;

public sealed class Version
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? Pre { get; }

    public Version(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Pre = null;
    }

    public Version(string version)
    {
        var components = version.Split(['.', '-'], StringSplitOptions.RemoveEmptyEntries);

        Major = int.Parse(components[0]);
        Minor = int.Parse(components[1]);
        Patch = int.Parse(components[2]);

        if (components.Length == 4
            && !int.TryParse(components[3],out _))
        {
            Pre = components[3];
        }

        if (components.Length > 4)
        {
            Pre = $"{components[3]}.{components[4]}";
        }
    }

    public override string ToString()
    {
        if (Pre is null)
        {
            return $"{Major}.{Minor}.{Patch}";
        }

        return $"{Major}.{Minor}.{Patch}-{Pre}";
    }

    private bool Equals(Version other)
    {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Version other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch);
    }

    public static bool operator >(Version a, Version b)
    {
        if (a.Major > b.Major)
        {
            return true;
        }

        if (a.Major < b.Major)
        {
            return false;
        }

        if (a.Minor > b.Minor)
        {
            return true;
        }

        if (a.Minor < b.Minor)
        {
            return false;
        }

        return a.Patch > b.Patch;
    }

    public static bool operator <(Version a, Version b)
    {
        return a != b && !(a > b);
    }

    public static bool operator ==(Version a, Version b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Version a, Version b)
    {
        return !a.Equals(b);
    }
}