using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ceenth.Viewmodel
{
    // Uma versao modificada de um command standard
    // Esta versao, faz com que o código seja mais straight forward em questoes de execuçao
    // Isto faz com que o _canExecute seja opcional de usar
    // Exemplo:
    // new RelayCommand(ActionExample, p => true);
    // Passa a ser:
    // new RelayCommand(ActionExample);

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) 
        { 
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
