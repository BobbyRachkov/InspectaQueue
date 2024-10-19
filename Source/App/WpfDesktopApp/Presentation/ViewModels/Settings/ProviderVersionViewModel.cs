﻿using Rachkov.InspectaQueue.Abstractions;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class ProviderVersionViewModel(KeyValuePair<string, IQueueProvider> version)
{
    public IQueueProvider Instance { get; } = version.Value;

    public string Name => version.Key;
}