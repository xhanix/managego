using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
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
                    new int[]{-Android.Resource.Attribute.StateEnabled},
                    new int[]{Android.Resource.Attribute.StateChecked},
                    new int[] { }
            },
            new int[]{
                    Android.Resource.Color.HoloGreenDark,
                    Android.Resource.Color.HoloGreenDark,
                    Android.Resource.Color.HoloRedDark
            }
    );

        public CustomSwitchRenderer(Context context) : base(context)
        {
        }




        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement is null || Control is null)
                return;
            var view = (CustomSwitch)e.NewElement;

            Control.SetTrackResource(Resource.Drawable.switch_track_custom);
            Control.SetOnCheckedChangeListener(new CheckedChangedListener(e.NewElement));
            Control.ThumbDrawable.SetColorFilter(Android.Graphics.Color.White, PorterDuff.Mode.SrcAtop);
            if (Control.Checked)
                Control.TrackDrawable.SetColorFilter(new Android.Graphics.Color(37, 206, 4), PorterDuff.Mode.SrcOver);
            else
                Control.TrackDrawable.SetColorFilter(new Android.Graphics.Color(201, 36, 4), PorterDuff.Mode.SrcOver);
        }
    }

    public class CheckedChangedListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
    {
        Xamarin.Forms.Switch _owner;

        public CheckedChangedListener(Xamarin.Forms.Switch owner)
        {
            _owner = owner;
        }
        public void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
        {
            this._owner.IsToggled = isChecked;
            var control = (Android.Widget.Switch)buttonView;
            //  buttonView.SetOutlineSpotShadowColor(Android.Graphics.Color.Red);
            // ((Android.Widget.Switch)buttonView).TrackDrawable.SetTint(Android.Resource.Color.Black);
            if (isChecked)
                control.TrackDrawable.SetColorFilter(new Android.Graphics.Color(37, 206, 4), PorterDuff.Mode.SrcOver);
            else
                control.TrackDrawable.SetColorFilter(new Android.Graphics.Color(201, 36, 4), PorterDuff.Mode.SrcOver);
        }
    }

}
