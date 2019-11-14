using System;
using Xamarin.Forms;

namespace ManageGo.Controls
{
    public class CustomListView : ListView
    {
        public CustomListView(ListViewCachingStrategy strategy) : base(strategy)
        {
            base.BackgroundColor = Color.White;
        }

        public CustomListView()
        {

        }
    }
}
