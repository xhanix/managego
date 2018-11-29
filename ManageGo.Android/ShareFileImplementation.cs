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
                    Console.WriteLine("Plugin.ShareFile: ShareLocalFile Warning: localFilePath null or empty");
                    return;
                }

                // if (!localFilePath.StartsWith("file://", StringComparison.CurrentCulture))
                //   localFilePath = string.Format("file://{0}", localFilePath);

                var fileUri = Android.Net.Uri.Parse(localFilePath);

                var intent = new Intent();
                intent.SetFlags(ActivityFlags.ClearTop);
                intent.SetFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionView);
                //intent.SetType("*/*");
                //intent.PutExtra(Intent.ExtraStream, fileUri);
                intent.PutExtra(Intent.ExtraText, title);
                //intent.PutExtra(Intent.ExtraSubject, string.Empty);
                intent.SetFlags(ActivityFlags.GrantReadUriPermission);
                Java.IO.File file = new Java.IO.File(localFilePath);
                file.SetReadable(true);
                var uri = FileProvider.GetUriForFile(CrossCurrentActivity.Current.AppContext, CrossCurrentActivity.Current.AppContext.PackageName + ".ManageGo.pcm", file);
                var type = "*/*";
                if (localFilePath.EndsWith("pdf", StringComparison.Ordinal) || localFilePath.EndsWith("ai", StringComparison.Ordinal))
                    type = "application/pdf";
                else if (localFilePath.EndsWith("jpg", StringComparison.Ordinal)
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
                else
                    intent.SetAction(Intent.ActionSend);
                intent.SetDataAndType(uri, type);
                var chooserIntent = Intent.CreateChooser(intent, title);
                chooserIntent.SetFlags(ActivityFlags.ClearTop);
                chooserIntent.SetFlags(ActivityFlags.NewTask);
                //intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(chooserIntent);
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
