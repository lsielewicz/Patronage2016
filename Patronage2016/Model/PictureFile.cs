using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace Patronage2016.Model
{
   public class PictureFile
    {
        public BitmapImage FileBitmap { get; set; }
        public BitmapImage FileThumbnail { get; set; }
        public ImageProperties FileProperties { get; set; }
        public StorageFile FileStorage { get; set; }
        public string FilePath { get; set; }

       public PictureFile(BitmapImage bitmap, BitmapImage thumbnail, ImageProperties props, StorageFile storage,
           string path)
       {
           FileBitmap = bitmap;
           FileThumbnail = thumbnail;
           FileProperties = props;
           FileStorage = storage;
           FilePath = path;
       }
    }
}
