namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public class ErrorViewModel : ViewModel
{
    public required string Text { get; set; }
    public string? Source { get; set; }
    public string? ExceptionHeader { get; set; }
    public string? ExceptionText { get; set; }
}