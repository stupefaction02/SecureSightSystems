using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SecureSightSystems.Services
{
    public class Navigation
    {
        public event Action<Page> OnPageChanged;

        public void Navigate(Page page) => OnPageChanged?.Invoke(page);
    }
}
