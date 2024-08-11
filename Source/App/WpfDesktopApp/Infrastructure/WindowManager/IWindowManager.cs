namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;

public interface IWindowManager
{
    void Create(IPresenterViewModel viewModel);

    void Show(IPresenterViewModel viewModel);

    void Hide(IPresenterViewModel viewModel);

    void Close(IPresenterViewModel viewModel);
}