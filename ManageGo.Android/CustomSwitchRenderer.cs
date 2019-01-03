using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using ManageGo.Controls;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomSwitch), typeof(CustomSwitchRenderer))]
namespace ManageGo.Droid
{
    public class CustomSwitchRenderer : SwitchRenderer
    {

        ColorStateList thumbStates = new ColorStateList(
            new int[][]{
                    new int[]{-Android.Resource.Attribute.StateEnabled},
                    new int[]{Android.Resource.Attribute.StateChecked},
                    new int[]{}
            },
            new int[]{
                   Android.Resource.Color.Black,
                   Android.Resource.Color.DarkerGray,
                   Android.Resource.Color.HoloBlueDark
        });

        ColorStateList trackStates = new ColorStateList(
            new int[][]{

                    new int[]{Android.Resource.Attribute.StateChecked},
                    new int[] { }
            },
            new int[]{
                    Android.Resource.Color.HoloGreenDark,
                    Android.Resource.Color.HoloRedDark
            }
    );

        public CustomSwitchRenderer(Context context) : base(context)
        {
        }




        protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null || e.NewElement == null)
                return;
            var view = (CustomSwitch)Element;
            Control.TrackTintList = trackStates;
            Element.Toggled += (_sender, _e) =>
            {
                Control.TrackTintList = trackStates;
                Control.TrackDrawable.ClearColorFilter();
                if (this.Control.Checked)
                {
                    this.Control.ThumbDrawable.SetColorFilter(Android.Graphics.Color.White, PorterDuff.Mode.SrcAtop);

                    Control.TrackDrawable.SetColorFilter(Android.Graphics.Color.DarkGreen, PorterDuff.Mode.Add);
                }
                else
                {
                    this.Control.ThumbDrawable.SetColorFilter(Android.Graphics.Color.White, PorterDuff.Mode.SrcAtop);

                    Control.TrackDrawable.SetColorFilter(Android.Graphics.Color.DarkRed, PorterDuff.Mode.SrcAtop);
                }
            };
            Control.TrackDrawable.ClearColorFilter();
            if (this.Control.Checked)
            {
                this.Control.ThumbDrawable.SetColorFilter(Android.Graphics.Color.White, PorterDuff.Mode.SrcAtop);

                Control.TrackDrawable.SetColorFilter(Android.Graphics.Color.DarkGreen, PorterDuff.Mode.Add);
            }
            else
            {
                this.Control.ThumbDrawable.SetColorFilter(Android.Graphics.Color.White, PorterDuff.Mode.SrcAtop);

                Control.TrackDrawable.SetColorFilter(Android.Graphics.Color.DarkRed, PorterDuff.Mode.SrcAtop);
            }

        }


    }
}
