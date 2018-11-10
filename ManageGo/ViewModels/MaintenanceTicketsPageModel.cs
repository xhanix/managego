using System;
using System.Threading.Tasks;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo
{
    internal class MaintenanceTicketsPageModel : BaseDetailPage
    {
        public View PopContentView { get; private set; }


        internal override Task LoadData()
        {
            //throw new NotImplementedException();
            return Task.FromResult<int>(0);
        }

        public FreshAwaitCommand OnCalendarButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    var cal = new Controls.CalendarView
                    {
                        SelectedDates = new CustomCalendar.DateRange(DateTime.Today),
                        HeightRequest = 275,
                        WidthRequest = 400,
                        HorizontalOptions = LayoutOptions.Center,
                        AllowMultipleSelection = true
                    };
                    PopContentView = cal;
                    /*
                     * SelectedDates="{Binding SelectedDates}"
                                               AllowMultipleSelection="true" 
                                               OnPresetRangeUpdate="Handle_OnPresetRangeUpdate"
                                               HeightRequest="275" 
                                               WidthRequest="400" 
                                               HorizontalOptions="Center"
                     * */
                    tcs?.SetResult(true);
                });
            }
        }
    }
}
