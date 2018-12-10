using Xamarin.Forms;
using PropertyChanged;
using System.Linq;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public partial class TicketDetailsPage : ContentPage
    {
        double pageHeight;
        public double PageWidth { get; private set; }
        bool WasFocused { get; set; }
        readonly double ReplyBoxHeigh = 250;
        public TicketDetailsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ReplyBox.HeightRequest = ReplyBoxHeigh;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            pageHeight = height;
            PageWidth = width * 0.7;
        }



        void Handle_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            ReplyBox.HeightRequest = pageHeight * 0.65;
            MyScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
            WasFocused = true;
        }


        async void Handle_Scrolled(object sender, Xamarin.Forms.ScrolledEventArgs e)
        {
            if ((WasFocused && e.ScrollY > 12) || (string.IsNullOrWhiteSpace(ReplyEditor.Text) && e.ScrollY > 12))
            {
                await MyScrollView.ScrollToAsync(0, 0, true);
                WasFocused = false;
            }
        }

        void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {
            var c = (Comments)e.Item;
            if (c.Files != null)
            {
                var f = c.Files.ToList();
                c.Files = new System.Collections.ObjectModel.ObservableCollection<File>(f);
            }
        }

        void Handle_Unfocused(object sender, FocusEventArgs e)
        {
            ReplyBox.HeightRequest = ReplyBoxHeigh;
            //MyNavBar.IsVisible = true;
            //TopDetailsView.IsVisible = true;
        }
    }
}
