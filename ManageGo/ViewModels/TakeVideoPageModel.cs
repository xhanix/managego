using System;
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
                FileUrl = e.MovieUrl;

                await CoreMethods.PushPageModel<MGWebViewPageModel>(data: new Tuple<string, bool>(FileUrl, true),
                                                                    modal: true);
            };
        }

        public FreshAwaitCommand OnCancelTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    await CoreMethods.PopPageModel(modal: true, animate: false);
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnModeToggleTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
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
                //if webview returned true we should close this page & use the file for attachment
                if ((bool)returnedData)
                    await CoreMethods.PopPageModel(data: FileUrl, modal: true);

            }
        }
    }
}
