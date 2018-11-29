using System;
using Android.Content;
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
                _wkWebView.Settings.DomStorageEnabled = true;
                _wkWebView.Settings.SetAppCacheEnabled(true);
                _wkWebView.Settings.DatabaseEnabled = true;
                _wkWebView.Settings.DatabasePath = Context.FilesDir + "/databases";

                SetNativeControl(_wkWebView);
            }
            if (e.OldElement != null)
            {
                // Cleanup
            }
            if (e.NewElement != null)
            {
                var customWebView = Element as CustomWebView;
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!string.IsNullOrWhiteSpace(customWebView.Uri))
                {
                    string data = "<body>" + $"<img width=\"100%\" src=\"{customWebView.Uri}\"/></body>";
                    Control.LoadDataWithBaseURL("file:///android_asset/", data, "text/html", "utf-8", "");
                }
                else if (!string.IsNullOrWhiteSpace(customWebView.Data))
                {
                    Control.LoadDataWithBaseURL("file:///android_asset/", customWebView.Data, "text/html", "UTF-8", "");

                }



                Control.SetFitsSystemWindows(true);
            }
        }
    }
}
