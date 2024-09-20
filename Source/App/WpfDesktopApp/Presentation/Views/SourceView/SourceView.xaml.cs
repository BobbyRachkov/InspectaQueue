using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
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
                    Presenter = new NumericUpDown
                    {
                        ButtonsAlignment = ButtonsAlignment.Left,
                        TextAlignment = TextAlignment.Left
                    },
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
            var nameMargin = new Thickness(0, marginTop, 10, 0);
            var valueMargin = new Thickness(0, marginTop, 20, 0);

            var name = new Label
            {
                Content = settingEntryViewModel.Name,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = settingEntryViewModel.ToolTip
            };

            var nameGrid = new Grid
            {
                Height = rowHeight,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = nameMargin
            };
            nameGrid.Children.Add(name);
            Grid.SetColumn(nameGrid, 0);
            Grid.SetRow(nameGrid, 1);

            var presenterConfig = _typeHandlers[settingEntryViewModel.Type]();
            presenterConfig.Presenter.Margin = new Thickness(10, 0, 20, 0);
            presenterConfig.Presenter.VerticalAlignment = VerticalAlignment.Center;

            Binding valueBinding = new Binding
            {
                Source = settingEntryViewModel,
                Path = new PropertyPath(nameof(settingEntryViewModel.Value)),
                Mode = BindingMode.TwoWay
            };

            if (presenterConfig.ValueConverter is not null)
            {
                valueBinding.Converter = presenterConfig.ValueConverter;
            }

            presenterConfig.Presenter.SetBinding(presenterConfig.DependencyPropertyToBind, valueBinding);


            var valueGrid = new Grid
            {
                Height = rowHeight,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = valueMargin
            };
            valueGrid.Children.Add(presenterConfig.Presenter);
            Grid.SetColumn(valueGrid, 2);
            Grid.SetRow(valueGrid, 1);

            TableGrid.Children.Add(nameGrid);
            TableGrid.Children.Add(valueGrid);
        }

        private static Brush GetRandomColor()
        {
            Random r = new Random();
            return new SolidColorBrush(Color.FromRgb((byte) r.Next(1, 255),
                (byte) r.Next(1, 255), (byte) r.Next(1, 233)));
        }
    }
}
