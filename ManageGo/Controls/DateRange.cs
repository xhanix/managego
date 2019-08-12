using System;
using System.Linq;
using System.Collections.Generic;

namespace CustomCalendar
{
    public delegate void CurrentMonthYearHandler(DateTime date);
    public delegate void DateRangeHandler(DateRange dates);
    [Serializable]
    public class DateRange
    {
        List<DateTime> _dates;
        List<DateTime> Dates
        {
            get
            {
                if (_dates == null)
                    _dates = new List<DateTime>();
                else
                    _dates = _dates.OrderBy(d => d.Date).ToList();

                return _dates;
            }
            set => _dates = value;
        }

        public DateTime StartDate
        {
            get => Dates[0].Date;
        }

        public DateTime? EndDate
        {
            get
            {
                if (Dates.Count > 1)
                    return Dates[1].Date;

                return null;
            }
        }

        public DateRange(DateTime startDate) : this(startDate, null)
        { }

        public DateRange(DateTime startDate, DateTime? endDate)
        {
            AddDate(startDate);

            if (endDate.HasValue)
                AddDate(endDate.Value);
        }

        public void SetStartDate(DateTime date)
        {
            Dates.Clear();
            Dates.Add(date);
        }

        public void AddDate(DateTime date)
        {
            if (Dates.Count == 2)
                Dates.Clear();

            Dates.Add(date);
        }

        public List<DateTime> GetDateRangeDates()
        {
            var dates = new List<DateTime>();

            if (EndDate.HasValue)
            {
                var date = StartDate;

                while (date <= EndDate.Value.Date)
                {
                    dates.Add(date.Date);
                    date = date.AddDays(1);
                }
            }
            else
            {
                dates.Add(StartDate);
            }

            return dates;
        }
    }
}