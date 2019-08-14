using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//taken from http://jonathanpeppers.com/Blog/xamarin-forms-performance-on-android

[assembly: ExportImageSourceHandler(typeof(FileImageSource), typeof(xfperf.FileImageSourceHandler))]

namespace xfperf
{
    public class FileImageSourceHandler : IImageSourceHandler
    {
        public Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
        {
            return Task.FromResult<Bitmap>(null);
        }
    }
}