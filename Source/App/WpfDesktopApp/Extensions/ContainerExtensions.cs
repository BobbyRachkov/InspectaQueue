using Autofac;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Extensions;

public static class ContainerExtensions
{
    public static IContainer InitializeWindowManager(this IContainer container)
    {
        container.Resolve<IWindowManager>();
        return container;
    }
}