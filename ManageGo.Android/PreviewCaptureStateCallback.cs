using System;
using Android.Hardware.Camera2;

namespace ManageGo.Droid
{
    public class PreviewCaptureStateCallback : CameraCaptureSession.StateCallback
    {
        CamRecorder owner;
        public PreviewCaptureStateCallback(CamRecorder frag)
        {
            owner = frag;
        }

        public override void OnConfigured(CameraCaptureSession session)
        {
            owner.mCaptureSession = session;
            owner.updatePreview();

        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {

        }
    }
}
