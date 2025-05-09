﻿using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models.Modifiers;
using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public interface ISettingViewModel
{
    PropertyInfo ReflectedProperty { get; }
    string PropertyName { get; }
    string Name { get; }
    string? ToolTip { get; }
    Modifiers Modifiers { get; }
    Type Type { get; }
    object? Value { get; set; }
    ISettingViewModel Clone();
}