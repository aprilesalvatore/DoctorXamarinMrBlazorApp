using System;
using System.Security;
using Microsoft.MobileBlazorBindings;
using Microsoft.Extensions.Hosting;
using Xamarin.Essentials;
using Xamarin.Forms;
using Microsoft.Extensions.DependencyInjection;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace DoctorXamarinMrBlazorApp
{
    public class App : Application
    {
        public App()
        {
            var host = MobileBlazorBindingsHost.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Register app-specific services
                    //services.AddSingleton<AppState>();
                    services.AddSingleton<BLEService>();
                    services.AddSingleton<IPermissions>((e) => CrossPermissions.Current);
                    services.AddSingleton<IBluetoothLE>((e) => CrossBluetoothLE.Current);
                    services.AddSingleton<IAdapter>((e) => CrossBluetoothLE.Current.Adapter);
                })
                .Build();

            MainPage = new ContentPage();
            host.AddComponent<AppPage>(parent: MainPage);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
