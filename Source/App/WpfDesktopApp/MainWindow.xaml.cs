using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;
using DialogManager = Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager.DialogManager;

namespace Rachkov.InspectaQueue.WpfDesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (newContent is PresenterViewModel presenterViewModel)
            {
                presenterViewModel.PropertyChanged += PresenterViewModel_PropertyChanged;
            }

            if (newContent is ICanManageDialogs dialogManagerViewModel)
            {
                dialogManagerViewModel.DialogManager = new DialogManager(this.ShowModalMessageExternal, this.ShowProgressAsync);
            }
        }

        private void PresenterViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ICanBeTopmost.Topmost)
                && sender is ICanBeTopmost topmostViewModel)
            {
                Topmost = topmostViewModel.Topmost;
            }
        }
    }
}