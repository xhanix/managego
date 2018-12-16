using System;
using Android.App;
using Android.Hardware.Camera2;

namespace ManageGo.Droid
{
    public class CameraStateListener : CameraDevice.StateCallback
    {
        private readonly CamRecorder owner;

        public CameraStateListener(CamRecorder owner)
        {
            this.owner = owner ?? throw new System.ArgumentNullException(nameof(owner));
        }

        public override void OnOpened(CameraDevice camera)
        {
            // This method is called when the camera is opened.  We start camera preview here.
            owner.mCameraOpenCloseLock.Release();
            owner.mCameraDevice = camera;
            owner.CreateCameraPreviewSession(forVideo: false);
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            owner.mCameraOpenCloseLock.Release();
            camera.Close();
            owner.mCameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            owner.mCameraOpenCloseLock.Release();
            camera.Close();
            owner.mCameraDevice = null;
            if (owner == null)
                return;

        }
    }
}
