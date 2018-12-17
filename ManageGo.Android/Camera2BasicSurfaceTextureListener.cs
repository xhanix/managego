using System;
using Android.Views;

namespace ManageGo.Droid
{
    public class Camera2BasicSurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        private readonly CamRecorder owner;

        public Camera2BasicSurfaceTextureListener(CamRecorder owner)
        {
            this.owner = owner ?? throw new System.ArgumentNullException(nameof(owner));
        }

        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            owner.SurfaceWidth = width;
            owner.SurfaceHeight = height;
            owner.OpenCamera(width, height);
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            // IMPORTANT TO CLOSE CAMERA. 
            // OLDER CAMERA HARWARE CAN CRASH IF WE DONT CLOSE HERE
            owner.CloseCamera();
            owner.CloseCameraVideo();
            return true;
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            owner.ConfigureTransform(width, height);
        }

        public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {

        }
    }
}
