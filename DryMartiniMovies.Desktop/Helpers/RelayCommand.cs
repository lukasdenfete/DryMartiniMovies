

using System.IO;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    private Action _execute;
    private Action<object> _handler;
    private Func<bool> _canExecute;
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    public RelayCommand(Action<object> handler, Func<bool>? canExecute = null)
    {
        _handler = handler;
        _canExecute = canExecute;
    }
    public bool CanExecute(object? parameter)
    {
        if(_canExecute == null) return true; //knappen klickbar om ingen CanExecute alls skickas
        return _canExecute();
    }

    public void Execute(object? parameter)
    {
        if(_handler != null)
        {
            _handler(parameter);
        } else
        {
            _execute();
        }
    }
}