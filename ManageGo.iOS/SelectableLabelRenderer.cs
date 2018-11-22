using System;
using ManageGo.Controls;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SelectableLabel), typeof(SelectableLabelRenderer))]
namespace ManageGo.iOS
{
    public class SelectableLabelRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control == null) return;

            Control.Selectable = true;
            Control.Editable = false;
            Control.ScrollEnabled = false;
            Control.TextContainerInset = UIEdgeInsets.Zero;
            //Control.TextContainer.LineFragmentPadding = 0;
            Control.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;

            Control.BackgroundColor = new UIColor(0, 0);

        }
    }

}
