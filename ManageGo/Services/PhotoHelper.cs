using System;
using System.IO;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace ManageGo.Services
{
    public static class PhotoHelper
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



        public async static Task<string> AddNewPhoto(bool fromAlbum = true)
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
            else if (CrossMedia.Current.IsPickPhotoSupported)
            {
                photo = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    CustomPhotoSize = compression,
                    RotateImage = false,
                    SaveMetaData = false
                });
            }
            // convert stream to string
            if (photo != null)
            {

                var extension = Path.GetExtension(photo.Path);
                var imgName = Guid.NewGuid() + extension;
                using (MemoryStream ms = new MemoryStream())
                {
                    photo.GetStream().CopyTo(ms);

                    var array = ms.ToArray();
                    //var removeOrientation = DependencyService.Resolve<App.IMediaService>()
                    //    .SetOrientationUp(array);

                    return Convert.ToBase64String(array);

                    // var thumbnail = DependencyService.Resolve<App.IMediaService>()
                    //  .ResizeImage(removeOrientation, 100, 100);

                }
            }
            return null;
        }


    }
}
