using MahApps.Metro.Controls.Dialogs;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;

public class DialogManager
{
    private readonly Func<string, string, MessageDialogStyle, MetroDialogSettings, MessageDialogResult> _simpleDialogHandle;
    private readonly Func<string, string, bool, MetroDialogSettings, Task<ProgressDialogController>> _progressDialogHandle;
    private readonly Func<string, string, MetroDialogSettings, Task<string?>> _inputDialogHandle;
    private readonly Func<BaseMetroDialog, MetroDialogSettings?, Task> _showMetroDialogHandle;
    private readonly Func<BaseMetroDialog, MetroDialogSettings, Task> _hideDialogHandle;

    public DialogManager(
        Func<string, string, MessageDialogStyle, MetroDialogSettings, MessageDialogResult> simpleDialogHandle,
        Func<string, string, bool, MetroDialogSettings, Task<ProgressDialogController>> progressDialogHandle,
        Func<string, string, MetroDialogSettings, Task<string?>> inputDialogHandle,
        Func<BaseMetroDialog, MetroDialogSettings?, Task> showMetroDialogHandle,
        Func<BaseMetroDialog, MetroDialogSettings, Task> hideDialogHandle)
    {
        _simpleDialogHandle = simpleDialogHandle;
        _progressDialogHandle = progressDialogHandle;
        _inputDialogHandle = inputDialogHandle;
        _showMetroDialogHandle = showMetroDialogHandle;
        _hideDialogHandle = hideDialogHandle;
    }

    public UpdateDialogResult ShowNewUpdateDialog(string currentVersion, string newVersion)
    {
        var result = _simpleDialogHandle(
            "New Update available!",
            $"Current version: {currentVersion}\nAvailable version: {newVersion}",
            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "Download & Install now",
                FirstAuxiliaryButtonText = "Not now",
                NegativeButtonText = "Install in the background"
            });

        return result switch
        {
            MessageDialogResult.Affirmative => UpdateDialogResult.InstallNow,
            MessageDialogResult.Negative => UpdateDialogResult.InstallOnClose,
            MessageDialogResult.FirstAuxiliary => UpdateDialogResult.NotNow,
            _ => UpdateDialogResult.NotNow
        };
    }

    public void ShowVersionUpToDateDialog(string currentVersion)
    {
        var result = _simpleDialogHandle(
            "You are up to date!",
            $"Current version: {currentVersion}",
            MessageDialogStyle.Affirmative,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "Nice!",
            });
    }

    public void ShowVersionInfoDialog(string currentVersion)
    {
        var result = _simpleDialogHandle(
            "About",
            $"Current version: {currentVersion}",
            MessageDialogStyle.Affirmative,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "Close",
            });
    }

    public async Task ShowProgressDialog(string title, string content, bool isCancellable, bool isIndeterminate)
    {
        var settings = new MetroDialogSettings()
        {
            AnimateShow = false,
            AnimateHide = false
        };

        var controller = await _progressDialogHandle(title, content, isCancellable, settings);

        if (isIndeterminate)
        {
            controller.SetIndeterminate();
        }
    }

    public async Task<string?> ShowInputDialog(string title, string content)
    {
        var settings = new MetroDialogSettings()
        {
            AnimateShow = false,
            AnimateHide = false
        };

        return await _inputDialogHandle(title, content, settings);
    }

    public async Task ShowDismissibleMessage(string title, string content, bool animate = true, int millisecondsDelayToClose = 1500)
    {
        var dialog = new CustomDialog(new MetroDialogSettings
        {
            AffirmativeButtonText = "OK",
            AnimateHide = animate,
            AnimateShow = animate,
            DialogMessageFontSize = 50,
            NegativeButtonText = "OK",
            OwnerCanCloseWithDialog = true
        })
        {
            Content = content,
            Title = title,
        };

        await _showMetroDialogHandle(dialog, null);

        await Task.Delay(millisecondsDelayToClose);
        await _hideDialogHandle(dialog, new MetroDialogSettings());
    }
}