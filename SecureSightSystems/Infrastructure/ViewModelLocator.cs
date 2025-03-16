using SecureSightSystems.Infrastructure;
using SecureSightSystems.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecureSightSystems.Infrastructure
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => DependencyContainer.Resolve<MainViewModel>();

        public OverviewViewModel OverviewViewModel => DependencyContainer.Resolve<OverviewViewModel>();

    }
}
