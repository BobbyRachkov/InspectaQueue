using System.Windows;
using System.Windows.Data;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView
{
    internal class PresenterConfig
    {
        public required FrameworkElement Presenter { get; init; }
        public required DependencyProperty DependencyPropertyToBind { get; init; }
        public IValueConverter? ValueConverter { get; init; }
    }
}
