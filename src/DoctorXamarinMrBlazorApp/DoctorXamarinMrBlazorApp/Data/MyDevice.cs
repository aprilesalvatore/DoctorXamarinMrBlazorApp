using System;
using System.Collections.Generic;
using System.Text;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

namespace DoctorXamarinMrBlazorApp.Data
{
    public class MyDevice : BindableObject
    {
        public IDevice Device { get; private set; }

        public Guid Id { get; set; }

        public bool Autoconnect { get; set; }
        public bool IsConnected => Device != null && Device.State == DeviceState.Connected;
        public bool IsStoredDevice => Device == null;
        public int Rssi => Device.Rssi;
        public string Name { get; set; }

        public MyDevice()
        {

        }

        public MyDevice(IDevice device)
        {
            Device = device;
            Id = Device.Id;
            Name = Device.Name ?? Device.Id.ToString();
        }

        public void Update(IDevice newDevice = null)
        {
            if (newDevice != null)
            {
                Device = newDevice;
                Id = newDevice.Id;
                Name = Device.Name;
            }
            Refresh();
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(Rssi));
            OnPropertyChanged(nameof(Name));
        }
    }
}
