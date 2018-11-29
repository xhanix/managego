using System;
using AVFoundation;
using Foundation;
using Xamarin.Essentials;

// Delegate for AVCaotureFileOutout

namespace ManageGo.iOS
{
    public partial class UICameraPreview
    {
        public class OutputRecorder : AVCaptureFileOutputRecordingDelegate
        {
            public event EventHandler<VideoSavedEventArgs> SavedMovie;

            public override void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError error)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (error != null)
                    {
                        //todo: log error
                    }
                    SavedMovie(this, new VideoSavedEventArgs { error = error });

                });

            }
        }
    }

    public class VideoSavedEventArgs : EventArgs
    {
        public NSError error;
    }
}
