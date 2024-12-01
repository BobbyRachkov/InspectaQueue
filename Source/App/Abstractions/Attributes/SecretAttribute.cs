namespace Rachkov.InspectaQueue.Abstractions.Attributes;

[AttributeUsage(System.AttributeTargets.Property)]
public class SecretAttribute(bool canBeRevealed = false) : Attribute
{
    public bool CanBeRevealed { get; } = canBeRevealed;
}