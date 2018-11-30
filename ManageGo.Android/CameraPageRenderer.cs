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

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPageRenderer))]
namespace ManageGo.Droid
{



    public class CameraPageRenderer : ViewRenderer<CameraPreview, UICameraPreview>
    {

        UICameraPreview cameraPreview;
        private MediaRecorder recorder;
        string LocalPath { get; set; }
        public bool VideoIsRecording { get; private set; }

        public CameraPageRenderer(Context context) : base(context)
        {


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
                    cameraPreview = new UICameraPreview(Context);
                    SetNativeControl(cameraPreview);
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
                cameraPreview.Click -= OnCameraPreviewClicked;
            }
            if (e.NewElement != null)
            {
                Control.Preview = Android.Hardware.Camera.Open((int)e.NewElement.Camera);

                // Subscribe
                cameraPreview.Click += OnCameraPreviewClicked;
            }
        }




        void OnCameraPreviewClicked(object sender, EventArgs e)
        {
            // string localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            //  var localPath = System.IO.Path.Combine(localFolder, $"Video_{DateTime.Now.ToString("yyMMdd-hhmmss")}.mp4");


            if (cameraPreview.IsPreviewing)
            {
                if (Element.Mode == CameraModes.Snapshot)
                {
                    var _delegate = new Jpeg(Context);
                    _delegate.SavedMovie += _delegate_SavedMovie;
                    cameraPreview.Preview.TakePicture(null, null, null, jpeg: _delegate);
                    cameraPreview.IsPreviewing = false;
                }
                else if (Element.Mode == CameraModes.Video && !VideoIsRecording)
                {
                    cameraPreview.Preview.StopPreview();

                    if (recorder is null)
                    {
                        cameraPreview.Preview.Unlock();
                        recorder = new MediaRecorder();
                        recorder.SetCamera(cameraPreview.Preview);
                        recorder.SetVideoSource(VideoSource.Camera);
                        recorder.SetAudioSource(AudioSource.Mic);
                        recorder.SetOutputFormat(OutputFormat.Mpeg4);
                        recorder.SetVideoEncoder(VideoEncoder.H264);
                        recorder.SetAudioEncoder(AudioEncoder.Default);
                        recorder.SetPreviewDisplay(Control.Holder.Surface);

                    }

                    recorder.SetOrientationHint(cameraPreview.CamRotation);
                    LocalPath = Android.OS.Environment.ExternalStorageDirectory + $"/Video_{DateTime.Now.ToString("yyMMdd-hhmmss")}.mp4";
                    recorder.SetOutputFile(LocalPath);
                    recorder.Prepare();
                    recorder.Start();
                    VideoIsRecording = true;
                }
                else if (Element.Mode == CameraModes.Video && VideoIsRecording && !string.IsNullOrWhiteSpace(LocalPath))
                {
                    recorder.Stop();
                    VideoIsRecording = false;
                    recorder.Release();
                    Element.SavedMoview(LocalPath, string.Empty);
                }
            }
            else
            {
                cameraPreview.Preview.StartPreview();
                cameraPreview.IsPreviewing = true;
            }
        }

        void _delegate_SavedMovie(object sender, ListEventArgs e)
        {
            var url = e.MovieUrl;
            Element.SavedMoview(url, e.ErrorMessage);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control.Preview.Release();
            }
            base.Dispose(disposing);
        }

    }
}
