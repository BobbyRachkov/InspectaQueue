using System.Windows;

namespace Rachkov.InspectaQueue.WpfDesktopApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppBootstrapper.OnStartup(e);
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppBootstrapper.OnShutdown(e);
            base.OnExit(e);
        }
    }

}
