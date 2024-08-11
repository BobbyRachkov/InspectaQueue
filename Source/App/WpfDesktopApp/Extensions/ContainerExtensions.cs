using Autofac;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Extensions;

public static class ContainerExtensions
{
    public static IContainer InitializeWindowManager(this IContainer container)
    {
        container.Resolve<IWindowManager>();
        return container;
    }
}