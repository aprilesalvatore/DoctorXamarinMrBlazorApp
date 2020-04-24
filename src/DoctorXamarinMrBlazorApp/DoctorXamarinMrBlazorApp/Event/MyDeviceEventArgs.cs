using DoctorXamarinMrBlazorApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorXamarinMrBlazorApp.Event
{
    public class MyDeviceEventArgs : System.EventArgs
    {
        public MyDevice Device { get; set; }

        public MyDeviceEventArgs(MyDevice device)
        {
            Device = device;
        }
    }
}
