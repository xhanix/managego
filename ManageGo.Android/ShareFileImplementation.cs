using System;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using ManageGo.Droid;
using Xamarin.Forms;
using Android.Support.V4.Content;
using Plugin.CurrentActivity;

[assembly: Permission(Name = "android.permission.READ_EXTERNAL_STORAGE")]
[assembly: Permission(Name = "android.permission.WRITE_EXTERNAL_STORAGE")]
[assembly: Dependency(typeof(ShareFileImplementation))]
namespace ManageGo.Droid
{

    public class ShareFileImplementation : IShareFile
    {
        /// <summary>
        /// Simply share a local file on compatible services
        /// </summary>
        /// <param name="localFilePath">path to local file</param>
        /// <param name="title">Title of popup on share (not included in message)</param>
        /// <returns>awaitable Task</returns>
        public void ShareLocalFile(string localFilePath, string title = "", object view = null)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(localFilePath))
                {
                    Console.WriteLine("ShareFile: ShareLocalFile Warning: localFilePath null or empty");
                    return;
                }

                var fileName = System.IO.Path.GetFileName(localFilePath);
                Java.IO.File dir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
                Java.IO.File file = new Java.IO.File(dir, fileName);
                Android.Net.Uri path = FileProvider.GetUriForFile(CrossCurrentActivity.Current.AppContext, "com.ManageGo.ManageGo.ManageGo.pcm", file);
                file.SetReadable(true, false);
                var email = new Intent(Intent.ActionSend);
                email.PutExtra(Intent.ExtraEmail, new string[] { "" });
                email.PutExtra(Android.Content.Intent.ExtraCc, new string[] { "" });
                email.PutExtra(Intent.ExtraSubject, "Attached File");
                email.PutExtra(Intent.ExtraText, "See attached file");
                email.PutExtra(Intent.ExtraStream, path);

                email.SetFlags(ActivityFlags.NewTask);


                var type = "*/*";
                if (localFilePath.EndsWith("pdf", StringComparison.Ordinal) || localFilePath.EndsWith("ai", StringComparison.Ordinal))
                {
                    type = "application/pdf";
                    email.SetType("message/rfc822");
                    email.SetDataAndType(path, type);
                }
                else
                {
                    if (localFilePath.EndsWith("jpg", StringComparison.Ordinal)
                         || localFilePath.EndsWith("jpeg", StringComparison.Ordinal)
                         || localFilePath.EndsWith("png", StringComparison.Ordinal)
                         || localFilePath.EndsWith("tiff", StringComparison.Ordinal)
                         || localFilePath.EndsWith("gif", StringComparison.Ordinal)
                         || localFilePath.EndsWith("bmp", StringComparison.Ordinal))
                        type = "image/*";
                    else if (localFilePath.EndsWith("mp4", StringComparison.Ordinal) || localFilePath.EndsWith("mpeg", StringComparison.Ordinal) || localFilePath.EndsWith("mpg", StringComparison.Ordinal))
                        type = "video/mp4";
                    else if (localFilePath.EndsWith("wav", StringComparison.Ordinal))
                        type = "video/wav";
                    email.SetDataAndType(path, type);
                }

                //

                CrossCurrentActivity.Current.AppContext.StartActivity(email);

            }
            catch (Exception ex)
            {
                if (ex != null && !string.IsNullOrWhiteSpace(ex.Message))
                    Console.WriteLine("Exception in Plugin.ShareFile: ShareLocalFile Exception: {0}", ex);
            }
        }

        /// <summary>
        /// Simply share a file from a remote resource on compatible services
        /// </summary>
        /// <param name="fileUri">uri to external file</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="title">Title of popup on share (not included in message)</param>
        /// <returns>awaitable bool</returns>
        public async Task ShareRemoteFile(string fileUri, string fileName, string title = "", object view = null)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    var uri = new System.Uri(fileUri);
                    var bytes = await webClient.DownloadDataTaskAsync(uri);
                    var filePath = WriteFile(fileName, bytes);
                    ShareLocalFile(filePath, title);
                    //return true;
                }
            }
            catch (Exception ex)
            {
                if (ex != null && !string.IsNullOrWhiteSpace(ex.Message))
                    Console.WriteLine("Exception in Plugin.ShareFile: ShareRemoteFile Exception: {0}", ex.Message);
            }
        }

        public string GetPublicExternalFolderPath()
        {

            return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
        }

        /// <summary>
        /// Writes the file to local storage.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="fileName">File name.</param>
        /// <param name="bytes">Bytes.</param>
        private string WriteFile(string fileName, byte[] bytes)
        {
            string localPath = "";

            try
            {
                var localFolder = Android.App.Application.Context.ExternalCacheDir.AbsolutePath;
                localPath = System.IO.Path.Combine(localFolder, fileName);
                System.IO.File.WriteAllBytes(localPath, bytes); // write to local storage

                return localPath;
            }
            catch (Exception ex)
            {
                if (ex != null && !string.IsNullOrWhiteSpace(ex.Message))
                    Console.WriteLine("Exception in Plugin.ShareFile: ShareRemoteFile Exception: {0}", ex);
            }

            return localPath;
        }
    }

}
