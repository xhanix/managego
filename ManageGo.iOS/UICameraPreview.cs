using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using CoreVideo;
using Foundation;
using UIKit;

namespace ManageGo.iOS
{
    public partial class UICameraPreview : UIView
    {
        AVCaptureVideoPreviewLayer previewLayer;
        CameraOptions cameraOptions;
        SpeedOptions speedOptions;
        AVCaptureMovieFileOutput movieFileOutput;
        AVCapturePhotoOutput photoFileOutput;
        protected bool isRecording;
        string VideoFileUrl { get; set; }
        string FileUrl { get; set; }
        public CameraModes CameraMode { get; set; }
        UIPaintCodeButton takePhotoButton;
        UIPaintCodeButton stopRecordButton;
        bool setupInterface;


        public event EventHandler StartedRecordingVideo;
        public event EventHandler<string> Video;
        public event EventHandler<string> PhotoPath;
        public AVCaptureSession CaptureSession { get; private set; }

        public bool IsPreviewing { get; set; }

        public UICameraPreview(CameraOptions options, SpeedOptions sOptions, string fileUrl, CameraModes mode)
        {
            cameraOptions = options;
            speedOptions = sOptions;
            CameraMode = mode;
            IsPreviewing = false;
            fileUrl = FileUrl;
            Initialize();
            //SetupUserInterface();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            previewLayer.Frame = rect;
            if (!setupInterface)
            {
                setupInterface = true;
                //SetupUserInterface();
            }
        }



        protected void Initialize()
        {
            // configure the capture session for medium resolution, change this if your code
            // can cope with more data or volume
            CaptureSession = new AVCaptureSession
            {
                SessionPreset = AVCaptureSession.PresetMedium
            };
            previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
            {
                Frame = Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

            var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

            if (device == null)
            {
                return;
            }

            // SET to slow motion



            NSError error;
            var input = new AVCaptureDeviceInput(device, out error);

            movieFileOutput = new AVCaptureMovieFileOutput
            {
                //set max record time to 10 minutes
                MaxRecordedDuration = CMTime.FromSeconds(600, 1)
            };


            photoFileOutput = new AVCapturePhotoOutput();

            photoFileOutput.IsHighResolutionCaptureEnabled = true;

            if (CaptureSession.CanAddOutput(movieFileOutput))
            {
                CaptureSession.BeginConfiguration();
                CaptureSession.AddOutput(movieFileOutput);
                CaptureSession.AddOutput(photoFileOutput);
                var ranges = device.ActiveFormat.VideoSupportedFrameRateRanges;
                if (device.LockForConfiguration(out error))
                {
                    device.ActiveVideoMinFrameDuration = new CMTime(1, (int)ranges.First().MinFrameRate);
                    device.ActiveVideoMaxFrameDuration = new CMTime(1, (int)ranges.First().MaxFrameRate);

                }

                var connection = movieFileOutput.ConnectionFromMediaType(AVMediaType.Video);
                if (connection != null)
                {
                    if (connection.SupportsVideoStabilization)
                        connection.PreferredVideoStabilizationMode = AVCaptureVideoStabilizationMode.Auto;


                }
                CaptureSession.CommitConfiguration();
            }

            CaptureSession.AddInput(input);
            Layer.AddSublayer(previewLayer);
            CaptureSession.StartRunning();
            // set frame rate if Slow-mo is requested
            if (speedOptions == SpeedOptions.SlowMo)
            {
                foreach (var vFormat in device.Formats)
                {
                    var _ranges = vFormat.VideoSupportedFrameRateRanges as AVFrameRateRange[];
                    var frameRates = _ranges[0];

                    if (frameRates.MaxFrameRate >= 240.0)
                    {
                        device.LockForConfiguration(out NSError _error);
                        if (_error is null)
                        {
                            device.ActiveFormat = vFormat as AVCaptureDeviceFormat;
                            device.ActiveVideoMinFrameDuration = frameRates.MinFrameDuration;
                            device.ActiveVideoMaxFrameDuration = frameRates.MaxFrameDuration;
                            device.UnlockForConfiguration();
                            break;
                        }
                    }
                }
            }


            IsPreviewing = true;
        }

        private void SetupUserInterface()
        {

            var centerButtonX = previewLayer.Frame.GetMidX() - 35;
            var bottomButtonY = previewLayer.Frame.Bottom - 85;
            var buttonWidth = 70;
            var buttonHeight = 70;


            takePhotoButton = new UIPaintCodeButton(DrawTakePhotoButton)
            {
                Frame = new CGRect(centerButtonX, bottomButtonY, buttonWidth, buttonHeight)
            };

            stopRecordButton = new UIPaintCodeButton(DrawStopRecordButton)
            {
                Frame = new CGRect(centerButtonX, bottomButtonY, buttonWidth, buttonHeight)

            };

            takePhotoButton.TouchUpInside += (s, e) =>
           {
               CaptureMovie();
           };
            stopRecordButton.TouchUpInside += (s, e) =>
           {
               CaptureMovie();
           };

            // this.Add(takePhotoButton);
            // stopRecordButton.Hidden = true;
            // this.Add(stopRecordButton);
        }


        #region Drawings
        private void DrawStopRecordButton(CGRect frame)
        {
            var color = UIColor.Red;
            var RectPath = UIBezierPath.FromRect(new CGRect(NMath.Floor((frame.Width * 0.5f) - 24),
                                                             NMath.Floor((frame.Height * 0.5f) - 24),
                                                            48, 48));
            color.SetFill();
            RectPath.Fill();
            UIColor.White.SetStroke();
            RectPath.LineWidth = 1.0f;
            RectPath.Stroke();
        }


        private void DrawTakePhotoButton(CGRect frame)
        {
            var color = UIColor.White;
            var bezierPath = new UIBezierPath();
            bezierPath.MoveTo(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.27302f * frame.Width, frame.GetMinY() + 0.15053f * frame.Height), new CGPoint(frame.GetMinX() + 0.41628f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height), new CGPoint(frame.GetMinX() + 0.33832f * frame.Width, frame.GetMinY() + 0.10803f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.08333f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.15883f * frame.Width, frame.GetMinY() + 0.22484f * frame.Height), new CGPoint(frame.GetMinX() + 0.08333f * frame.Width, frame.GetMinY() + 0.35360f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.91667f * frame.Height), new CGPoint(frame.GetMinX() + 0.08333f * frame.Width, frame.GetMinY() + 0.73012f * frame.Height), new CGPoint(frame.GetMinX() + 0.26988f * frame.Width, frame.GetMinY() + 0.91667f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.91667f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.73012f * frame.Width, frame.GetMinY() + 0.91667f * frame.Height), new CGPoint(frame.GetMinX() + 0.91667f * frame.Width, frame.GetMinY() + 0.73012f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height), new CGPoint(frame.GetMinX() + 0.91667f * frame.Width, frame.GetMinY() + 0.26988f * frame.Height), new CGPoint(frame.GetMinX() + 0.73012f * frame.Width, frame.GetMinY() + 0.08333f * frame.Height));
            bezierPath.ClosePath();
            bezierPath.MoveTo(new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 1.00000f * frame.Height), new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.77614f * frame.Height), new CGPoint(frame.GetMinX() + 0.77614f * frame.Width, frame.GetMinY() + 1.00000f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.00000f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.22386f * frame.Width, frame.GetMinY() + 1.00000f * frame.Height), new CGPoint(frame.GetMinX() + 0.00000f * frame.Width, frame.GetMinY() + 0.77614f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.19894f * frame.Width, frame.GetMinY() + 0.10076f * frame.Height), new CGPoint(frame.GetMinX() + 0.00000f * frame.Width, frame.GetMinY() + 0.33689f * frame.Height), new CGPoint(frame.GetMinX() + 0.07810f * frame.Width, frame.GetMinY() + 0.19203f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.00000f * frame.Height), new CGPoint(frame.GetMinX() + 0.28269f * frame.Width, frame.GetMinY() + 0.03751f * frame.Height), new CGPoint(frame.GetMinX() + 0.38696f * frame.Width, frame.GetMinY() + 0.00000f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.77614f * frame.Width, frame.GetMinY() + 0.00000f * frame.Height), new CGPoint(frame.GetMinX() + 1.00000f * frame.Width, frame.GetMinY() + 0.22386f * frame.Height));
            bezierPath.ClosePath();
            color.SetFill();
            bezierPath.Fill();
            UIColor.Black.SetStroke();
            bezierPath.LineWidth = 1.0f;
            bezierPath.Stroke();
            var ovalPath = UIBezierPath.FromOval(new CGRect(frame.GetMinX() + NMath.Floor(frame.Width * 0.12500f + 0.5f), frame.GetMinY() + NMath.Floor(frame.Height * 0.12500f + 0.5f), NMath.Floor(frame.Width * 0.87500f + 0.5f) - NMath.Floor(frame.Width * 0.12500f + 0.5f), NMath.Floor(frame.Height * 0.87500f + 0.5f) - NMath.Floor(frame.Height * 0.12500f + 0.5f)));
            color.SetFill();
            ovalPath.Fill();
            UIColor.Black.SetStroke();
            ovalPath.LineWidth = 1.0f;
            ovalPath.Stroke();
        }

        private void DrawCancelPictureButton(CGRect frame)
        {
            var color2 = UIColor.Red;
            var bezierPath = new UIBezierPath();
            bezierPath.MoveTo(new CGPoint(frame.GetMinX() + 0.73928f * frame.Width, frame.GetMinY() + 0.14291f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.85711f * frame.Width, frame.GetMinY() + 0.26074f * frame.Height), new CGPoint(frame.GetMinX() + 0.73926f * frame.Width, frame.GetMinY() + 0.14289f * frame.Height), new CGPoint(frame.GetMinX() + 0.85711f * frame.Width, frame.GetMinY() + 0.26074f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.61785f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.85711f * frame.Width, frame.GetMinY() + 0.26074f * frame.Height), new CGPoint(frame.GetMinX() + 0.74457f * frame.Width, frame.GetMinY() + 0.37328f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.85355f * frame.Width, frame.GetMinY() + 0.73570f * frame.Height), new CGPoint(frame.GetMinX() + 0.74311f * frame.Width, frame.GetMinY() + 0.62526f * frame.Height), new CGPoint(frame.GetMinX() + 0.85355f * frame.Width, frame.GetMinY() + 0.73570f * frame.Height));
            bezierPath.AddLineTo(new CGPoint(frame.GetMinX() + 0.73570f * frame.Width, frame.GetMinY() + 0.85355f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.61785f * frame.Height), new CGPoint(frame.GetMinX() + 0.73570f * frame.Width, frame.GetMinY() + 0.85355f * frame.Height), new CGPoint(frame.GetMinX() + 0.62526f * frame.Width, frame.GetMinY() + 0.74311f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.26785f * frame.Width, frame.GetMinY() + 0.85000f * frame.Height), new CGPoint(frame.GetMinX() + 0.37621f * frame.Width, frame.GetMinY() + 0.74164f * frame.Height), new CGPoint(frame.GetMinX() + 0.26785f * frame.Width, frame.GetMinY() + 0.85000f * frame.Height));
            bezierPath.AddLineTo(new CGPoint(frame.GetMinX() + 0.15000f * frame.Width, frame.GetMinY() + 0.73215f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.38215f * frame.Width, frame.GetMinY() + 0.50000f * frame.Height), new CGPoint(frame.GetMinX() + 0.15000f * frame.Width, frame.GetMinY() + 0.73215f * frame.Height), new CGPoint(frame.GetMinX() + 0.25836f * frame.Width, frame.GetMinY() + 0.62379f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.14645f * frame.Width, frame.GetMinY() + 0.26430f * frame.Height), new CGPoint(frame.GetMinX() + 0.25689f * frame.Width, frame.GetMinY() + 0.37474f * frame.Height), new CGPoint(frame.GetMinX() + 0.14645f * frame.Width, frame.GetMinY() + 0.26430f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.22060f * frame.Width, frame.GetMinY() + 0.19014f * frame.Height), new CGPoint(frame.GetMinX() + 0.14645f * frame.Width, frame.GetMinY() + 0.26430f * frame.Height), new CGPoint(frame.GetMinX() + 0.18706f * frame.Width, frame.GetMinY() + 0.22369f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.26430f * frame.Width, frame.GetMinY() + 0.14645f * frame.Height), new CGPoint(frame.GetMinX() + 0.24420f * frame.Width, frame.GetMinY() + 0.16655f * frame.Height), new CGPoint(frame.GetMinX() + 0.26430f * frame.Width, frame.GetMinY() + 0.14645f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.50000f * frame.Width, frame.GetMinY() + 0.38215f * frame.Height), new CGPoint(frame.GetMinX() + 0.26430f * frame.Width, frame.GetMinY() + 0.14645f * frame.Height), new CGPoint(frame.GetMinX() + 0.37474f * frame.Width, frame.GetMinY() + 0.25689f * frame.Height));
            bezierPath.AddCurveToPoint(new CGPoint(frame.GetMinX() + 0.73926f * frame.Width, frame.GetMinY() + 0.14289f * frame.Height), new CGPoint(frame.GetMinX() + 0.62672f * frame.Width, frame.GetMinY() + 0.25543f * frame.Height), new CGPoint(frame.GetMinX() + 0.73926f * frame.Width, frame.GetMinY() + 0.14289f * frame.Height));
            bezierPath.AddLineTo(new CGPoint(frame.GetMinX() + 0.73928f * frame.Width, frame.GetMinY() + 0.14291f * frame.Height));
            bezierPath.ClosePath();
            color2.SetFill();
            bezierPath.Fill();
            UIColor.Black.SetStroke();
            bezierPath.LineWidth = 1.0f;
            bezierPath.Stroke();
        }
        #endregion

        private NSUrl ApplicationDocumentsDirectory()
        {
            return NSFileManager.DefaultManager
                                .GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0];
        }

        public void CaptureMovie()
        {
            //start recording

            if (!isRecording && CameraMode == CameraModes.Video)
            {
                var outputUrl = ApplicationDocumentsDirectory().Append($"Video_{DateTime.Now.ToString("yyMMdd_hhmmss")}", false).AppendPathExtension("mov");
                VideoFileUrl = outputUrl.AbsoluteString;
                // string localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                // var localPath = System.IO.Path.Combine(localFolder, $"Video_{DateTime.Now.ToString("yyMMdd_hhmmss")}.mov");
                //  var outputUrl = ApplicationDocumentsDirectory().Append($"Video_{DateTime.Now.ToString("yyMMdd-hhmmss")}", false).AppendPathExtension("mov");
                var _delegate = new OutputRecorder();
                _delegate.SavedMovie += (sender, e) =>
                 {
                     Video?.Invoke(this, VideoFileUrl);
                     /// SavedMovie(this, new ListEventArgs(outputUrl.AbsoluteString, e.error?.LocalizedDescription));
                     //Analytics.TrackEvent("Trigger seconds SavedMovie Event");

                 };
                movieFileOutput.StartRecordingToOutputFile(outputUrl, _delegate);
                //takePhotoButton.Hidden = true;
                //stopRecordButton.Hidden = false;
                StartedRecordingVideo?.Invoke(this, EventArgs.Empty);
            }
            else if (isRecording && CameraMode == CameraModes.Video)
            {
                movieFileOutput.StopRecording();
                //takePhotoButton.Hidden = false;
                //stopRecordButton.Hidden = true;
            }
            else if (CameraMode == CameraModes.Snapshot)
            {

                var cb = new PhotoRecorderDelegate();
                cb.SavedPhoto += (object sender, ListEventArgs e) =>
                {
                    /// SavedMovie(this, e);
                    PhotoPath?.Invoke(this, e.MovieUrl);

                };
                var dictionary = new NSDictionary<NSString, NSObject>
                (
                    AVVideo.CodecKey, AVVideo.CodecJPEG
                );
                var setting = AVCapturePhotoSettings.FromFormat(dictionary);
                setting.IsHighResolutionPhotoEnabled = true;
                setting.IsAutoStillImageStabilizationEnabled = true;
                photoFileOutput.CapturePhoto(setting, cb);
            }

            isRecording = !isRecording;
        }



        public AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
        {
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

            foreach (var device in devices)
            {
                if (device.Position == orientation)
                {
                    return device;
                }
            }
            return null;
        }


        public void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }

        internal class UIPaintCodeButton : UIButton
        {
            Action<CGRect> _drawing;
            public UIPaintCodeButton(Action<CGRect> drawing)
            {
                _drawing = drawing;
            }

            public override void Draw(CGRect rect)
            {
                base.Draw(rect);
                _drawing(rect);
            }

        }
    }

    public class PhotoRecorderDelegate : AVCapturePhotoCaptureDelegate
    {
        public event EventHandler<ListEventArgs> SavedPhoto;

        [Export("captureOutput:didFinishProcessingPhoto:error:")]
        override public void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError error)
        {
            string localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var localPath = System.IO.Path.Combine(localFolder, $"Photo_{DateTime.Now.ToString("yyMMdd_hhmmss")}.jpg");
            var errMsg = error?.Description;
            try
            {
                System.IO.File.WriteAllBytes(localPath, photo.FileDataRepresentation.ToArray()); // write to local storage
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            finally
            {
                if (SavedPhoto != null)
                    SavedPhoto(this, new ListEventArgs(localPath, errMsg));
            }
        }
    }
}
