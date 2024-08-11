using System.Windows.Input;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?>? _execute;
        private readonly Action? _executeWithoutParameter;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<bool>? _canExecuteWithoutParameter;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action execute)
        {
            _executeWithoutParameter = execute;
        }

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public RelayCommand(Action<object?> execute, Func<object?, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<object?, bool> canExecute)
        {
            _executeWithoutParameter = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object?> execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecuteWithoutParameter = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _executeWithoutParameter = execute;
            _canExecuteWithoutParameter = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute is null && _canExecuteWithoutParameter is null
                   || _canExecuteWithoutParameter is not null && _canExecuteWithoutParameter()
                   || _canExecute is not null && _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute?.Invoke(parameter);
            _executeWithoutParameter?.Invoke();
        }
    }
}
