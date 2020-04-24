using System;
using System.Collections.Generic;
using System.Text;
using Plugin.BLE.Abstractions.Contracts;

namespace DoctorXamarinMrBlazorApp.Data
{
    public class MyService
    {
        public MyService(IService service, ICharacteristic characteristic)
        {
            Service = service;
            Characteristic = characteristic;
        }

        public IService Service { get; set; }

        public ICharacteristic Characteristic { get; set; }
    }
}
