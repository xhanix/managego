using Android.Content;
using Android.Graphics.Drawables;
using ManageGo;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
namespace ManageGo
{
    public class CustomEntryRenderer : EntryRenderer
    {
        public CustomEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.Background = null;
            }

            Control?.SetBackground(new ColorDrawable(Android.Graphics.Color.Transparent));
            Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
        }
    }
}
