using Android.Hardware.Camera2;

namespace ManageGo.Droid
{
    internal class CameraCaptureStillPictureSessionCallback : CameraCaptureSession.CaptureCallback
    {
        private CamRecorder owner;

        public CameraCaptureStillPictureSessionCallback(CamRecorder camRecorder)
        {
            this.owner = camRecorder;
        }
        public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
        {
            owner.UnlockFocus();
        }
    }
}