﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SecureSightSystems.ViewModels
{
	/// <summary>
	/// Description of BaseViewModel.
	/// </summary>
	public class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}
	}
}
