using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views
{
    /// <summary>
    /// Interaction logic for SourceView.xaml
    /// </summary>
    public partial class SourceView : UserControl
    {
        private Dictionary<Type, Func<(FrameworkElement, DependencyProperty)>> _typeHandlers;
        public SourceView()
        {
            InitializeComponent();
            InitTypeHandlers();
            DataContextChanged += SourceView_DataContextChanged;
        }

        private void InitTypeHandlers()
        {
            _typeHandlers = new Dictionary<Type, Func<(FrameworkElement, DependencyProperty)>>
            {
                {typeof(string),()=>(new TextBox(),TextBox.TextProperty)},
                {typeof(bool),()=>(new CheckBox(), ToggleButton.IsCheckedProperty)},
                {typeof(int),()=>(new NumericUpDown(), NumericUpDown.ValueProperty)},

            };
        }

        private void SourceView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is SourceViewModel sourceViewModel)
            {
                RenderSettingsControls(sourceViewModel);
            }
        }

        public void RenderSettingsControls(SourceViewModel sourceViewModel)
        {
            for (var i = 0; i < sourceViewModel.Settings.Count; i++)
            {
                RenderSettingRow(sourceViewModel.Settings[i], i);
            }
        }

        private void RenderSettingRow(SettingEntryViewModel settingEntryViewModel, int index)
        {
            double rowHeight = 30;
            double marginTop = index * (rowHeight + 10);
            var nameMargin = new Thickness(0, marginTop + 2, 10, 0);
            var valueMargin = new Thickness(10, marginTop, 20, 0);
            TextBlock name = new TextBlock
            {
                Text = settingEntryViewModel.Name,
                Height = rowHeight,
                Margin = nameMargin,
                VerticalAlignment = VerticalAlignment.Top,
                ToolTip = settingEntryViewModel.ToolTip
            };
            Grid.SetColumn(name, 0);
            Grid.SetRow(name, 1);

            var (valuePresenter, dependencyProperty) = _typeHandlers[settingEntryViewModel.Type]();
            Grid.SetColumn(valuePresenter, 2);
            Grid.SetRow(valuePresenter, 1);
            valuePresenter.Margin = valueMargin;
            valuePresenter.VerticalAlignment = VerticalAlignment.Top;

            Binding valueBinding = new Binding
            {
                Source = settingEntryViewModel,
                Path = new PropertyPath(nameof(settingEntryViewModel.Value)),
                Mode = BindingMode.TwoWay
            };
            valuePresenter.SetBinding(dependencyProperty, valueBinding);

            TableGrid.Children.Add(name);
            TableGrid.Children.Add(valuePresenter);
        }
    }
}
