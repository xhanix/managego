using Android.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace CustomCalendar.Droid
{
    public class CalendarViewPage : Android.Support.V4.View.ViewPager
    {
        public event CurrentMonthYearHandler OnCurrentMonthYearChange;
        public event DateRangeHandler OnSelectedDatesChange;

        public bool AllowMultipleSelection { get; set; }

        DateRange _selectedDates;
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

        bool IsDragging { get; set; }

        DateTime NextMonth { get; set; }
        DateTime CurrentMonth { get; set; }
        DateTime PreviousMonth { get; set; }

        public DrawableControlView<CalendarMonthControl> Item0 { get; set; }
        public DrawableControlView<CalendarMonthControl> Item1 { get; set; }
        public DrawableControlView<CalendarMonthControl> Item2 { get; set; }

        public CalendarViewPage(Android.Content.Context context, bool allowMultipleSelection, DateRange selectedDates, List<DateTime> highlightedDates) : base(context)
        {
            AllowMultipleSelection = allowMultipleSelection;

            Adapter = new CalendarPageAdapter(this);

            AddOnPageChangeListener(new OnPageChangeListener(this));

            Item0 = new DrawableControlView<CalendarMonthControl>(context, new CalendarMonthControl());
            Item1 = new DrawableControlView<CalendarMonthControl>(context, new CalendarMonthControl());
            Item2 = new DrawableControlView<CalendarMonthControl>(context, new CalendarMonthControl());

            Item1.ControlDelegate.DatesInteracted += UpdateSelectedDate;

            if (SelectedDates.EndDate.HasValue)
            {
                SetMonth(SelectedDates.EndDate.Value);
            }
            else
            {
                SetMonth(SelectedDates.StartDate);
            }

            SelectedDates = selectedDates;
            HighlightedDates = highlightedDates;

            UpdateCalendars();
        }

        void UpdateCalendars()
        {
            Item0.ControlDelegate.HighlightedDates = HighlightedDates;
            Item1.ControlDelegate.HighlightedDates = HighlightedDates;
            Item2.ControlDelegate.HighlightedDates = HighlightedDates;

            Item0.ControlDelegate.SelectedDates = SelectedDates;
            Item1.ControlDelegate.SelectedDates = SelectedDates;
            Item2.ControlDelegate.SelectedDates = SelectedDates;

            Item0.Invalidate();
            Item1.Invalidate();
            Item2.Invalidate();
        }

        public void UpdateSelectedDates(DateRange dates)
        {
            SelectedDates = dates;
            Invalidate();
        }

        public void UpdateHighlightedDates(List<DateTime> dates)
        {
            HighlightedDates = dates;
            Invalidate();
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            // We need to reset the current item when the user is about to start dragging.
            if (e.Action == MotionEventActions.Down)
            {
                var pager = this;

                if (pager.CurrentItem == 0)
                {
                    pager.SetCurrentItem(1, false);
                    pager.SetMonth(pager.PreviousMonth);
                    Invalidate();
                }
                else if (pager.CurrentItem == 2)
                {
                    pager.SetCurrentItem(1, false);
                    pager.SetMonth(pager.NextMonth);
                    Invalidate();
                }
            }

            if (e.Action == MotionEventActions.Up && !IsDragging)
            {
                var x = e.GetX();
                var y = e.GetY();
                var points = new SKPoint[] { new SKPoint(x, y) };

                Item1.ControlDelegate.EndInteractions(points);
            }

            return base.DispatchTouchEvent(e);
        }

        void UpdateSelectedDate(DateTime selectedDate)
        {
            if (SelectedDates == null || !AllowMultipleSelection)
            {
                SelectedDates = new DateRange(selectedDate);
            }
            else if (AllowMultipleSelection)
            {
                SelectedDates.AddDate(selectedDate);
            }

            UpdateSelectedDates();
        }

        void UpdateSelectedDates()
        {
            UpdateCalendars();

            OnSelectedDatesChange?.Invoke(SelectedDates);
        }

        void SetMonth(DateTime dateTime)
        {
            var monthDate = dateTime.Date;

            NextMonth = monthDate.AddMonths(1);
            CurrentMonth = monthDate;
            PreviousMonth = monthDate.AddMonths(-1);

            Item0.ControlDelegate.Date = PreviousMonth;
            Item1.ControlDelegate.Date = CurrentMonth;
            Item2.ControlDelegate.Date = NextMonth;

            OnCurrentMonthYearChange?.Invoke(new DateTime(monthDate.Year, monthDate.Month, 1));
        }

        class OnPageChangeListener : Java.Lang.Object, Android.Support.V4.View.ViewPager.IOnPageChangeListener
        {
            WeakReference<CalendarViewPage> _weakPager;

            bool _isInitialized;
            float _previousScrollX;

            public OnPageChangeListener(CalendarViewPage pager)
            {
                _weakPager = new WeakReference<CalendarViewPage>(pager);
            }

            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                if (_weakPager.TryGetTarget(out CalendarViewPage pager))
                {
                    var shouldUpdate = false;

                    if (pager.ScrollX == 0 && !_isInitialized)
                    {
                        _isInitialized = true;
                        pager.SetCurrentItem(1, false);
                        pager.ScrollX = pager.Width;

                        shouldUpdate = true;
                    }

                    if (pager.IsDragging)
                    {
                        if (pager.ScrollX > pager.Width && pager.ScrollX > _previousScrollX)
                        {
                            pager.SetMonth(pager.NextMonth);
                            pager.ScrollX = pager.ScrollX - pager.Width; // right

                            shouldUpdate = true;
                        }
                        else if (pager.ScrollX < pager.Width && pager.ScrollX < _previousScrollX)
                        {
                            pager.SetMonth(pager.PreviousMonth);
                            pager.ScrollX = pager.ScrollX + pager.Width; // left

                            shouldUpdate = true;
                        }
                    }

                    _previousScrollX = pager.ScrollX;

                    if (shouldUpdate)
                    {
                        pager.Item0.Invalidate();
                        pager.Item1.Invalidate();
                        pager.Item2.Invalidate();
                    }
                }
            }

            public void OnPageScrollStateChanged(int state)
            {
                if (_weakPager.TryGetTarget(out CalendarViewPage pager))
                {
                    pager.IsDragging = state == 1;
                }
            }

            public void OnPageSelected(int position)
            { }
        }
    }
}
