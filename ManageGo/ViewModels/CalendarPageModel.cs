using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;

namespace ManageGo
{
    internal class CalendarPageModel : BaseDetailPage
    {
        private DateTime _selectedDate;
        event EventHandler SelectedDateChanged;
        public List<DateTime> HighlightedDates { get; set; }
        public List<Models.CalendarEvent> CalendarEvents { get; set; }
        public bool DateHasNoEvents { get; set; }
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                SelectedDateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        DateTime FetchEventsFromDate { get; set; }
        DateTime FetchEventsToDate { get; set; }

        public CalendarPageModel()
        {
            FetchEventsFromDate = DateTime.Today.AddDays(-60);
            FetchEventsToDate = DateTime.Today.AddDays(60);
            HighlightedDates = new List<DateTime>();
            SelectedDateChanged += CalendarSelectedDateChanged;
        }

        private async void CalendarSelectedDateChanged(object sender, EventArgs e)
        {
            try
            {
                if (HighlightedDates.Any(t => t.Date == SelectedDate.Date))
                {
                    var requestItem = new Models.CalendarEventRequestItem
                    {
                        DateFrom = SelectedDate,
                        DateTo = SelectedDate.AddDays(1)
                    };


                    CalendarEvents = await Services.DataAccess.GetEventsForDate(requestItem);
                    DateHasNoEvents = false;
                }
                else
                {
                    CalendarEvents?.Clear();
                    CalendarEvents = new List<Models.CalendarEvent>();
                    DateHasNoEvents = true;
                }
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                APIhasFailed = true;
            }
            finally
            {
                HasLoaded = true;
                var dic = new Models.EventsDatesRequestItem
                {
                    DateFrom = FetchEventsFromDate,
                    DateTo = FetchEventsToDate
                };
                HighlightedDates = (await Services.DataAccess.GetEventsList(dic)).Dates.ToList();
                ((CalendarPage)CurrentPage).DataLoaded();
            }

        }

        public FreshAwaitCommand OnViewTicketTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var calEvent = (Models.CalendarEvent)par;
                    var ticketId = calEvent.TicketID;
                    try
                    {
                        var ticketDetails = await Services.DataAccess.GetTicketDetails(ticketId);
                        var ticket = await Services.DataAccess.GetTicketsAsync(new TicketRequestItem
                        {
                            Ticket = ticketId
                        });

                        var dic = new Dictionary<string, object>
                            {
                            {"TicketDetails", ticketDetails},
                            {"TicketNumber", ticket.First()?.TicketNumber},
                            {"Address", ticket.First()?.Building?.BuildingShortAddress},
                            {"TicketTitleText", ticket.First()?.TicketSubject},
                            {"Ticket", ticket.FirstOrDefault()}
                            };

                        await CoreMethods.PushPageModel<TicketDetailsPageModel>(dic, false, false);
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong!", $"Unable to get ticket. {ex.Message}", "DISMISS");
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnShowDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var calEvent = (Models.CalendarEvent)par;
                    var alreadyExpandedEvent = CalendarEvents?.FirstOrDefault(t => t.DetailsShown && t.EventID != calEvent.EventID);
                    if (alreadyExpandedEvent != null)
                        alreadyExpandedEvent.DetailsShown = false;
                    calEvent.DetailsShown = !calEvent.DetailsShown;
                    tcs?.SetResult(true);
                });
            }
        }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            HasLoaded = false;
            var dic = new Models.EventsDatesRequestItem
            {
                DateFrom = FetchEventsFromDate,
                DateTo = FetchEventsToDate
            };
            try
            {
                HighlightedDates = (await Services.DataAccess.GetEventsList(dic)).Dates.ToList();
                if (HighlightedDates.Any(t => t.Date == SelectedDate.Date))
                {
                    var requestItem = new Models.CalendarEventRequestItem
                    {
                        DateFrom = SelectedDate,
                        DateTo = SelectedDate.AddDays(1)
                    };
                    CalendarEvents = await Services.DataAccess.GetEventsForDate(requestItem);
                }
                else
                {
                    DateHasNoEvents = true;
                }
            }
            catch (Exception)
            {
                APIhasFailed = true;
            }
            finally
            {
                HasLoaded = true;
                ((CalendarPage)CurrentPage).DataLoaded();
            }

        }
    }
}
