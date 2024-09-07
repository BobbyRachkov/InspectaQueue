using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView.ValueConverters;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView
{
    /// <summary>
    /// Interaction logic for SourceView.xaml
    /// </summary>
    public partial class SourceView : UserControl
    {
        private Dictionary<Type, Func<PresenterConfig>> _typeHandlers;

        public SourceView()
        {
            InitializeComponent();
            InitTypeHandlers();
            DataContextChanged += SourceView_DataContextChanged;
        }

        private void InitTypeHandlers()
        {
            _typeHandlers = new Dictionary<Type, Func<PresenterConfig>>
            {
                {typeof(string),()=>new PresenterConfig
                {
                    Presenter = new TextBox(),
                    DependencyPropertyToBind = TextBox.TextProperty
                }},
                {typeof(bool),()=>new PresenterConfig
                {
                    Presenter = new CheckBox(),
                    DependencyPropertyToBind = ToggleButton.IsCheckedProperty
                }},
                {typeof(int),()=>new PresenterConfig
                {
                    Presenter = new NumericUpDown(),
                    DependencyPropertyToBind = NumericUpDown.ValueProperty,
                    ValueConverter = new DoubleToIntValueConverter()
                }},
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
            TableGrid.Children.Clear();

            for (var i = 0; i < sourceViewModel.Settings.Length; i++)
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
            var name = new Label
            {
                Content = settingEntryViewModel.Name,
                Height = rowHeight,
                Margin = nameMargin,
                VerticalAlignment = VerticalAlignment.Top,
                ToolTip = settingEntryViewModel.ToolTip
            };
            Grid.SetColumn(name, 0);
            Grid.SetRow(name, 1);

            var presenterConfig = _typeHandlers[settingEntryViewModel.Type]();
            Grid.SetColumn(presenterConfig.Presenter, 2);
            Grid.SetRow(presenterConfig.Presenter, 1);
            presenterConfig.Presenter.Margin = valueMargin;
            presenterConfig.Presenter.VerticalAlignment = VerticalAlignment.Top;

            Binding valueBinding = new Binding
            {
                Source = settingEntryViewModel,
                Path = new PropertyPath(nameof(settingEntryViewModel.Value)),
                Mode = BindingMode.TwoWay
            };

            if (presenterConfig.ValueConverter is not null)
            {
                valueBinding.Converter= presenterConfig.ValueConverter;
            }

            presenterConfig.Presenter.SetBinding(presenterConfig.DependencyPropertyToBind, valueBinding);

            TableGrid.Children.Add(name);
            TableGrid.Children.Add(presenterConfig.Presenter);
        }
    }
}
