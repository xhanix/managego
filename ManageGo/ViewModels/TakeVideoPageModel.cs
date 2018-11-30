using System;
using System.IO;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo
{
    public class TakeVideoPageModel : FreshBasePageModel
    {
        public CameraPreview CameraPreviewContent { get; set; }
        string FileUrl { get; set; }
        public string ToggleButtonText { get; private set; }
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
            CameraPreviewContent.SavedMovie += async (object sender, ListEventArgs e) =>
            {
                //this can be a movie ".mp4" or a photo
                FileUrl = e.MovieUrl;
                if (Path.GetExtension(FileUrl).Equals(".mp4", StringComparison.CurrentCultureIgnoreCase))
                {
                    //need to pop the current(cam preview) page for Android to playback the video (old android phone/memory issue)
                    await CoreMethods.PopPageModel(modal: true, animate: false);
                    if (CameraPreviewContent.Mode == CameraModes.Video)
                        await CoreMethods.PushPageModel<VideoPlayerPageModel>(data: FileUrl, modal: false, animate: false);

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
