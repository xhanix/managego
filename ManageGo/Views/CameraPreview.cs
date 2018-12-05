using System;
using Xamarin.Forms;
using FreshMvvm;

namespace ManageGo
{
    public class CameraPreview : View
    {


        #region Events

        /// <summary>
        /// Occurs when orientation change.
        /// </summary>
        public event EventHandler<Orientation> OrientationChange;

        /// <summary>
        /// Occurs when focus.
        /// </summary>
        public event EventHandler<Point> CamFocus;

        /// <summary>
        /// Occurs when availability change.
        /// </summary>
        public event EventHandler<bool> AvailabilityChange;

        /// <summary>
        /// Occurs when open camera.
        /// </summary>
        public event EventHandler<bool> OpenCamera;

        /// <summary>
        /// Occurs when busy.
        /// </summary>
        public event EventHandler<bool> Busy;

        public event EventHandler RecordingVideo;
        public event EventHandler StoppedRecordingVideo;
        public event EventHandler OnCaptureButtonTapped;
        /// <summary>
        /// Occurs when flash.
        /// </summary>
        public event EventHandler<bool> Flash;

        /// <summary>
        /// Occurs when torch.
        /// </summary>
        public event EventHandler<bool> Torch;

        /// <summary>
        /// Occurs when loading.
        /// </summary>
        public event EventHandler<bool> Loading;

        /// <summary>
        /// Occurs when photo.
        /// </summary>
        public event EventHandler<byte[]> Photo;
        public event EventHandler<string> PhotoPath;

        public event EventHandler<string> Video;

        /// <summary>
        /// Occurs when widths.
        /// </summary>
        public event EventHandler<float> Widths;

        /// <summary>
        /// Occurs when shutter.
        /// </summary>
        public event EventHandler Shutter;

        #endregion

        #region Public Properties

        /// <summary>
        /// The camera available.
        /// </summary>
        public bool CameraAvailable;

        /// <summary>
        /// The orientation.
        /// </summary>
        public Orientation Orientation;

        /// <summary>
        /// The width of the camera button container.
        /// </summary>
        public float CameraButtonContainerWidth = 0f;

        #endregion

        #region Public Methods

        /// <summary>
        /// Notifies the shutter.
        /// </summary>
        public void NotifyShutter()
        {
            Shutter?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyCaptureButtonTapped()
        {
            OnCaptureButtonTapped?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyRecordingVideo()
        {
            RecordingVideo?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyStoppedRecordingVideo()
        {
            StoppedRecordingVideo?.Invoke(this, EventArgs.Empty);
        }


        public void NotifyStoppedRecordingVideo(object sender, string filePath)
        {
            Video?.Invoke(this, filePath);
        }
        /// <summary>
        /// Notifies the open camera.
        /// </summary>
        /// <param name="open">If set to <c>true</c> open.</param>
        public void NotifyOpenCamera(bool open)
        {
            OpenCamera?.Invoke(this, open);
        }

        /// <summary>
        /// Notifies the focus.
        /// </summary>
        /// <param name="touchPoint">Touch point.</param>
        public void NotifyFocus(Point touchPoint)
        {
            CamFocus?.Invoke(this, touchPoint);
        }

        /// <summary>
        /// Notifies the busy.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="busy">If set to <c>true</c> busy.</param>
        public void NotifyBusy(object sender, bool busy)
        {
            Busy?.Invoke(this, busy);
        }

        /// <summary>
        /// Notifies the orientation change.
        /// </summary>
        /// <param name="orientation">Orientation.</param>
        public void NotifyOrientationChange(Orientation orientation)
        {
            Orientation = orientation;

            OrientationChange?.Invoke(this, orientation);
        }

        /// <summary>
        /// Notifies the availability.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="isAvailable">If set to <c>true</c> is available.</param>
        public void NotifyAvailability(object sender, bool isAvailable)
        {
            CameraAvailable = isAvailable;

            AvailabilityChange?.Invoke(this, isAvailable);
        }

        /// <summary>
        /// Notifies the photo.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="imageData">Image data.</param>
        public void NotifyPhoto(object sender, byte[] imageData)
        {
            Photo?.Invoke(this, imageData);
        }

        public void NotifyPhoto(object sender, string filePath)
        {
            PhotoPath?.Invoke(this, filePath);
        }

        public void NotifyVideo(object sender, string videoFilePath)
        {
            Video?.Invoke(this, videoFilePath);
        }
        /// <summary>
        /// Notifies the flash.
        /// </summary>
        /// <param name="flashOn">If set to <c>true</c> flash on.</param>
        public void NotifyFlash(bool flashOn)
        {
            Flash?.Invoke(this, flashOn);
        }

        /// <summary>
        /// Notifies the torch.
        /// </summary>
        /// <param name="torchOn">If set to <c>true</c> torch on.</param>
        public void NotifyTorch(bool torchOn)
        {
            Torch?.Invoke(this, torchOn);
        }

        /// <summary>
        /// Notifies the loading.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="loading">If set to <c>true</c> loading.</param>
        public void NotifyLoading(object sender, bool loading)
        {
            Loading?.Invoke(this, loading);
        }

        /// <summary>
        /// Notifies the widths.
        /// </summary>
        /// <param name="cameraButtonContainerWidth">Camera button container width.</param>
        public void NotifyWidths(float cameraButtonContainerWidth)
        {
            CameraButtonContainerWidth = cameraButtonContainerWidth;

            Widths?.Invoke(this, cameraButtonContainerWidth);
        }

        #endregion

        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
            propertyName: "Camera",
            returnType: typeof(CameraOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: CameraOptions.Rear);

        public static readonly BindableProperty SpeedProperty = BindableProperty.Create(
            propertyName: "Speed",
            returnType: typeof(SpeedOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: SpeedOptions.Normal);

        public static readonly BindableProperty FileUrlProperty = BindableProperty.Create(
            propertyName: "FileUrl",
            returnType: typeof(string),
            declaringType: typeof(string),
            defaultValue: null);

        public static readonly BindableProperty ModeProperty = BindableProperty.Create(
            propertyName: "Mode",
            returnType: typeof(CameraModes),
            declaringType: typeof(CameraModes),
            defaultValue: CameraModes.Snapshot);


        public event EventHandler<ListEventArgs> SavedMovie;

        public void SavedMoview(String url, String errorMessage)
        {
            SavedMovie?.Invoke(this, new ListEventArgs(url, errorMessage));
        }

        public SpeedOptions Speed
        {
            get { return (SpeedOptions)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        public CameraOptions Camera
        {
            get { return (CameraOptions)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }
        public CameraModes Mode
        {
            get { return (CameraModes)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        public string FileUrl
        {
            get { return (string)GetValue(FileUrlProperty); }
            set { SetValue(FileUrlProperty, value); }
        }
    }

    public enum CameraOptions
    {
        Rear,
        Front
    }

    public enum SpeedOptions
    {
        Normal,
        SlowMo
    }

    public enum CameraModes
    {
        Snapshot,
        Video
    }

}
