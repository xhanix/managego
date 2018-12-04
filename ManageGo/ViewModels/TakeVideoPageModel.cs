using System;
using System.IO;
using Android.Media;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo
{
    public class TakeVideoPageModel : FreshBasePageModel
    {
        #region Events
        public event EventHandler<Orientation> OrientationChange;
        public event EventHandler<Point> Focus;
        public event EventHandler<bool> AvailibilityChange;
        public event EventHandler<bool> OpenCamera;
        public event EventHandler<bool> Busy;
        public event EventHandler<bool> Flash;
        public event EventHandler<bool> Torch;
        public event EventHandler<bool> Loading;
        public event EventHandler<byte[]> Photo;
        public event EventHandler<float> Widths;
        public event EventHandler Shutter;
        #endregion

        public CameraPreview CameraPreviewContent { get; set; }

        string FileUrl { get; set; }
        public string ToggleButtonText { get; private set; }

        #region Public Methods
        public void NotifyShutter()
        {
            Shutter?.Invoke(this, EventArgs.Empty);
        }
        public void NotifyOpenCamera(bool open)
        {
            OpenCamera?.Invoke(this, open);
        }
        public void NotifyFocus(Point touchPoint)
        {
            Focus?.Invoke(this, touchPoint);
        }
        public void NotifyBusy(object sender, bool busy)
        {
            Busy?.Invoke(this, busy);
        }
        public void NotifyOrientationChange(Orientation orientation)
        {
            Orientation Orientation = orientation;
            OrientationChange?.Invoke(this, orientation);
        }
        public void NotifyAvailibility(object sender, bool isAvailable)
        {
            CameraAvailibility = isAvailable;
            AvailibilityChange?.Invoke(this, isAvailable);
        }
        public void NotifyPhoto(object sender, byte[] imageData)
        {
            Photo?.Invoke(this, imageData);
        }
        public void NotifyFlash(bool flashOn)
        {
            Flash?.Invoke(this, flashOn);
        }
        public void NotifyTorch(bool torchOn)
        {
            Torch?.Invoke(this, torchOn);
        }
        public void NotifyLoading(object sender, bool loading)
        {
            Loading?.Invoke(this, loading);
        }
        public void NotifyWidths(float cameraButtonContainerWidth)
        {
            CameraButtonContainerWidth = cameraButtonContainerWidth;
            Widths?.Invoke(this, cameraButtonContainerWidth);
        }
        #endregion

        public TakeVideoPageModel()
        {
            CameraPreviewContent = new CameraPreview
            {
                Camera = CameraOptions.Rear,
                Speed = SpeedOptions.Normal,
                Mode = CameraModes.Snapshot,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            ToggleButtonText = "VIDEO";
            CameraPreviewContent.Photo += (object sender, byte[] e) =>
            {
                //photo is ready from Android
                //show the webview for photos, preview page remains on top of current page
                string localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                var localPath = System.IO.Path.Combine(localFolder, $"Photo_{DateTime.Now.ToString("yyMMdd-hhmmss")}.jpg");
                string errMsg = null;
                try
                {
                    System.IO.File.WriteAllBytes(localPath, e); // write to local storage
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
                FileUrl = localPath;
                Device.BeginInvokeOnMainThread(async () =>
                    await CoreMethods.PushPageModel<MGWebViewPageModel>
                        (data: new Tuple<string, bool>(localPath, true),
                                                              modal: true, animate: false));
            };



            CameraPreviewContent.SavedMovie += async (object sender, ListEventArgs e) =>
            {
                //this can be a movie ".mp4" or a photo
                FileUrl = e.MovieUrl;
                if (Path.GetExtension(FileUrl).Equals(".mp4", StringComparison.CurrentCultureIgnoreCase))
                {
                    //need to pop the current(cam preview) page for Android to playback the video (old android phone/memory issue)
                    // await CoreMethods.PopPageModel(modal: true, animate: false);
                    if (CameraPreviewContent.Mode == CameraModes.Video)
                        await CoreMethods.PushPageModel<VideoPlayerPageModel>(data: FileUrl, modal: true, animate: false);

                }
                else
                {
                    //show the webview for photos, preview page remains on top of current page
                    await CoreMethods.PushPageModel<MGWebViewPageModel>(data: new Tuple<string, bool>(FileUrl, true),
                                                                  modal: true);
                }

            };
        }

        public FreshAwaitCommand OnCancelTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    //user does not want to take photo, close cam preview page
                    await CoreMethods.PopPageModel(modal: true, animate: false);
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnModeToggleTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (CameraPreviewContent is null)
                        return;
                    else if (CameraPreviewContent.Mode == CameraModes.Snapshot)
                    {
                        CameraPreviewContent.Mode = CameraModes.Video;
                        ToggleButtonText = "PHOTO";
                    }
                    else
                    {
                        CameraPreviewContent.Mode = CameraModes.Snapshot;
                        ToggleButtonText = "VIDEO";
                    }

                    tcs?.SetResult(true);
                });
            }
        }

        public bool CameraAvailibility { get; private set; }
        public float CameraButtonContainerWidth { get; private set; }

        public override async void ReverseInit(object returnedData)
        {
            base.ReverseInit(returnedData);
            if (returnedData is bool)
            {
                //if webview/video view returned true we should close current page & use the file for attachment
                if ((bool)returnedData)
                    await CoreMethods.PopPageModel(data: FileUrl, modal: true);

            }
        }
    }
}
