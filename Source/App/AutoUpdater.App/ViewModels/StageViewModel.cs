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
        { Stage.WaitingAppToClose ,"Waiting IQ to exit"},
        { Stage.LaunchApp ,"Launching IQ"},
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
            RaiseStatusUpdated();
        }
    }

    private void RaiseStatusUpdated()
    {
        this.RaisePropertyChanged(nameof(IsPending));
        this.RaisePropertyChanged(nameof(IsInProgress));
        this.RaisePropertyChanged(nameof(IsDone));
        this.RaisePropertyChanged(nameof(IsFailed));
        this.RaisePropertyChanged(nameof(IsSkipped));
    }

    public bool IsPending => Status == StageStatus.Pending;
    public bool IsInProgress => Status == StageStatus.InProgress;
    public bool IsDone => Status == StageStatus.Done;
    public bool IsFailed => Status == StageStatus.Failed;
    public bool IsSkipped => Status == StageStatus.Skipped;
}