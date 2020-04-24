using System;
using System.Collections.Generic;
using System.Text;
using DoctorXamarinMrBlazorApp.Data;

namespace DoctorXamarinMrBlazorApp.Event
{
    public class ConnectDeviceEventArgs : System.EventArgs
    {
        public MyDevice Device { get; set; }

        public bool IsConnected { get; set; }

        public ConnectDeviceEventArgs(MyDevice device, bool isConnected)
        {
            IsConnected = isConnected;
            Device = device;
        }
    }
}
