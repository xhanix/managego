using System;
using Android.Content;
using Android.Views;
using ManageGo.Controls;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomListView), typeof(CustomListViewRenderer))]
namespace ManageGo.Droid
{
    public class CustomListViewRenderer : ListViewRenderer
    {
        public CustomListViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null && Control != null)
            {
                Control.NestedScrollingEnabled = false;
                Control.VerticalScrollBarEnabled = false;
                Control.SetFriction(ViewConfiguration.ScrollFriction * 1.5f);
            }
        }

    }
}

