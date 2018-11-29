using System;
using Xamarin.Forms;

namespace ManageGo
{
    public class CustomWebView : WebView
    {
        public static readonly BindableProperty UriProperty = BindableProperty.Create(propertyName: "Uri",
            returnType: typeof(string),
            declaringType: typeof(CustomWebView),
            defaultValue: default(string));

        public static readonly BindableProperty DataProperty = BindableProperty.Create(propertyName: "Data",
            returnType: typeof(string),
            declaringType: typeof(CustomWebView),
            defaultValue: default(string));

        public static readonly BindableProperty MimeProperty = BindableProperty.Create(propertyName: "Mime",
            returnType: typeof(string),
            declaringType: typeof(CustomWebView),
            defaultValue: default(string));


        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        public string Mime
        {
            get { return (string)GetValue(MimeProperty); }
            set { SetValue(MimeProperty, value); }
        }

        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }


    }
}
