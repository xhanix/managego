using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace CustomCalendar
{
    public enum HighlightType
    {
        Dark,
        Light,
        Enabled,
        Disabled
    }

    public class HighlightedDay
    {
        public HighlightType Type { get; set; }
        public int Day { get; set; }
    }

    public class CalendarMonthModel
    {
        private IEnumerable<HighlightedDay> _highlightedDays;

        public int Year { get; private set; }

        public int Month { get; private set; }

        public IEnumerable<CalendarDayModel> Days { get; private set; }

        public int GridSize { get; private set; }

        public IEnumerable<HighlightedDay> HighlightedDays
        {
            get => _highlightedDays;
            private set => _highlightedDays = value;
        }

        CalendarMonthModel(int year, int month, IEnumerable<CalendarDayModel> calendarDays, int gridSize, IEnumerable<HighlightedDay> highlightedDays)
        {
            Year = year;
            Month = month;
            Days = calendarDays;
            GridSize = gridSize;
            HighlightedDays = highlightedDays;
        }

        public CalendarDayModel TryFindCalendarDayByPoint(SKPoint point)
        {
            return this.Days.FirstOrDefault(day => day.Rectangle.Contains(point));
        }

        public static CalendarMonthModel Create(int year, int month, IEnumerable<HighlightedDay> highlightedDays, int width, int height)
        {
            var monthDate = new DateTime(year, month, 1);

            var calendarDays = new List<CalendarDayModel>();

            int dayOfWeek = int.Parse(monthDate.DayOfWeek.ToString("D"));

            var rows = Math.Ceiling((DateTime.DaysInMonth(year, month) + dayOfWeek) / 7f) + 1;

            var columns = 7;

            var date = monthDate.AddDays(0 - dayOfWeek);

            var gridSize = 0;
            int offset_x = 0;
            int offset_y = 0;

            if (width > height)
            {
                //gridSize = height / columns;

                gridSize = (int)((.75 * width) / columns);

                //var totalMargin = (width - (columns ))

                offset_x = (width - (columns * gridSize)) / 2;
            }
            else
            {
                gridSize = width / columns;
                offset_y = (height - (columns * gridSize)) / 2;
            }

            string[] days = { "S", "M", "T", "W", "T", "F", "S" };

            for (int i = 0; i < days.Length; i++)
            {
                var x = (float)((gridSize * i)) + offset_x;
                var y = 0f;

                calendarDays.Add(new CalendarDayModel(days[i], x, y, gridSize, gridSize));
            }

            for (int i = 1; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var x = (float)((gridSize * j)) + offset_x;
                    var y = (float)((((gridSize / 1.25)) * i)) + offset_y;

                    calendarDays.Add(new CalendarDayModel(date.Year, date.Month, date.Day, x, y, gridSize, gridSize));

                    date = date.AddDays(1);
                }
            }

            return new CalendarMonthModel(year, month, calendarDays, gridSize, highlightedDays);
        }
    }
}
