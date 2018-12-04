using System;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Runtime;
using Java.Nio;
using Xamarin.Forms;

namespace ManageGo.Droid
{
    public class CameraCaptureListener : CameraCaptureSession.CaptureCallback
    {
        public event EventHandler PhotoComplete;
        public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
        {
            base.OnCaptureCompleted(session, request, result);
            PhotoComplete?.Invoke(this, EventArgs.Empty);


        }
    }

    public class CameraCaptureStateListener : CameraCaptureSession.StateCallback
    {
        public Action<CameraCaptureSession> OnConfigurFailedAction;
        public Action<CameraCaptureSession> OnConfiguredAction;
        public override void OnConfigured(CameraCaptureSession session)
        {
            OnConfiguredAction?.Invoke(session);
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            if (OnConfiguredAction != null)
            {
                OnConfigurFailedAction?.Invoke(session);
            }
        }

        public class CameraStateListener : CameraDevice.StateCallback
        {
            public CamRecorder Camera;
            public override void OnDisconnected(CameraDevice camera)
            {
                camera.Close();
                if (Camera != null)
                {
                    Camera.CameraDevice = null;
                    Camera.OpeningCamera = false;
                    // Camera?.NotifyAvailable(false);
                }
            }

            public override void OnError(CameraDevice camera, [GeneratedEnum] CameraError error)
            {
                camera.Close();
                if (Camera != null)
                {
                    Camera.CameraDevice = null;
                    Camera.OpeningCamera = false;
                    //Camera?.NotifyAvailable(false);
                }
            }

            public override void OnOpened(CameraDevice camera)
            {
                if (Camera != null)
                {
                    Camera.CameraDevice = camera;
                    //Camera.StartPreview();
                    Camera.OpeningCamera = false;
                    //Camera?.NotifyAvailable(true);
                }
            }
        }


        public class ImageAvailableListner : Java.Lang.Object, ImageReader.IOnImageAvailableListener
        {
            public event EventHandler<byte[]> Photo;
            public void OnImageAvailable(ImageReader reader)
            {
                Android.Media.Image image = null;
                try
                {
                    image = reader.AcquireLatestImage();
                    ByteBuffer buffer = image.GetPlanes()[0].Buffer;
                    byte[] imageData = new byte[buffer.Capacity()];
                    buffer.Get(imageData);
                    Photo?.Invoke(this, imageData);
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (image != null)
                    {
                        image.Close();
                    }
                }
            }
        }
    }
}
