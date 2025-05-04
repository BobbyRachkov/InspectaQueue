using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views
{
    /// <summary>
    /// Interaction logic for QueueInspectorView.xaml
    /// </summary>
    public partial class QueueInspectorView : UserControl
    {
        private QueueInspectorViewModel? _dataContext;

        public QueueInspectorView()
        {
            InitializeComponent();
            DataContextChanged += SourceView_DataContextChanged;
        }

        private void SourceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is QueueInspectorViewModel queueInspectorViewModel)
            {
                _dataContext = queueInspectorViewModel;
            }
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.LeftShift && sender is TextBox tb)
            {
                tb.AcceptsReturn = false;
            }

            if (e.Key is Key.Enter && Keyboard.IsKeyDown(Key.LeftShift))
            {
                SendButton.Command.Execute(null);
            }
        }

        private void TextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.LeftShift && sender is TextBox tb)
            {
                tb.AcceptsReturn = true;
            }
        }

        private void OnSelectedEntriesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && _dataContext is not null)
            {
                _dataContext.SelectedEntries = dataGrid.SelectedItems.Cast<QueueEntryViewModel>().ToList();
            }
        }
    }
}
