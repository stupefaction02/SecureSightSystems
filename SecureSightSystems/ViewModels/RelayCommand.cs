using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SecureSightSystems.ViewModels
{
	public class RelayCommand : ICommand
	{
		private Action<object> _execute;

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public RelayCommand(Action<object> execute)
		{
			_execute = execute;
		}
	}

	public class RelayVoidCommand : ICommand
	{
		private Action _execute;

		public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
			_execute();
        }

        public RelayVoidCommand(Action execute)
		{
			_execute = execute;
		}
	}
}
