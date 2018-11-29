using System;
using Xamarin.Forms;
using FreshMvvm;

namespace ManageGo
{
    public class CameraPreview : View
    {
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
            SavedMovie(this, new ListEventArgs(url, errorMessage));
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
