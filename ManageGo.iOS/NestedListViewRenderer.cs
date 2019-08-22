using System;
using ManageGo.Controls;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(CustomListView), typeof(CustomListViewRenderer))]
namespace ManageGo.iOS
{
    public class CustomListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null && Control != null)
            {
                Control.ShowsVerticalScrollIndicator = false;
                Control.DecelerationRate = UIScrollView.DecelerationRateFast;
            }
        }
    }
}