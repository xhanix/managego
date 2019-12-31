using System;
using System.Collections.Generic;
using CustomCalendar;
using Xamarin.Forms;

namespace ManageGo.Controls
{
    public class Calendar : View
    {
        public event CurrentMonthYearHandler OnMonthYearChanged;
        public event EventHandler OnNextMonthRequested;
        public event EventHandler OnPreviousMonthRequested;
        public event DateRangeHandler chevron;

        public Action<DateRange> UpdateSelectedDates { get; set; }
        public Action<List<DateTime>> UpdateHighlightedDates { get; set; }
        public Action<IEnumerable<DateTime>> UpdateEnabledDates { get; set; }

        public bool AllowMultipleSelection { get; set; }


        public static readonly BindableProperty AvailableDaysProperty
            = BindableProperty.Create(nameof(AvailableDays),
                                      typeof(List<DateTime>),
                                      typeof(Calendar),
                                      new List<DateTime>(),
                                      BindingMode.TwoWay,
                                      propertyChanged: HandleAvailableDaysPropertyChanged);

        public List<DateTime> AvailableDays
        {
            get => (List<DateTime>)GetValue(AvailableDaysProperty);
            set
            {
                SetValue(AvailableDaysProperty, value);
                UpdateEnabledDates?.Invoke(value);
            }
        }

        static void HandleAvailableDaysPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendar = bindable as Calendar;
            calendar?.UpdateEnabledDates?.Invoke(newValue as IEnumerable<DateTime>);
        }

        public static readonly BindableProperty SelectedDatesProperty
            = BindableProperty.Create(nameof(SelectedDates),
                                      typeof(DateRange),
                                      typeof(Calendar),
                                      new DateRange(DateTime.Now),
                                      BindingMode.TwoWay,
                                      propertyChanged: HandleSelectedDatesPropertyChanged);

        public DateRange SelectedDates
        {
            get
            {
                var v = GetValue(SelectedDatesProperty) as DateRange;
                return v;
            }
            set
            {
                SetValue(SelectedDatesProperty, value);
                this.UpdateHighlightedDates?.Invoke(value.GetDateRangeDates());
            }
        }

        static void HandleSelectedDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendar = bindable as Calendar;
            calendar?.UpdateSelectedDates?.Invoke(newValue as DateRange);
        }

        public static readonly BindableProperty HighlightedDatesProperty
            = BindableProperty.Create(nameof(HighlightedDates),
                                      typeof(List<DateTime>),
                                      typeof(Calendar),
                                      null,
                                      BindingMode.TwoWay,
                                      propertyChanged: HandleHighlightedDatesPropertyChanged);

        public List<DateTime> HighlightedDates
        {
            get => (List<DateTime>)GetValue(HighlightedDatesProperty);
            set => SetValue(HighlightedDatesProperty, value);
        }

        static void HandleHighlightedDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendar = bindable as Calendar;
            calendar?.UpdateHighlightedDates?.Invoke(newValue as List<DateTime>);
        }

        public static readonly BindableProperty CurrentMonthYearProperty
            = BindableProperty.Create(nameof(CurrentMonthYear),
                                      typeof(DateTime),
                                      typeof(Calendar),
                                      DateTime.Now,
                                      BindingMode.TwoWay);

        public DateTime CurrentMonthYear
        {
            get => (DateTime)GetValue(CurrentMonthYearProperty);
            set
            {
                SetValue(CurrentMonthYearProperty, value);
            }
        }

        public static readonly BindableProperty ShowDisabledDatesProperty = BindableProperty.Create(nameof(ShowDisabledDates),
                                                                                    typeof(bool),
                                                                                    typeof(CalendarView),
                                                                                                     false,
                                                                                                     propertyChanged: HandleShowDisabledDatesPropertyChanged);


        public bool ShowDisabledDates
        {
            get { return (bool)GetValue(ShowDisabledDatesProperty); }
            set { SetValue(ShowDisabledDatesProperty, value); }
        }

        static void HandleShowDisabledDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Calendar calendar)
                calendar.ShowDisabledDates = (bool)newValue;
        }

        public void OnCurrentMonthYearChanged(DateTime date)
        {
            if (CurrentMonthYear.Month != date.Month || CurrentMonthYear.Year != date.Year)
            {
                CurrentMonthYear = date;
                OnMonthYearChanged?.Invoke(date);
            }
        }
        public void GotoNextMonth() => OnNextMonthRequested?.Invoke(this, EventArgs.Empty);
        public void GotoPreviousMonth() => OnPreviousMonthRequested?.Invoke(this, EventArgs.Empty);


        public void OnDatesChanged(DateRange dates) => chevron?.Invoke(dates);
    }
}
