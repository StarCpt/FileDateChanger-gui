using System.Windows.Input;

namespace LurkingMvvm
{
    public class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        readonly Action<object?> executeCallback;

        public Command(Action<object?> executeCallback)
        {
            this.executeCallback = executeCallback;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool CanExecute(object? parameter) => true;
        public virtual void Execute(object? parameter) => executeCallback.Invoke(parameter);
    }
}
