using System;
using System.IO;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace ManageGo.Services
{
    public static partial class PhotoHelper
    {
        public static async Task<bool> HasPhotoPermissions()
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            //try getting permission if not already
            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage });
                cameraStatus = results[Permission.Camera];
                storageStatus = results[Permission.Storage];
            }
            //now return permission status
            return cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted;
        }

        public async static Task<(byte[], string)> PickPhotoAndVideos()
        {
            Tuple<Stream, string, MGFileType> result = await DependencyService.Get<IPicturePicker>().GetImageStreamAsync();
            if (result is null || result.Item1 is null || string.IsNullOrWhiteSpace(result.Item2))
                return (new byte[0], string.Empty);
            using (MemoryStream ms = new MemoryStream())
            {
                result.Item1.CopyTo(ms);
                var extension = Path.GetExtension(result.Item2);
                var fileName = result.Item3 == MGFileType.Photo ? "Photo_" : "Video_";
                fileName = fileName + $"{DateTime.Now.ToString("yyMMdd_hhmmss")}" + extension;
                var array = ms.ToArray();
                //var removeOrientation = DependencyService.Resolve<App.IMediaService>()
                //    .SetOrientationUp(array);

                return (array, fileName);//Convert.ToBase64String(array);

                // var thumbnail = DependencyService.Resolve<App.IMediaService>()
                //  .ResizeImage(removeOrientation, 100, 100);

            }
        }

        public async static Task<(byte[], string)> AddNewPhoto(bool fromAlbum = true)
        {

            var compression = 50;

            MediaFile photo = null;
            if (!fromAlbum && CrossMedia.Current.IsTakePhotoSupported)
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    AllowCropping = false,
                    DefaultCamera = CameraDevice.Rear,
                    RotateImage = false,
                    CustomPhotoSize = compression,
                    SaveMetaData = false
                });
            }
            else if (CrossMedia.Current.IsPickVideoSupported)
            {
                photo = await CrossMedia.Current.PickVideoAsync();
            }
            // convert stream to string
            if (photo != null)
            {

                var extension = Path.GetExtension(photo.Path);
                var imgName = Guid.NewGuid() + extension;
                using (MemoryStream ms = new MemoryStream())
                {
                    photo.GetStream().CopyTo(ms);
                    var fileName = Path.GetFileName(photo.Path);
                    var array = ms.ToArray();
                    //var removeOrientation = DependencyService.Resolve<App.IMediaService>()
                    //    .SetOrientationUp(array);

                    return (array, fileName);//Convert.ToBase64String(array);

                    // var thumbnail = DependencyService.Resolve<App.IMediaService>()
                    //  .ResizeImage(removeOrientation, 100, 100);

                }
            }
            return (null, "");


        }


    }
}
