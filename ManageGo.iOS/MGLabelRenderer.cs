using System;
using System.ComponentModel;
using Foundation;
using ManageGo;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(MGLabel), typeof(MGLabelRenderer))]

namespace ManageGo.iOS
{
    public class MGLabelRenderer : LabelRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (Element != null && Control != null && !string.IsNullOrWhiteSpace(Element.Text))
            {
                var lineSpacingLabel = (MGLabel)this.Element;
                var paragraphStyle = new NSMutableParagraphStyle()
                {
                    LineSpacing = (nfloat)lineSpacingLabel.LineSpacing * 4
                };
                var _string = new NSMutableAttributedString(lineSpacingLabel.Text);
                var style = UIStringAttributeKey.ParagraphStyle;
                var range = new NSRange(0, _string.Length);

                _string.AddAttribute(style, paragraphStyle, range);
                this.Control.AttributedText = _string;
            }
        }
    }
}
