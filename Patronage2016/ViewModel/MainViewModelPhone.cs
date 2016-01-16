using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Patronage2016.Messages;
using Patronage2016.Model;
using Patronage2016.Strings;

namespace Patronage2016.ViewModel
{
    class MainViewModelPhone : ViewModelBase
    {

        private List<PictureFile> picturesList;
        private BitmapImage imgSource;
        private int currentBitmapIndex;
        private string informationsTextBlock;
        private Navigation.NavigationService _nav = new Navigation.NavigationService();
        private bool progressRingActive = false;
        public MainViewModelPhone()
        {
            picturesList = new List<PictureFile>();
            Messenger.Default.Register<CurrentIndexMessage>(this, x => HandleIndexMessage(x.CurrentIndex));
            currentBitmapIndex = 0;
            SwitchImageCommand = new RelayCommand(SwitchImage);
            PhotoCommand = new RelayCommand(Photo);
            GoToPhotosListCommand = new RelayCommand(GoToPhotosList);
            ShareCommand = new RelayCommand(Share);
            Initialize();
        }

        #region Methods
        private async void Initialize()
        {
            try
            {
                ProgressRingActive = true;
                StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
                var query = CommonFileQuery.DefaultQuery;
                var queryOptions = new QueryOptions(query, new[] { ".png", ".jpg" });
                queryOptions.FolderDepth = FolderDepth.Deep;
                var queryResult = picturesFolder.CreateFileQueryWithOptions(queryOptions);
                var files = await queryResult.GetFilesAsync();

                foreach (var f in files)
                {
                    var stream = await f.OpenReadAsync();
                    var bitmapImage = new BitmapImage();

                    await bitmapImage.SetSourceAsync(await f.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem,240,ThumbnailOptions.ResizeThumbnail));


                    ImageProperties props = await GetImageProperties((StorageFile)f);
                    //thumbnails
                    var thumbnailImage = new BitmapImage();

                    thumbnailImage.SetSource(await f.GetThumbnailAsync(ThumbnailMode.ListView,80));

                    picturesList.Add(new PictureFile(bitmapImage, thumbnailImage, props, (StorageFile)f, f.Path));
                }
                if (picturesList.Count >= 1)
                {
                    ImgSource = picturesList[currentBitmapIndex].FileBitmap;
                    InformationsTextBlock = generateInformations(picturesList[currentBitmapIndex].FileProperties);
                }
            }
            catch (Exception ex)
            {
                MessageDialog msg = new MessageDialog(ex.Message);
                await msg.ShowAsync();
            }
            finally
            {
                ProgressRingActive = false;
            }
        }


        private async Task<ImageProperties> GetImageProperties(StorageFile imageFile)
        {
            ImageProperties props = await imageFile.Properties.GetImagePropertiesAsync();

            string title = props.Title;
            if (title == null)
            {
                // Format does not support, or image does not contain Title property
            }
            DateTimeOffset date = props.DateTaken;
            if (date == null)
            {
                // Format does not support, or image does not contain DateTaken property
            }

            //pictureInfo.PropertiesList.Add(props);
            return props;
        }

        #endregion

        #region Getters/Setters

        public bool ProgressRingActive
        {
            get
            {
                return progressRingActive;

            }
            set
            {
                if (progressRingActive != value)
                {
                    progressRingActive = value;
                    RaisePropertyChanged();
                }
            }
        }
        public string InformationsTextBlock
        {
            get { return informationsTextBlock; }
            set
            {
                if (informationsTextBlock != value)
                {
                    informationsTextBlock = value;
                    RaisePropertyChanged();
                }
            }
        }

        public BitmapImage ImgSource
        {
            get { return imgSource; }
            set
            {
                if (imgSource != value)
                {
                    imgSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Buttons
        public ICommand SwitchImageCommand { get; set; }

        private void SwitchImage()
        {
            int? lastIndex = picturesList.Count - 1;
            if (picturesList.Count > 1)
            {
                if (currentBitmapIndex < lastIndex)
                    currentBitmapIndex++;
                else if (currentBitmapIndex == lastIndex)
                    currentBitmapIndex = 0;
                ImgSource = picturesList[currentBitmapIndex].FileBitmap;
                InformationsTextBlock = generateInformations(picturesList[currentBitmapIndex].FileProperties);
            }
        }

        private string generateInformations(ImageProperties props)
        {
            string infos = "\n" + (new LocalString("PICTURE INFORMATIONS").text);
            if (!string.IsNullOrEmpty(props.CameraManufacturer))
                infos += "\n" + (new LocalString("CameraManufacturer").text) + props.CameraManufacturer;
            if (!string.IsNullOrEmpty(props.CameraModel))
                infos += "\n" + (new LocalString("CameraModel").text) + props.CameraModel;
            if (!string.IsNullOrEmpty(props.Title))
                infos += "\n" + (new LocalString("Title").text) + props.Title;
            if (props.Width != 0)
                infos += "\n" + (new LocalString("Width").text) + props.Width;
            if (props.Height != 0)
                infos += "\n" + (new LocalString("Height").text) + props.Height;
            DateTimeOffset? tempDate = props.DateTaken;
            if (tempDate != null)
                infos += "\n" + (new LocalString("Date taken").text) + Convert.ToString(props.DateTaken);
            if (props.Latitude != null)
                infos += "\n" + (new LocalString("Latitude").text) + Convert.ToString(props.Latitude);
            if (props.Longitude != null)
                infos += "\n" + (new LocalString("Longitude").text) + Convert.ToString(props.Longitude);

            return infos;
        }

        public ICommand PhotoCommand { get; set; }

        private async void Photo()
        {

            var devices =
                await
                    Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
                        Windows.Devices.Enumeration.DeviceClass.VideoCapture);
            if (devices.Count < 1)
            {
                MessageDialog dialogbox = new MessageDialog("There is no camera devices available", "Error");
                await dialogbox.ShowAsync();
                return;
            }
            try
            {
                CameraCaptureUI dialog = new CameraCaptureUI();

                StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
                if (file != null)
                {
                    string photoName = GenerateFileName("Photo") + ".png";
                    var fileCopy = await file.CopyAsync(KnownFolders.CameraRoll, photoName, NameCollisionOption.GenerateUniqueName);

                    if (picturesList != null && picturesList.Count > 0)
                        picturesList.Clear();

                    await Task.Delay(TimeSpan.FromSeconds(1));
                    Initialize();
                }

            }
            catch (Exception ex)
            {
                MessageDialog dialogbox = new MessageDialog(ex.Message, "Error");
                await dialogbox.ShowAsync();
                bool result = await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-webcam"));
            }




        }
        public ICommand GoToPhotosListCommand { get; set; }

        private void GoToPhotosList()
        {
            _nav.Navigate(typeof(View.PhotosListView));
            Messenger.Default.Send<PassedData>(new PassedData(picturesList));
        }
        public string GenerateFileName(string context)
        {
            return context + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Guid.NewGuid().ToString("N");
        }

        private void HandleIndexMessage(int index)
        {
            currentBitmapIndex = index;
            ImgSource = picturesList[currentBitmapIndex].FileBitmap;
            InformationsTextBlock = generateInformations(picturesList[currentBitmapIndex].FileProperties);
        }
        public ICommand ShareCommand { get; set; }
        private void Share()
        {
            DataTransferManager.ShowShareUI();
        }
        #endregion

        public void OnShareRequested(DataRequest dataRequest)
        {
            List<IStorageItem> storageList = new List<IStorageItem>();
            storageList.Add(picturesList[currentBitmapIndex].FileStorage);

            dataRequest.Data.Properties.Title = "Shared from Application";
            dataRequest.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromFile(picturesList[currentBitmapIndex].FileStorage);
            dataRequest.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(picturesList[currentBitmapIndex].FileStorage));
            dataRequest.Data.SetStorageItems(storageList);
        }
    }
}

