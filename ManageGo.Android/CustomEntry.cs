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
        Drawable bgColor;

        public CustomEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null && Control != null)
            {
                bgColor = new ColorDrawable(Android.Graphics.Color.Transparent);
                Control?.SetBackground(bgColor);
                Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
                if (!Control.Enabled)
                    Control?.SetTextColor(Android.Graphics.Color.Gray);
            }

            if (e.OldElement != null)
            {
                bgColor.Dispose();
            }
        }
    }
}
