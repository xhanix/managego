using Xamarin.Forms;
using PropertyChanged;
using System.Linq;
using System;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public partial class TicketDetailsPage : ContentPage
    {
        double pageHeight;
        public double PageWidth { get; private set; }
        public double PermittedEditorWidth { get; private set; }
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
            PermittedEditorWidth = PageWidth + 20;
        }



        protected override bool OnBackButtonPressed()
        {
            var model = (TicketDetailsPageModel)BindingContext;
            if (model.ShouldShowClock)
            {
                model.OnCloseTimePickerTapped.Execute(null);
                return true;
            }
            else if (model.WorkOrderActionSheetIsVisible || model.EventActionSheetIsVisible ||
                    model.ReplyBoxIsVisible)
            {
                model.OnCloseReplyBubbleTapped.Execute(null);
                ReplyEditor.Unfocus();
                return true;
            }
            else if (model.PopContentView != null)
            {
                model.OnHideDetailsTapped.Execute(null);
                return true;
            }
            else
            {
                base.OnBackButtonPressed();
                return false;
            }
        }

        void Handle_Focused(object sender, FocusEventArgs e)
        {
            ReplyBox.HeightRequest = pageHeight * 0.65;
            MyScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
            WasFocused = true;
        }


        async void Handle_Scrolled(object sender, ScrolledEventArgs e)
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


        void Handle_Tapped(object sender, System.EventArgs e)
        {
            ReplyEditor.Focus();
        }


        void Handle_Clicked(object sender, System.EventArgs e)
        {
            ((TicketDetailsPageModel)BindingContext).OnReplyLabelTapped.Execute(null);
            ReplyBox.HeightRequest = pageHeight * 0.65;
            MyScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
            WasFocused = true;
            ReplyEditor.Focus();
        }

        internal void ScrollToBottom(object item)
        {
            // MyListView.ScrollTo(item, ScrollToPosition.End, false);
        }

        internal void RedrawTable()
        {
            MyListView.HasUnevenRows = !MyListView.HasUnevenRows;
            MyListView.HasUnevenRows = !MyListView.HasUnevenRows;
        }
    }
}
