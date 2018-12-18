using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using ManageGo.Droid;
using ManageGo.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PicturePickerImplementation))]
namespace ManageGo.Droid
{
    public class PicturePickerImplementation : IPicturePicker
    {


        public Task<Tuple<Stream, string, MGFileType>> GetImageStreamAsync()
        {
            // Define the Intent for getting images
            Intent intent = new Intent(Intent.ActionPick);
            intent.SetType("*/*");
            intent.PutExtra(Intent.ExtraMimeTypes, new String[] { "image/*", "video/*" });
            intent.SetAction(Intent.ActionGetContent);

            // Start the picture-picker activity (resumes in MainActivity.cs)
            MainActivity.Current.StartActivityForResult(
                Intent.CreateChooser(intent, "Select Picture"),
                MainActivity.PickImageId);

            // Save the TaskCompletionSource object as a MainActivity property
            MainActivity.Current.PickMediaTaskCompletionSource =
                    new TaskCompletionSource<Tuple<Stream, string, MGFileType>>();

            // Return Task object
            return MainActivity.Current.PickMediaTaskCompletionSource.Task;
        }
    }
}
