using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using ManageGo;
using ManageGo.Controls;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomPicker), typeof(CustomPickerRenderer))]
namespace ManageGo.Droid
{

    public class CustomPickerRenderer : PickerRenderer
    {
        Drawable bgColor;

        public CustomPickerRenderer(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                if (Control != null)
                {
                    bgColor = new ColorDrawable(Android.Graphics.Color.Transparent);
                    Control?.SetBackground(bgColor);
                    Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    var shape = new ShapeDrawable(new Android.Graphics.Drawables.Shapes.RectShape());
                    shape.Paint.Color = Xamarin.Forms.Color.LightGray.ToAndroid();
                    shape.Paint.SetStyle(Paint.Style.Stroke);
                    Control.Background = shape;
                }
            }
            if (e.OldElement != null)
            {
                bgColor.Dispose();
            }
        }
    }
}
