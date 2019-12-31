using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace CustomCalendar.iOS
{
    public class CalendarView : UIView
    {
        public event CurrentMonthYearHandler OnCurrentMonthYearChange;
        public event DateRangeHandler OnSelectedDatesChange;

        internal int CurrentIndex { get; set; }
        internal bool AllowMultipleSelection { get; set; }
        internal DateTime Month { get; set; }

        DateRange _selectedDates;
        internal bool showDisabledDays;

        public DateRange SelectedDates
        {
            get
            {
                if (_selectedDates == null)
                    _selectedDates = new DateRange(DateTime.Now.Date);
                return _selectedDates;
            }
            set => _selectedDates = value;
        }

        public List<DateTime> HighlightedDates { get; set; }
        public IEnumerable<DateTime> AvailableDays { get; private set; }

        InfiniteScrollView<CalendarViewCell> infiniteScrollView;

        public CalendarView(CGRect frame, bool allowMultipleSelection,
             IEnumerable<DateTime> availableDays,
                            DateRange selectedDates = null,
                            List<DateTime> highlightedDates = null) : base(frame)
        {
            InitViews(frame);
            AllowMultipleSelection = allowMultipleSelection;

            SelectedDates = selectedDates ?? new DateRange(DateTime.Now);

            HighlightedDates = highlightedDates;
            AvailableDays = availableDays;

            SetCurrentlyTargetedMonth();

        }

        void InitViews(CGRect frame)
        {
            var del = new CalendarViewDelegate(new WeakReference<CalendarView>(this));

            infiniteScrollView = new InfiniteScrollView<CalendarViewCell>(del, frame);

            AddSubview(infiniteScrollView);
        }


        void SetCurrentlyTargetedMonth()
        {
            if (SelectedDates.EndDate.HasValue)
            {
                Month = SelectedDates.EndDate.Value;
            }
            else
            {
                Month = SelectedDates.StartDate;
            }
        }

        internal void GoToNextMonth(object sender, EventArgs e)
        {
            Month = Month.AddMonths(1);
            infiniteScrollView.ReloadData();
        }

        internal void GoToPreviousMonth(object sender, EventArgs e)
        {
            Month = Month.AddMonths(-1);
            infiniteScrollView.ReloadData();
        }

        public void UpdateSelectedDates(DateRange dates)
        {
            SelectedDates = dates;
            infiniteScrollView.ReloadData();
        }

        public void UpdateHighlatedDates(List<DateTime> dates)
        {
            HighlightedDates = dates;
            infiniteScrollView.ReloadData();
        }

        internal void UpdateEnabledDates(IEnumerable<DateTime> days)
        {
            AvailableDays = days;
            infiniteScrollView.ReloadData();
        }




        class CalendarViewDelegate : IInfiniteScrollViewDelegate<CalendarViewCell>
        {
            WeakReference<CalendarView> _weakView;

            public CalendarViewDelegate(WeakReference<CalendarView> weakView)
            {
                _weakView = weakView;
            }

            public void InitializeCell(InfiniteScrollView<CalendarViewCell> infiniteScrollView, CalendarViewCell cell, int index)
            {
                cell.ControlDelegate.DatesInteracted += selectedDate =>
                {
                    if (_weakView.TryGetTarget(out CalendarView v))
                    {
                        if (v.AllowMultipleSelection)
                            v.SelectedDates.AddDate(selectedDate);
                        else
                            v.SelectedDates.SetStartDate(selectedDate);

                        cell.ControlDelegate.AllowMultipleSelection = v.AllowMultipleSelection;
                        cell.ControlDelegate.SelectedDates = v.SelectedDates;
                        cell.ControlDelegate.HighlightedDates = v.HighlightedDates;
                        cell.ControlDelegate.AvailableDays = v.AvailableDays;
                        cell.ControlDelegate.ShowDisabledDays = v.showDisabledDays;
                        v.OnSelectedDatesChange?.Invoke(v.SelectedDates);

                        cell.SetNeedsDisplay();
                    }
                };
            }

            public void UpdateCell(InfiniteScrollView<CalendarViewCell> infiniteScrollView, CalendarViewCell cell, int index)
            {
                if (_weakView.TryGetTarget(out CalendarView view))
                {
                    if (infiniteScrollView.CurrentIndex < index) // right
                    {
                        cell.ControlDelegate.Date = view.Month.AddMonths(1);
                    }
                    else if (infiniteScrollView.CurrentIndex == index) // middle
                    {
                        cell.ControlDelegate.Date = view.Month;
                        view.OnCurrentMonthYearChange?.Invoke(view.Month);
                    }
                    else // left
                    {
                        cell.ControlDelegate.Date = view.Month.AddMonths(-1);
                    }

                    cell.ControlDelegate.AllowMultipleSelection = view.AllowMultipleSelection;
                    cell.ControlDelegate.SelectedDates = view.SelectedDates;
                    cell.ControlDelegate.HighlightedDates = view.HighlightedDates;
                    cell.ControlDelegate.AvailableDays = view.AvailableDays;
                    cell.ControlDelegate.ShowDisabledDays = view.showDisabledDays;
                    cell.SetNeedsDisplay();
                }
            }

            public void OnCurrentIndexChanged(InfiniteScrollView<CalendarViewCell> infiniteScrollView, int currentIndex)
            {
                if (_weakView.TryGetTarget(out CalendarView view))
                {
                    if (view.CurrentIndex > infiniteScrollView.CurrentIndex) // left
                    {
                        view.Month = view.Month.AddMonths(-1);
                    }
                    else if (view.CurrentIndex < infiniteScrollView.CurrentIndex) // right
                    {
                        view.Month = view.Month.AddMonths(1);
                    }
                    view.CurrentIndex = infiniteScrollView.CurrentIndex;
                }
            }


        }
    }
}
