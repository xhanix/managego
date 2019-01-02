using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreshMvvm;

namespace ManageGo
{
    internal class CalendarPageModel : BaseDetailPage
    {
        public List<DateTime> HighlightedDates { get; set; }
        public DateTime SelectedDate { get; set; }
        DateTime FetchEventsFromDate { get; set; }
        DateTime FetchEventsToDate { get; set; }

        public CalendarPageModel()
        {
            FetchEventsFromDate = DateTime.Today.AddDays(-30);
            FetchEventsToDate = DateTime.Today.AddDays(30);
            HighlightedDates = new List<DateTime>();
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
