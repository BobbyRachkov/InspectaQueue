using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

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
