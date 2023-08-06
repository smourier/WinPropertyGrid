using System;
using System.Windows.Input;

namespace WinPropertyGrid.Utilities
{
    public class BaseCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public BaseCommand(Action<object?> executor)
        {
            ArgumentNullException.ThrowIfNull(executor);
            Executor = executor;
        }

        public Action<object?> Executor { get; }

        public virtual void Execute(object? parameter) => Executor.Invoke(parameter);

        public virtual bool CanExecute(object? parameter) => true;
        protected virtual void OnCanExecuteChanged(object? sender, EventArgs e) => CanExecuteChanged?.Invoke(sender, e);
    }
}
