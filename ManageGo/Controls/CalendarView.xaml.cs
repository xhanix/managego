using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CustomCalendar;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo.Controls
{
    public partial class CalendarView : Grid
    {
        public event EventHandler OnPresetRangeUpdate;
        public event EventHandler OnSelectedDatesUpdate;

        public static readonly BindableProperty AllowMultipleSelectionProperty = BindableProperty.Create(nameof(AllowMultipleSelection),
                                                                                        typeof(bool),
                                                                                        typeof(CalendarView),
                                                                                                         false,
                                                                                                         propertyChanged: AllowMultipleSelectionPropertyChanged);


        public bool AllowMultipleSelection
        {
            get { return (bool)GetValue(AllowMultipleSelectionProperty); }
            set { SetValue(AllowMultipleSelectionProperty, value); }
        }

        public static readonly BindableProperty AllowPastSelectionProperty = BindableProperty.Create(nameof(AllowPastSelection),
                                                                                       typeof(bool),
                                                                                       typeof(CalendarView),
                                                                                                        true,
                                                                                                        propertyChanged: null);


        public bool AllowPastSelection
        {
            get { return (bool)GetValue(AllowPastSelectionProperty); }
            set { SetValue(AllowPastSelectionProperty, value); }
        }


        public static readonly BindableProperty ShowDisabledDatesProperty = BindableProperty.Create(nameof(ShowDisabledDates),
                                                                                     typeof(bool),
                                                                                     typeof(CalendarView),
                                                                                                      false,
                                                                                                      propertyChanged: HandleShowDisabledDatesPropertyChanged);


        public bool ShowDisabledDates
        {
            get { return (bool)GetValue(ShowDisabledDatesProperty); }
            set
            {
                SetValue(ShowDisabledDatesProperty, value);
            }
        }


        static void HandleShowDisabledDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarView = bindable as CalendarView;

            if (newValue != null)
            {
                var d = (bool)newValue;
                calendarView.calendar.ShowDisabledDates = d;
            }
        }

        public static readonly BindableProperty AvailableDaysProperty = BindableProperty.Create(nameof(AvailableDaysProperty),
                                                                                      typeof(IEnumerable<DateTime>),
                                                                                      typeof(CalendarView),
                                                                                                       new List<DateTime>(),
                                                                                                       propertyChanged: HandleAvailableDatesPropertyChanged);


        public IEnumerable<DateTime> AvailableDays
        {
            get { return (IEnumerable<DateTime>)GetValue(AvailableDaysProperty); }
            set
            {
                SetValue(AvailableDaysProperty, value);
            }
        }


        static void HandleAvailableDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarView = bindable as CalendarView;

            if (newValue != null)
            {
                var d = (newValue as IEnumerable<DateTime>)?.ToList();
                calendarView.calendar.AvailableDays = d;
            }
        }




        static void AllowMultipleSelectionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarView = bindable as CalendarView;

            bool allowMultiplSelection = (bool)newValue;

            calendarView.calendar.AllowMultipleSelection = allowMultiplSelection;
            calendarView.past7DaysButton.IsVisible = allowMultiplSelection;
            calendarView.past30DaysButton.IsVisible = allowMultiplSelection;
        }

        public static readonly BindableProperty SelectedDateProperty
            = BindableProperty.Create(nameof(SelectedDate),
                                      typeof(DateTime),
                                      typeof(CalendarView),
                                      DateTime.Now,
                                      BindingMode.TwoWay);

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set
            {
                SetValue(SelectedDateProperty, value);

                calendar.SelectedDates = new DateRange(value);
            }
        }

        public static readonly BindableProperty SelectedDatesProperty
            = BindableProperty.Create(nameof(SelectedDates),
                                      typeof(DateRange),
                                      typeof(CalendarView),
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
            set => SetValue(SelectedDatesProperty, value);
        }

        static void HandleSelectedDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarView = bindable as CalendarView;
            calendarView.calendar.SelectedDates = newValue as DateRange;

        }

        public static readonly BindableProperty HighlightedDatesProperty
            = BindableProperty.Create(nameof(HighlightedDates),
                                      typeof(List<DateTime>),
                                      typeof(CalendarView),
                                      new List<DateTime>(),
                                      BindingMode.TwoWay,
                                      propertyChanged: HandleHighlightedDatesPropertyChanged);

        public List<DateTime> HighlightedDates
        {
            get => (List<DateTime>)GetValue(HighlightedDatesProperty);
            set => SetValue(HighlightedDatesProperty, value);
        }

        static void HandleHighlightedDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarView = bindable as CalendarView;

            if (newValue != null)
            {
                calendarView.calendar.HighlightedDates = newValue as List<DateTime>;
            }
        }

        public CalendarView()
        {
            InitializeComponent();

            titleLabel.Text = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(DateTime.Now.Month)}, {DateTime.Now.Year}";

            past7DaysButton.Tapped += Past7DaysButton_Tapped;
            past30DaysButton.Tapped += Past30DaysButton_Tapped;
        }

        void Past7DaysButton_Tapped(object sender, EventArgs e)
        {
            calendar.SelectedDates = new DateRange(DateTime.Now.AddDays(-7), DateTime.Now);

            SelectedDates = new DateRange(DateTime.Now.AddDays(-7), DateTime.Now);

            OnPresetRangeUpdate?.Invoke(sender, e);
        }

        void ShowNextMonth_Tapped(object sender, EventArgs e)
        {
            calendar.GotoNextMonth();
        }

        void ShowPreviousMonth_Tapped(object sender, EventArgs e)
        {
            calendar.GotoPreviousMonth();
        }

        void Past30DaysButton_Tapped(object sender, EventArgs e)
        {
            var dateRange = new DateRange(DateTime.Now.AddDays(-30), DateTime.Now);
            calendar.SelectedDates = dateRange;
            SelectedDates = dateRange;
            OnPresetRangeUpdate?.Invoke(sender, e);
        }

        void Handle_chevron(DateRange dates)
        {
            SelectedDate = dates.StartDate;
            SelectedDates = dates;
            OnSelectedDatesUpdate?.Invoke(this, EventArgs.Empty);
        }

        public static readonly BindableProperty CurrentMonthYearChangedCommandProperty = BindableProperty.Create(
          "CurrentMonthYearChangedCommand",
          typeof(ICommand),
          typeof(CalendarView));

        public ICommand CurrentMonthYearChangedCommand
        {
            get => (ICommand)GetValue(CurrentMonthYearChangedCommandProperty);
            set => SetValue(CurrentMonthYearChangedCommandProperty, value);
        }

        void Handle_OnMonthYearChanged(DateTime date)
        {
            titleLabel.Text = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(date.Month)}, {date.Year}";
            CurrentMonthYearChangedCommand?.Execute(date);

        }
    }
}
