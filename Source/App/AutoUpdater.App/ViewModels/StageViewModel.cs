using Rachkov.InspectaQueue.AutoUpdater.Core;
using ReactiveUI;
using System.Collections.Generic;

namespace AutoUpdater.App.ViewModels;

public class StageViewModel : ViewModelBase
{
    private Stage _stage;
    private StageStatus _status;

    private readonly Dictionary<Stage, string> _stageNames = new Dictionary<Stage, string>
    {
        { Stage.DownloadingRelease ,"Downloading release"},
        { Stage.VerifyingInstaller ,"Verifying installer"},
        { Stage.Unzipping ,"Extracting"},
        { Stage.CopyingFiles ,"Copying files"},
        { Stage.CleaningUp ,"Cleaning up"},
        { Stage.WaitingAppToClose ,"Waiting InspectaQueue to be closed"},
        { Stage.LaunchApp ,"Launching InspectaQueue"},
        { Stage.Uninstalling ,"Uninstalling"},
    };

    public Stage Stage
    {
        get => _stage;
        set
        {
            _stage = value;
            this.RaisePropertyChanged();
        }
    }

    public string Name => _stageNames.GetValueOrDefault(Stage, "unnamed stage");

    public StageStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            this.RaisePropertyChanged();
        }
    }
}