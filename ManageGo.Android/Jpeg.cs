using System;
using Android.Content;
using ManageGo;
using ManageGo.Droid;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(CameraPreview), typeof(CameraPreviewRenderer))]
namespace ManageGo.Droid
{
    public class Jpeg : Java.Lang.Object, Android.Hardware.Camera.IPictureCallback
    {
        Context _context;
        public event EventHandler<ListEventArgs> SavedMovie;

        public Jpeg(Context cont)
        {
            _context = cont;
        }

        public void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            camera.StopPreview();
            //write data to file
            string localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var localPath = System.IO.Path.Combine(localFolder, $"Photo_{DateTime.Now.ToString("yyMMdd-hhmmss")}.jpg");
            string errMsg = null;
            try
            {
                System.IO.File.WriteAllBytes(localPath, data); // write to local storage
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            finally
            {
                SavedMovie?.Invoke(this, new ListEventArgs(localPath, errMsg));
            }
        }


    }
}
