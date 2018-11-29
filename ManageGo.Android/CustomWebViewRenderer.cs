using System;
using Android.Content;
using Android.Util;
using Android.Webkit;
using ManageGo;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace ManageGo.Droid
{

    public class CustomWebViewRenderer : ViewRenderer<CustomWebView, Android.Webkit.WebView>
    {
        Android.Webkit.WebView _wkWebView;
        Context _context;
        public CustomWebViewRenderer(Context context) : base(context)
        {
            this._context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CustomWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                // var config = new WKWebViewConfiguration();
                _wkWebView = new Android.Webkit.WebView(_context);
                _wkWebView.Settings.JavaScriptEnabled = true;
                _wkWebView.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                SetNativeControl(_wkWebView);
            }
            if (e.OldElement != null)
            {
                // Cleanup
            }
            if (e.NewElement != null)
            {
                var customWebView = Element as CustomWebView;
                Control.LoadUrl(customWebView.Uri);
                Control.SetFitsSystemWindows(true);
            }
        }
    }
}
