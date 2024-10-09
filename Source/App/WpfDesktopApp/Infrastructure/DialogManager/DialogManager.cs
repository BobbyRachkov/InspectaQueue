using MahApps.Metro.Controls.Dialogs;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;

public class DialogManager
{
    private readonly Func<string, string, MessageDialogStyle, MetroDialogSettings, MessageDialogResult> _simpleDialogHandle;
    private readonly Func<string, string, bool, MetroDialogSettings, Task<ProgressDialogController>> _progressDialogHandle;

    public DialogManager(
        Func<string, string, MessageDialogStyle, MetroDialogSettings, MessageDialogResult> simpleDialogHandle,
        Func<string, string, bool, MetroDialogSettings, Task<ProgressDialogController>> progressDialogHandle)
    {
        _simpleDialogHandle = simpleDialogHandle;
        _progressDialogHandle = progressDialogHandle;
    }

    public bool ShowNewUpdateDialog(string currentVersion, string newVersion)
    {
        var result = _simpleDialogHandle(
            "New Update available!",
            $"Current version: {currentVersion}\nAvailable version: {newVersion}",
            MessageDialogStyle.AffirmativeAndNegative,
            new MetroDialogSettings
            {
                AffirmativeButtonText = "Download & Install",
                NegativeButtonText = "Not now"
            });

        return result is MessageDialogResult.Affirmative;
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
}