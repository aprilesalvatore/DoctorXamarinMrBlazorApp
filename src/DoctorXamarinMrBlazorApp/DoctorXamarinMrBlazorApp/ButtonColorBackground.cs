using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DoctorXamarinMrBlazorApp
{
    public class ButtonColorBackground
    {
        public static Color GetColor(bool isConnected)
        {
            if (isConnected)
                return Color.LightGreen;
            else
            {
                return Color.IndianRed;
            }
        }
    }
}

