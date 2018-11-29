using System;
using System.IO;
using System.Net;
using Foundation;
using ManageGo;
using ManageGo.iOS;
using UIKit;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace ManageGo.iOS
{
    public class CustomWebViewRenderer : ViewRenderer<CustomWebView, WKWebView>
    {
        WKWebView _wkWebView;
        protected override void OnElementChanged(ElementChangedEventArgs<CustomWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var config = new WKWebViewConfiguration();
                config.AllowsInlineMediaPlayback = true;
                config.Preferences.JavaScriptEnabled = true;
                _wkWebView = new WKWebView(Frame, config);
                SetNativeControl(_wkWebView);
            }
            if (e.OldElement != null)
            {
                // Cleanup
            }
            if (e.NewElement != null)
            {
                Control.SizeToFit();
                var customWebView = Element as CustomWebView;
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!string.IsNullOrWhiteSpace(customWebView.Uri))
                {
                    var fileUrl = new NSUrl(customWebView.Uri, false);
                    var docUrl = new NSUrl(documentsPath, true);
                    Control.LoadFileUrl(fileUrl, docUrl);
                }
                else if (!string.IsNullOrWhiteSpace(customWebView.Data))
                    Control.LoadHtmlString((NSString)customWebView.Data, NSUrl.FromString("https://localhost"));

            }
        }
    }
}
