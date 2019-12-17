using Android.Content;
using Android.Graphics;
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
                if (e.NewElement is CustomEntry ent && ent.HasBorder)
                {
                    var shape = new ShapeDrawable(new Android.Graphics.Drawables.Shapes.RectShape());
                    shape.Paint.Color = Xamarin.Forms.Color.LightGray.ToAndroid();
                    shape.Paint.SetStyle(Paint.Style.Stroke);
                    Control.Background = shape;
                }
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
