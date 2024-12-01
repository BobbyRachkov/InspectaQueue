using MahApps.Metro.Controls;
using Microsoft.Win32;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView.ValueConverters;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models.Modifiers;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using PasswordBoxHelper = Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView.AttachedProperties.PasswordBoxHelper;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView
{
    /// <summary>
    /// Interaction logic for SourceView.xaml
    /// </summary>
    public partial class SourceView : UserControl
    {
        private Dictionary<Type, Func<Modifiers, PresenterConfig>> _typeHandlers;
        private Func<PresenterConfig> _dropdownHandler;
        private Style? _revealedPasswordBoxStyle;

        public SourceView()
        {
            InitializeComponent();
            InitTypeHandlers();
            DataContextChanged += SourceView_DataContextChanged;
        }

        private void InitTypeHandlers()
        {
            _typeHandlers = new Dictionary<Type, Func<Modifiers, PresenterConfig>>
            {
                {typeof(string),(m)=>
                    {
                        FrameworkElement frameworkElement;
                        DependencyProperty dependencyPropertyToBind = TextBox.TextProperty;
                        if (m.Secret is not null)
                        {
                            frameworkElement = new PasswordBox
                            {
                                PasswordChar = m.Secret.PasswordChar,
                            };

                            if (m.Secret.CanBeRevealed)
                            {
                                frameworkElement.Style = FindStyle("MahApps.Styles.PasswordBox.Revealed");
                            }

                            dependencyPropertyToBind = PasswordBoxHelper.BoundPasswordProperty;
                            frameworkElement.SetValue(PasswordBoxHelper.BindPasswordProperty,true);
                        }
                        else if (m.FilePath is not null)
                        {
                            frameworkElement = new TextBox();
                            frameworkElement.Style = FindStyle("MahApps.Styles.TextBox.Search");
                            frameworkElement.SetValue(TextBoxHelper.ButtonContentProperty,"M16.5,12C19,12 21,14 21,16.5C21,17.38 20.75,18.21 20.31,18.9L23.39,22L22,23.39L18.88,20.32C18.19,20.75 17.37,21 16.5,21C14,21 12,19 12,16.5C12,14 14,12 16.5,12M16.5,14A2.5,2.5 0 0,0 14,16.5A2.5,2.5 0 0,0 16.5,19A2.5,2.5 0 0,0 19,16.5A2.5,2.5 0 0,0 16.5,14M19,8H3V18H10.17C10.34,18.72 10.63,19.39 11,20H3C1.89,20 1,19.1 1,18V6C1,4.89 1.89,4 3,4H9L11,6H19A2,2 0 0,1 21,8V11.81C20.42,11.26 19.75,10.81 19,10.5V8Z");
                            frameworkElement.SetValue(TextBoxHelper.ButtonCommandProperty,new RelayCommand(()=>
                            {
                                var textBox = (TextBox)frameworkElement;
                                var defaultDirectory = !string.IsNullOrWhiteSpace(textBox.Text) ? Path.GetDirectoryName(textBox.Text) : null;
                                var ofd = new OpenFileDialog()
                                {
                                    Filter = m.FilePath.Filter,
                                    CheckFileExists = true,
                                    InitialDirectory = defaultDirectory ?? "C:\\",
                                    Multiselect = false,
                                };

                                var result=ofd.ShowDialog();

                                if (result is true)
                                {
                                    frameworkElement.SetValue(TextBox.TextProperty,ofd.FileName);
                                    frameworkElement.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                                }
                            }));
                        }
                        else
                        {
                            frameworkElement = new TextBox();
                        }

                        return new PresenterConfig
                        {
                            Presenter = frameworkElement,
                            DependencyPropertyToBind = dependencyPropertyToBind
                        };
                    }
                },
                {typeof(bool),(_)=>new PresenterConfig
                {
                    Presenter = new CheckBox(),
                    DependencyPropertyToBind = ToggleButton.IsCheckedProperty
                }},
                {typeof(int),(_)=>new PresenterConfig
                {
                    Presenter = new NumericUpDown
                    {
                        ButtonsAlignment = ButtonsAlignment.Left,
                        TextAlignment = TextAlignment.Left
                    },
                    DependencyPropertyToBind = NumericUpDown.ValueProperty,
                    ValueConverter = new DoubleToIntValueConverter()
                }}
            };

            _dropdownHandler = () => new PresenterConfig
            {
                Presenter = new ComboBox()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center
                },
                DependencyPropertyToBind = Selector.SelectedItemProperty
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

        private void RenderSettingRow(ISettingViewModel settingEntryViewModel, int index)
        {
            double rowHeight = 30;
            double marginTop = index * (rowHeight + 10);
            var nameMargin = new Thickness(0, marginTop, 5, 0);
            var valueMargin = new Thickness(5, marginTop, 20, 0);

            var name = new Label
            {
                Content = settingEntryViewModel.Name,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = settingEntryViewModel.ToolTip
            };

            var presenter = settingEntryViewModel switch
            {
                MultipleChoiceSettingViewModel { IsMultiSelectEnabled: false } multipleChoiceSetting => HandleDropdown(multipleChoiceSetting),
                _ => HandlePrimitive(settingEntryViewModel)
            };


            presenter.VerticalAlignment = VerticalAlignment.Center;
            var nameGrid = CreateGridWrapper(name, rowHeight, 1, 0, nameMargin);
            var valueGrid = CreateGridWrapper(presenter, rowHeight, 1, 2, valueMargin);

            TableGrid.Children.Add(nameGrid);
            TableGrid.Children.Add(valueGrid);
        }

        private FrameworkElement HandleDropdown(MultipleChoiceSettingViewModel viewModel)
        {
            var presenterConfig = _dropdownHandler();
            var presenter = (ComboBox)presenterConfig.Presenter;

            Binding binding = new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(viewModel.SelectedItem)),
                Mode = BindingMode.TwoWay
            };

            presenter.ItemsSource = viewModel.Options;
            presenter.DisplayMemberPath = nameof(DropdownOptionViewModel.DisplayName);

            presenterConfig.Presenter.SetBinding(presenterConfig.DependencyPropertyToBind, binding);
            return presenterConfig.Presenter;
        }

        private FrameworkElement HandlePrimitive(ISettingViewModel viewModel)
        {
            var presenterConfig = _typeHandlers.TryGetValue(viewModel.Type, out var handler)
                ? handler(viewModel.Modifiers)
                : _typeHandlers[typeof(string)](viewModel.Modifiers);

            Binding valueBinding = new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(viewModel.Value)),
                Mode = BindingMode.TwoWay
            };

            if (presenterConfig.ValueConverter is not null)
            {
                valueBinding.Converter = presenterConfig.ValueConverter;
            }

            presenterConfig.Presenter.SetBinding(presenterConfig.DependencyPropertyToBind, valueBinding);
            return presenterConfig.Presenter;
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

        private Style? FindStyle(string key)
        {
            return Application.Current.TryFindResource(key) as Style;
        }
    }
}
