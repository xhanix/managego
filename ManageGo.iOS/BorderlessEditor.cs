using System;
using System.Drawing;
using ManageGo.Controls;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessEditor), typeof(BorderlessEditorRenderer))]
namespace ManageGo.iOS
{
    public class BorderlessEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control == null) return;


            Control.BackgroundColor = new UIColor(0, 0);
            Control.Layer.BorderColor = new CoreGraphics.CGColor(0, 0);

        }
    }

}
