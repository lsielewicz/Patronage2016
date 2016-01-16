using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Xaml.Media.Imaging;
using Patronage2016.Model;

namespace Patronage2016.Messages
{
    public class PassedData
    {
        public List<PictureFile> PictureInformations { get; set; }

        public PassedData(List<PictureFile> picInfo)
        {
            PictureInformations = picInfo;
        }
    }
}
