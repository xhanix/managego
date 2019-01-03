using System;
using ManageGo.Controls;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(CustomSwitch), typeof(CustomSwitchRenderer))]
namespace ManageGo.iOS
{
    public class CustomSwitchRenderer : SwitchRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null || e.NewElement == null)
                return;
            var view = (CustomSwitch)Element;
            if (!string.IsNullOrEmpty(view.SwitchThumbImage))
            {
                Control.OnImage = UIImage.FromFile(view.SwitchThumbImage.ToString());
                Control.OffImage = UIImage.FromFile(view.SwitchThumbImage.ToString());
            }
            //The color used to tint the appearance of the thumb.
            Control.ThumbTintColor = view.SwitchThumbColor.ToUIColor();
            Control.BackgroundColor = UIColor.Red;
            Control.Layer.CornerRadius = 16.0f;
            //The color used to tint the appearance of the switch when it is turned on.
            Control.OnTintColor = view.SwitchOnColor.ToUIColor();
            //The color used to tint the outline of the switch when it is turned off.
            Control.TintColor = view.SwitchOffColor.ToUIColor();
        }
    }
}
