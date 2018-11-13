using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo
{
    internal class MaintenanceTicketsPageModel : BaseDetailPage
    {
        DateRange dateRange;
        public View PopContentView { get; private set; }
        bool CalendarIsShown { get; set; }
        public List<MaintenanceTicket> FetchedTickets { get; private set; }
        public bool ListIsEnabled { get; set; } = false;

        DateRange DateRange
        {
            get
            {
                return dateRange is null ? new DateRange(DateTime.Today, DateTime.Today.AddDays(-7)) : dateRange;
            }
            set
            {
                dateRange = value;
            }
        }

        internal override async Task LoadData(bool refreshData = false)
        {
            //throw new NotImplementedException();
            if (FetchedTickets is null || refreshData)
            {
                Dictionary<string, string> filters = new Dictionary<string, string>
                {
                    { "DateFrom", this.DateRange.StartDate.ToLongDateString() },
                    { "DateTo", this.DateRange.EndDate.Value.ToLongDateString() },

                };
                FetchedTickets = await Services.DataAccess.GetTicketsAsync(filters);
                Console.WriteLine($"Tickets Fetched: {FetchedTickets.Count}");
            }
        }

        public FreshAwaitCommand OnCalendarButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (!CalendarIsShown)
                    {
                        StackLayout container = new StackLayout();
                        Grid buttonContainer = new Grid { Padding = new Thickness(8, 8, 8, 12) };
                        var cancelButton = new Button
                        {
                            Text = "CANCEL",
                            BackgroundColor = Color.Transparent,
                            HorizontalOptions = LayoutOptions.Start,
                            TextColor = Color.Gray
                        };
                        cancelButton.Clicked += (sender, e) => OnCalendarButtonTapped.Execute(null);
                        var applyButton = new Button
                        {
                            Text = "APPLY",
                            BackgroundColor = Color.Transparent,
                            HorizontalOptions = LayoutOptions.End,
                            TextColor = Color.Red
                        };

                        var cal = new Controls.CalendarView
                        {
                            SelectedDates = DateRange,
                            HeightRequest = 245,
                            WidthRequest = 400,
                            HorizontalOptions = LayoutOptions.Center,
                            AllowMultipleSelection = true
                        };
                        applyButton.Clicked += async (sender, e) =>
                        {
                            this.DateRange = cal.SelectedDates;
                            OnCalendarButtonTapped.Execute(null);
                            await LoadData(true);
                        };
                        buttonContainer.Children.Add(cancelButton);
                        buttonContainer.Children.Add(applyButton);
                        container.Children.Add(cal);
                        container.Children.Add(buttonContainer);
                        PopContentView = container;
                        ListIsEnabled = false;
                    }
                    else
                    {
                        PopContentView = null;
                        ListIsEnabled = true;
                    }
                    CalendarIsShown = !CalendarIsShown;
                    tcs?.SetResult(true);
                });
            }
        }
    }
}
