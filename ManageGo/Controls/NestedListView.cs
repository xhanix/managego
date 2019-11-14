using System;
using Xamarin.Forms;

namespace ManageGo.Controls
{
    public class NestedListView : ListView
    {
        public NestedListView(ListViewCachingStrategy strategy) : base(strategy)
        {
            BackgroundColor = Color.White;
        }

        public NestedListView()
        {
            BackgroundColor = Color.White;
        }
    }
}
