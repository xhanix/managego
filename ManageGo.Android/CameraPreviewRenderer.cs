using System;
using System.ComponentModel;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using ManageGo;
using Plugin.CurrentActivity;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using RelativeLayout = Android.Widget.RelativeLayout;
using System.Linq;
using Plugin.Permissions;
using Android.Hardware;
using Android.Media;
using Android.Util;
using Android.Runtime;

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPreviewRenderer))]
namespace ManageGo.Droid
{

    public class CameraPreviewRenderer : ViewRenderer<CameraPreview, CamRecorder>//ViewRenderer<CameraPreview, CameraDroid>
    {


        //CameraDroid Camera;
        CamRecorder Camera;
        readonly Context _context;
        string LocalPath { get; set; }
        public bool VideoIsRecording { get; private set; }
        private SparseIntArray ORIENTATIONS = new SparseIntArray();
        public CameraPreviewRenderer(Context context) : base(context)
        {

            _context = context;
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<CameraPreview> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Camera);
                var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                var audioStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Microphone);
                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted ||
                audioStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted ||
                storageStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted
                    )
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Camera))
                    {
                        Android.App.AlertDialog.Builder dialog = new AlertDialog.Builder(Context);
                        AlertDialog alert = dialog.Create();
                        alert.SetTitle("Need Camera Permission");
                        alert.SetMessage("ManageGo needs camera access");
                        alert.SetButton("OK", async (c, ev) =>
                        {
                            // Ok button click task  
                            var results = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Camera,
                                Plugin.Permissions.Abstractions.Permission.Microphone, Plugin.Permissions.Abstractions.Permission.Storage);
                            //Best practice to always check that the key exists
                            if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Camera))
                                status = results[Plugin.Permissions.Abstractions.Permission.Camera];
                            if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Microphone))
                                audioStatus = results[Plugin.Permissions.Abstractions.Permission.Microphone];
                            if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Storage))
                                storageStatus = results[Plugin.Permissions.Abstractions.Permission.Storage];
                        });
                    }
                    else
                    {
                        var results = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Camera,
                        Plugin.Permissions.Abstractions.Permission.Microphone, Plugin.Permissions.Abstractions.Permission.Storage);
                        //Best practice to always check that the key exists
                        if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Camera))
                            status = results[Plugin.Permissions.Abstractions.Permission.Camera];
                        if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Microphone))
                            audioStatus = results[Plugin.Permissions.Abstractions.Permission.Microphone];
                        if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Storage))
                            storageStatus = results[Plugin.Permissions.Abstractions.Permission.Storage];
                    }


                }
                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted
                        && storageStatus == Plugin.Permissions.Abstractions.PermissionStatus.Granted
                        && audioStatus == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {

                    //Camera = new CameraDroid(_context);
                    Camera = new CamRecorder(_context);
                    SetNativeControl(Camera);
                    //Camera.IsPreviewing = true;
                    if (e.NewElement != null)
                    {
                        // Camera.Available += e.NewElement.NotifyAvailability;
                        Camera.Photo += e.NewElement.NotifyPhoto;
                        Camera.Video += e.NewElement.NotifyVideo;
                        //Camera.Busy += e.NewElement.NotifyBusy;

                        Camera.Click += OnCameraPreviewClicked;
                        e.NewElement.Flash += HandleFlashChange;
                        e.NewElement.OpenCamera += HandleCameraInitialisation;
                        e.NewElement.CamFocus += HandleFocus;
                        e.NewElement.Shutter += HandleShutter;
                        e.NewElement.OnCaptureButtonTapped += OnCameraPreviewClicked;
                    }
                }
                else
                {
                    Android.App.AlertDialog.Builder dialog = new AlertDialog.Builder(Context);
                    AlertDialog alert = dialog.Create();
                    alert.SetTitle("Need Campera Permission");
                    alert.SetMessage("ManageGo needs camera access");
                    alert.SetButton("OK", (c, ev) =>
                    {
                        //do nothing
                    });
                }

            }

            if (e.OldElement != null)
            {
                // Unsubscribe
                //cameraPreview.Click -= OnCameraPreviewClicked;
            }
            if (e.NewElement != null)
            {
                // Control.Preview = Android.Hardware.Camera.Open((int)e.NewElement.Camera);

                // Subscribe
                // cameraPreview.Click += OnCameraPreviewClicked;
            }
        }


        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
            //Camera?.OnLayout(l, t, r, b);
        }

        void OnCameraPreviewClicked(object sender, EventArgs e)
        {
            if (Element.Mode == CameraModes.Snapshot)
            {
                VideoIsRecording = false;
                Camera.TakePicture();
            }

            else if (Element.Mode == CameraModes.Video && !VideoIsRecording)
            {
                Camera.StartRecordingVideo();
                Element.NotifyRecordingVideo();
                VideoIsRecording = true;
            }
            else if (Element.Mode == CameraModes.Video && VideoIsRecording)
            {
                VideoIsRecording = false;
                Camera.StopRecordingVideo();
                Element.NotifyStoppedRecordingVideo();
            }
            /*
            if (Camera.IsPreviewing)
            {
                //Camera.IsTakingPhoto = false;
                if (Element.Mode == CameraModes.Snapshot)
                {
                    Camera.TakePhoto();
                    VideoIsRecording = false;
                }
                else if (Element.Mode == CameraModes.Video && !VideoIsRecording)
                {
                    Camera.StartRecordingVideo();
                    Element.NotifyRecordingVideo();
                    VideoIsRecording = true;
                }
                else if (Element.Mode == CameraModes.Video && VideoIsRecording)
                {
                    VideoIsRecording = false;
                    Camera.stopRecordingVideo();
                    Element.NotifyStoppedRecordingVideo();
                    // Element.SavedMoview(LocalPath, string.Empty);
                }

            }*/
        }


        void _delegate_SavedMovie(object sender, ListEventArgs e)
        {
            var url = e.MovieUrl;
            Element.SavedMoview(url, e.ErrorMessage);
        }


        protected override void Dispose(bool disposing)
        {
            Element.Flash -= HandleFlashChange;
            Element.OpenCamera -= HandleCameraInitialisation;
            Element.CamFocus -= HandleFocus;
            Element.Shutter -= HandleShutter;
            Camera.Click -= OnCameraPreviewClicked;
            //Camera.Available -= Element.NotifyAvailability;
            // Camera.Photo -= Element.NotifyPhoto;
            // Camera.Video -= Element.NotifyVideo;
            ///Camera.Busy -= Element.NotifyBusy;
            base.Dispose(disposing);
        }


        #region Private Methods
        /// <summary>
        /// Handles the camera initialisation.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">If set to <c>true</c> arguments.</param>
        private void HandleCameraInitialisation(object sender, bool args)
        {
            ///Camera.OpenCamera();
        }

        /// <summary>
        /// Handles the flash change.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">If set to <c>true</c> arguments.</param>
        private void HandleFlashChange(object sender, bool args)
        {
            ///  Camera.SwitchFlash(args);
        }

        /// <summary>
        /// Handles the shutter.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void HandleShutter(object sender, EventArgs e)
        {
            // Camera.TakePhoto();
        }

        /// <summary>
        /// Handles the focus.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void HandleFocus(object sender, Xamarin.Forms.Point e)
        {
            ///  Camera.ChangeFocusPoint(e);
        }

        #endregion

    }
}
