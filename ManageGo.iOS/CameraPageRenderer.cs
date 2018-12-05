using System.Threading.Tasks;
using AVFoundation;
using ManageGo.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using ManageGo;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPageRenderer))]
namespace ManageGo.iOS
{
    public class CameraPageRenderer : ViewRenderer<CameraPreview, UICameraPreview>
    {
        UICameraPreview uiCameraPreview;

        protected override void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                uiCameraPreview = new UICameraPreview(e.NewElement.Camera, e.NewElement.Speed, e.NewElement.FileUrl, e.NewElement.Mode);

                uiCameraPreview.StartedRecordingVideo += (object sender, System.EventArgs __e) =>
                {
                    Element.NotifyRecordingVideo();
                };
                uiCameraPreview.Video += (object sender, string filePath) =>
                {
                    Element.NotifyStoppedRecordingVideo(this, filePath);
                };
                uiCameraPreview.PhotoPath += (object sender, string photoPath) =>
                {
                    Element.NotifyPhoto(this, photoPath);
                };
                SetNativeControl(uiCameraPreview);
            }
            if (e.NewElement != null)
            {
                e.NewElement.OnCaptureButtonTapped += (sender, _e) =>
                {
                    uiCameraPreview.CaptureMovie();
                };
            }

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == "Mode")
            {
                uiCameraPreview.CameraMode = ((CameraPreview)sender).Mode;
            }
        }

        public async Task<bool> AuthorizeCameraUse()
        {
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                return await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
            }
            return await Task.FromResult(true);
        }
    }
}