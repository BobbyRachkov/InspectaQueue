namespace Rachkov.InspectaQueue.Abstractions.Attributes;

[AttributeUsage(System.AttributeTargets.Property)]
public class SecretAttribute : Attribute
{
    public bool CanBeRevealed = false;
    public char PasswordChar = '\u25cf';
}