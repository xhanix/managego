using FreshMvvm;
using MGDataAccessLibrary.Models.Amenities.Responses;
using System;
using System.Threading.Tasks;
using PropertyChanged;

namespace ManageGo
{
    internal class BookingDetailPageModel : BaseDetailPage
    {
        [AlsoNotifyFor("BookingIdString", "Address")]
        public Booking Booking { get; private set; }
        public string BookingIdString => "Booking #" + Booking?.AmenityBookingId.ToString();
        public string Address => Booking?.DisplayAddress;
        public string Subtitle { get; set; }
        public string BookingDate { get; set; }
        public string BookingTimeWindiw { get; set; }
        public string PayStatus { get; set; }
        public string Notes { get; set; }
        public string Note { get; set; }
        public string BookingStatus { get; set; }
        public bool ButtonsVisible { get; set; }
        public bool FeeVisible { get; set; }
        public bool DepositVisible { get; set; }
        public bool PopupIsVisible { get; set; }


        public override void Init(object initData)
        {
            base.Init(initData);

            if (initData is Booking booking)
            {
                Booking = booking;
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            HamburgerIsVisible = false;
            BookingStatus = Booking.Status.ToString();
            ButtonsVisible = Booking.CanBeApproved || Booking.CanBeDeclined;
            // The dates in booking have unspecified kind. Need to get and apply offset before comparing.
            int offsetHours = Xamarin.Essentials.Preferences.Get("time_offset", 0);
            TimeSpan timeSpan = new TimeSpan(offsetHours, 0, 0);

            var normalizedtime = new DateTimeOffset(Booking.ToDate, timeSpan);
            if (normalizedtime < DateTimeOffset.Now)
                BookingStatus = $"Passed (Was {BookingStatus})";

            FeeVisible = Booking.IsBookingFeeEnabled;
            DepositVisible = Booking.IsSecurityDepositEnabled;
            Subtitle = "Created by: " + (Booking?.AuditLog?.Creator?.User?.Role?.ToLower() == "tenant" ? "Tenant" : Booking?.AuditLog?.Creator?.User?.Name) + " on " + Booking?.AuditLog?.Creator?.Date.ToString("d/M/yy") + " at " + Booking?.AuditLog?.Creator?.Date.ToString("h:mm tt");
            BookingDate = Booking.FromDate.ToString("ddd, MMM d, yyyy");
            BookingTimeWindiw = Booking.FromDate.ToString("h:mm tt") + " to " + Booking.ToDate.ToString("h:mm tt");


            if (Booking.PaymentStatus == MGDataAccessLibrary.Models.Amenities.Requests.PaymentStatus.NotApplicable)
                PayStatus = "(Not Applicable)";
            else
                PayStatus = $"({Booking.PaymentStatus.ToString()})";
        }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {

        }

        public FreshAwaitCommand OnApproveButtonTapped => new FreshAwaitCommand(async (tcs) =>
        {
            try
            {
                //approve the bookig. show confirmation. pop back to prev page.
                await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.SetBookingStatus(Booking.AmenityBookingId, Note, MGDataAccessLibrary.Models.Amenities.Responses.BookingStatus.Approved);
                await CoreMethods.DisplayAlert("Success!", "Booking approved", "Ok");
                await CoreMethods.PopPageModel();
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "Dismiss");
            }
            tcs?.SetResult(true);
        }, () => Booking.CanBeApproved);

        public FreshAwaitCommand OnDeclineButtonTapped => new FreshAwaitCommand(async (tcs) =>
        {
            try
            {
                //decline the bookig. show confirmation. pop back to prev page.
                await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.SetBookingStatus(Booking.AmenityBookingId, Note, MGDataAccessLibrary.Models.Amenities.Responses.BookingStatus.Declined);
                await CoreMethods.DisplayAlert("ManageGo", "Booking declined", "Ok");
                await CoreMethods.PopPageModel();
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "Dismiss");
            }
            tcs?.SetResult(true);
        }, () => Booking.CanBeDeclined);

        public FreshAwaitCommand OnBackButtonTapped => new FreshAwaitCommand(async (tcs) =>
        {
            await CoreMethods.PopPageModel();
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnViewGuestListTapped => new FreshAwaitCommand((tcs) =>
        {
            PopupIsVisible = true;
            tcs?.SetResult(true);
        }, () => Booking != null && !string.IsNullOrWhiteSpace(Booking.GuestList));

        public FreshAwaitCommand OnClosePopupTapped => new FreshAwaitCommand((tcs) =>
        {
            PopupIsVisible = false;
            tcs?.SetResult(true);
        });
    }
}