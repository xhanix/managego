using System;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using ManageGo.Controls;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.Views.View;

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
            if (e.NewElement != null && Control != null)
            {

                Control.NestedScrollingEnabled = true;
                Control.VerticalScrollBarEnabled = false;
                Control.SetFriction(ViewConfiguration.ScrollFriction * 1.5f);

            }
        }


    }
}

