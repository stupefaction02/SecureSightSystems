using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SecureSightSystems.Services
{
    public class ApplicationInfoManager
    {
        public event Action<InfoMessage> NewInfoMessage;

        public event Action SignaledToHide;

        /// <summary>
        /// Signal to application to show message, that will be represented in GUI
        /// </summary>
        /// <param name="message"></param>
        public void ShowInfoMessage(InfoMessage message)
        {
           Application.Current.Dispatcher.Invoke(() => NewInfoMessage?.Invoke(message));
        }

        public void HideInfoMessage()
        {
            SignaledToHide?.Invoke();
        }
    }

    public abstract class InfoMessage
    {
        public bool Active { get; set; } = false;

        public abstract string ColorCodeHex { get; }

        public string Text { get; set; }
    }

    public class InfoWarningMessage : InfoMessage
    {
        public override string ColorCodeHex => "#FFFF00";
    }

    public class InfoErrorMessage : InfoMessage
    {
        public override string ColorCodeHex => "#FF0000";
    }

    public class SuccessInfoMessage : InfoMessage
    {
        public override string ColorCodeHex => "#008000";
    }

    public class NeutralInfoMessage : InfoMessage
    {
        public override string ColorCodeHex => "#808080";
    }
}
