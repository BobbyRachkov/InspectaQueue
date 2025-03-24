using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;
using System.Windows;
using System.Windows.Controls;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views
{
    /// <summary>
    /// Interaction logic for QueueInspectorView.xaml
    /// </summary>
    public partial class QueueInspectorView : UserControl
    {
        public QueueInspectorView()
        {
            InitializeComponent();
            DataContextChanged += SourceView_DataContextChanged;
        }

        private void SourceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is QueueInspectorViewModel queueInspectorViewModel)
            {

            }
        }
    }
}
