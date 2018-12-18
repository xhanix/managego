using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PicturePickerImplementation))]
namespace ManageGo.iOS
{
    public class PicturePickerImplementation : IPicturePicker
    {
        TaskCompletionSource<Tuple<System.IO.Stream, string, Services.MGFileType>> taskCompletionSource;
        UIImagePickerController imagePicker;

        public Task<Tuple<Stream, string, Services.MGFileType>> GetImageStreamAsync()
        {
            // Create and define UIImagePickerController
            imagePicker = new UIImagePickerController
            {
                SourceType = UIImagePickerControllerSourceType.SavedPhotosAlbum | UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary | UIImagePickerControllerSourceType.SavedPhotosAlbum)
            };

            // Set event handlers
            imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled += OnImagePickerCancelled;

            // Present UIImagePickerController;
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentViewController(imagePicker, true, null);

            // Return Task object
            taskCompletionSource = new TaskCompletionSource<Tuple<System.IO.Stream, string, Services.MGFileType>>();
            return taskCompletionSource.Task;
        }

        void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;

            if (image != null)
            {
                // Convert UIImage to .NET Stream object
                NSData data = image.AsJPEG(1);
                Stream stream = data.AsStream();
                UnregisterEventHandlers();
                // Set the Stream as the completion of the Task
                taskCompletionSource.SetResult(
                    new Tuple<Stream, string, Services.MGFileType>
                            (stream, args.ImageUrl.AbsoluteString, Services.MGFileType.Photo));
            }
            else if (args.MediaUrl != null)
            {
                NSData data = NSData.FromUrl(args.MediaUrl);
                Stream stream = data.AsStream();
                UnregisterEventHandlers();
                taskCompletionSource.SetResult(
                   new Tuple<Stream, string, Services.MGFileType>
                           (stream, args.MediaUrl.AbsoluteString, Services.MGFileType.Video));
            }
            else
            {
                UnregisterEventHandlers();
                taskCompletionSource.SetResult(null);
            }
            imagePicker.DismissViewController(true, null);
        }

        void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            taskCompletionSource.SetResult(null);
            imagePicker.DismissViewController(true, null);
        }

        void UnregisterEventHandlers()
        {
            imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            imagePicker.Canceled -= OnImagePickerCancelled;
        }
    }
}
