using System;
using Android.Content;
using Android.Views;
using ManageGo.Controls;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(NestedListView), typeof(NestedListViewRenderer))]
namespace ManageGo.Droid
{
    public class NestedListViewRenderer : ListViewRenderer
    {
        public NestedListViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                var listView = this.Control as Android.Widget.ListView;
                listView.NestedScrollingEnabled = true;
                listView.VerticalScrollBarEnabled = false;
                listView.SetFriction(ViewConfiguration.ScrollFriction * 2);
            }
        }

    }
}

