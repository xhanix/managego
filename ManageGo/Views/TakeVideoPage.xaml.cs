using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ManageGo
{
    public partial class TakeVideoPage : ContentPage
    {
        public TakeVideoPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            var isVisible = false;
            RecordIndicatorDot.Animate("recording", v =>
            {
                RecordIndicatorDot.IsVisible = isVisible;
            }, length: 350,
            finished: delegate (double d, bool b)
            {
                if (BindingContext != null)
                    RecordIndicatorDot.IsVisible = !isVisible && ((TakeVideoPageModel)this.BindingContext).IsRecordingVideo;
                else
                    RecordIndicatorDot.IsVisible = false;
            },
            repeat: () =>
            {
                if (BindingContext != null)
                    isVisible = !isVisible && ((TakeVideoPageModel)this.BindingContext).IsRecordingVideo;
                else
                {
                    isVisible = false;
                    return false;
                }

                return true;
            });


        }
    }
}
