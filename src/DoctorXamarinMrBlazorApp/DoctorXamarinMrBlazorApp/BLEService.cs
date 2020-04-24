using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DoctorXamarinMrBlazorApp.Event;
using DoctorXamarinMrBlazorApp.Data;
using Newtonsoft.Json;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Xamarin.Forms;
using Plugin.Permissions.Abstractions;

namespace DoctorXamarinMrBlazorApp
{
    public class BLEService : BindableObject
    {
        public const string COMPONENT_NAME = "BLEService";

        const string LAST_CONNECTED_DEVICE_FILENAME = "LastConnectedDevice.json";

        public IBluetoothLE BluetoothLE { get; private set; }
        
        public IPermissions Permission { get; private set; }

        public IAdapter Adapter { get; private set; }

        public MyDevice ConnectedDevice { get; private set; }

        public bool IsStateOn => BluetoothLE.IsOn;

        public bool IsScanning => Adapter.IsScanning;

        public event EventHandler<MyDeviceEventArgs> MyDeviceDiscovered;

        public event EventHandler<MyDeviceEventArgs> MyDeviceConnectionLost;

        private CancellationTokenSource _cancellationTokenSource;

        public BLEService(IBluetoothLE bluetoothLe, IAdapter adapter, IPermissions permission)
        {
            Adapter = adapter;
            BluetoothLE = bluetoothLe;
            Permission = permission;
            BluetoothLE.StateChanged += BluetoothLE_StateChanged;
            Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
            Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
            Adapter.DeviceConnected += AdapterOnDeviceConnected;
        }

        private void AdapterOnDeviceConnected(object sender, DeviceEventArgs e)
        {
            
        }

        private void Adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            OnMyDeviceConnectionLost(e.Device);
            MessagingCenter.Send<BLEService, ToastNotificationEventArgs>(this, string.Empty, new ToastNotificationEventArgs("The connection of {0} device has been lost. Message {1}", e.Device.Name, e.ErrorMessage));
        }

        private void Adapter_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            MessagingCenter.Send<BLEService, ToastNotificationEventArgs>(this, string.Empty, new ToastNotificationEventArgs("The Device {0} is disconnected", e.Device.Name));
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsScanning));
        }

        private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            //if (!String.IsNullOrEmpty(e.Device.Name))
            OnMyDeviceDiscovered(e.Device);
        }

        private void BluetoothLE_StateChanged(object sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            OnPropertyChanged(nameof(IsStateOn));
        }


        private void OnMyDeviceDiscovered(IDevice device)
        {
            var handler = MyDeviceDiscovered;
            if (handler != null)
            {
                handler(this, new MyDeviceEventArgs(new MyDevice(device)));
            }
        }

        private void OnMyDeviceConnectionLost(IDevice device)
        {
            var handler = MyDeviceConnectionLost;
            if (handler != null)
            {
                handler(this, new MyDeviceEventArgs(new MyDevice(device)));
            }
        }

        public async Task StartScan(ScanMode scanMode)
        {
            if (Xamarin.Forms.Device.RuntimePlatform == Device.Android)
            {
                var status = await Permission.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    var permissionResult = await Permission.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Location);

                    if (permissionResult.First().Value != PermissionStatus.Granted)
                    {
                        return;
                    }
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

            Adapter.ScanMode = scanMode;
            await Adapter.StartScanningForDevicesAsync(cancellationToken: _cancellationTokenSource.Token);
            MessagingCenter.Send<BLEService, ToastNotificationEventArgs>(this, string.Empty, new ToastNotificationEventArgs("Start scanning"));
        }

      
        public async Task DisconnectAll()
        {
            foreach (var item in Adapter.ConnectedDevices)
            {
                if (item.State == DeviceState.Connected)
                    await Adapter.DisconnectDeviceAsync(item);
            }
        }

        public async Task Connect(MyDevice myDevice)
        {
            await DisconnectAll();

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            if (myDevice != null)
            {
                if (!myDevice.IsStoredDevice && !myDevice.IsConnected)
                {
                    await Adapter.ConnectToDeviceAsync(myDevice.Device, new ConnectParameters(autoConnect: true, forceBleTransport: false), tokenSource.Token);

                }
                else
                {
                    var currenctDevice = await Adapter.ConnectToKnownDeviceAsync(myDevice.Id, new ConnectParameters(autoConnect: true, forceBleTransport: false), tokenSource.Token);

                    if (currenctDevice != null)
                        myDevice.Update(currenctDevice);
                }
                ConnectedDevice = myDevice;

                MessagingCenter.Send<BLEService, ConnectDeviceEventArgs>(this, string.Empty, new ConnectDeviceEventArgs(myDevice, true));
                MessagingCenter.Send<BLEService, ToastNotificationEventArgs>(this, string.Empty, new ToastNotificationEventArgs("Connected to {0} device", myDevice.Name));
            }
            myDevice.Autoconnect = true;

            myDevice.Refresh();
        }

        public async Task Disconnect(MyDevice device)
        {
            try
            {
                if (!device.IsConnected)
                    return;

                await Adapter.DisconnectDeviceAsync(device.Device);

                device.Autoconnect = false;
               

                MessagingCenter.Send<BLEService, ConnectDeviceEventArgs>(this, string.Empty, new ConnectDeviceEventArgs(device, false));
            }
            catch (Exception ex)
            {
                throw new Exception("Disconnected Operation exception", ex);
            }
            finally
            {
                device.Refresh();
            }
        }

        public async Task<MyService> GetMyService()
        {
            MyService myService = null;

            var res = await ConnectedDevice.Device.GetServicesAsync();

            if (res != null && res.Count > 0)
            {
                var service = res.FirstOrDefault(x => !x.Name.ToUpperInvariant().Contains("GENERIC"));

                if (service != null)
                {
                    var chars = await service.GetCharacteristicsAsync();

                    if (chars != null && chars.Count > 0)
                    {
                        myService = new MyService(service, chars.FirstOrDefault());
                    }
                }
            }

            return myService;
        }
    }
}
