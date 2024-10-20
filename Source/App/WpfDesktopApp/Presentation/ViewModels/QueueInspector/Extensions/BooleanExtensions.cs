using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Extensions;

public static class BooleanExtensions
{
    public static JsonFormatting ToJsonFormatting(this bool? value)
    {
        return value switch
        {
            true => JsonFormatting.Indented,
            false => JsonFormatting.Compact,
            _ => JsonFormatting.Unchanged,
        };
    }
}