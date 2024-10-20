﻿using MahApps.Metro.Controls;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView.ValueConverters;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

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
                sourceViewModel.PropertyChanged += SourceViewModel_PropertyChanged;
            }
        }

        private void SourceViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SourceViewModel.Settings)
                && sender is SourceViewModel sourceViewModel)
            {
                RenderSettingsControls(sourceViewModel);
            }
        }

        public void RenderSettingsControls(SourceViewModel sourceViewModel)
        {
            ResetGridChildren();

            for (var i = 0; i < sourceViewModel.Settings.Length; i++)
            {
                RenderSettingRow(sourceViewModel.Settings[i], i);
            }
        }

        private void ResetGridChildren()
        {
            TableGrid.Children.Clear();

            var splitter = new GridSplitter();
            Grid.SetRow(splitter, 0);
            Grid.SetColumn(splitter, 1);
            TableGrid.Children.Add(splitter);
        }

        private void RenderSettingRow(SettingEntryViewModel settingEntryViewModel, int index)
        {
            double rowHeight = 30;
            double marginTop = index * (rowHeight + 10);
            var nameMargin = new Thickness(0, marginTop, 5, 0);
            var valueMargin = new Thickness(5, marginTop, 20, 0);

            Binding valueBinding = new Binding
            {
                Source = settingEntryViewModel,
                Path = new PropertyPath(nameof(settingEntryViewModel.Value)),
                Mode = BindingMode.TwoWay
            };

            var name = new Label
            {
                Content = settingEntryViewModel.Name,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = settingEntryViewModel.ToolTip
            };

            var presenterConfig = _typeHandlers[settingEntryViewModel.Type]();
            presenterConfig.Presenter.VerticalAlignment = VerticalAlignment.Center;

            if (presenterConfig.ValueConverter is not null)
            {
                valueBinding.Converter = presenterConfig.ValueConverter;
            }

            presenterConfig.Presenter.SetBinding(presenterConfig.DependencyPropertyToBind, valueBinding);


            var nameGrid = CreateGridWrapper(name, rowHeight, 1, 0, nameMargin);
            var valueGrid = CreateGridWrapper(presenterConfig.Presenter, rowHeight, 1, 2, valueMargin);


            TableGrid.Children.Add(nameGrid);
            TableGrid.Children.Add(valueGrid);
        }

        private Grid CreateGridWrapper(
            FrameworkElement content,
            double rowHeight,
            int parentGridRow,
            int parentGridColumn,
            Thickness margin)
        {
            var grid = new Grid
            {
                Height = rowHeight,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = margin
            };
            grid.Children.Add(content);
            Grid.SetColumn(grid, parentGridColumn);
            Grid.SetRow(grid, parentGridRow);

            return grid;
        }

        private static Brush GetRandomColor()
        {
            Random r = new Random();
            return new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255),
                (byte)r.Next(1, 255), (byte)r.Next(1, 233)));
        }

        private void HideVersionChangeGrid(object sender, RoutedEventArgs e)
        {
            ChangeVersionToggle.IsChecked = false;
        }
    }
}
