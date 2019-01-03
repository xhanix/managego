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
                    CalendarEvents = await Services.DataAccess.GetEventsForDate(SelectedDate);
                }
                else
                {
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
            }

        }

        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            HasLoaded = false;
            var dic = new Dictionary<string, object>
            {
                {"DateFrom", FetchEventsFromDate},
                {"DateTo", FetchEventsToDate}
            };
            try
            {
                HighlightedDates = await Services.DataAccess.GetEventsList(dic);
                if (HighlightedDates.Any(t => t.Date == SelectedDate.Date))
                {
                    CalendarEvents = await Services.DataAccess.GetEventsForDate(SelectedDate);
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
            }

        }
    }
}
