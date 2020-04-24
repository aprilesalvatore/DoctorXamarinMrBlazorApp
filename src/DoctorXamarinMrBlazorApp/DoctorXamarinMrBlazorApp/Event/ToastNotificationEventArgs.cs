using System;
using System.Collections.Generic;
using System.Text;

namespace DoctorXamarinMrBlazorApp.Event
{
    public class ToastNotificationEventArgs
    {
        public string Message { get; set; }

        public object[] Args { get; set; }

        public ToastNotificationEventArgs(string message, params object[] args)
        {
            Message = message;
            Args = args;
        }
    }
}
