using System;
using System.ComponentModel;
using Android.Content;
using ManageGo;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MGLabel), typeof(MGLabelRenderer))]
namespace ManageGo.Droid
{

    public class MGLabelRenderer : LabelRenderer
    {
        public MGLabelRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (Control != null && Element != null)
            {
                var lineSpacingLabel = (MGLabel)this.Element;
                var lineSpacing = lineSpacingLabel.LineSpacing;
                this.Control.SetLineSpacing(0f, 1.5f);
                this.UpdateLayout();
            }
        }

    }
}
