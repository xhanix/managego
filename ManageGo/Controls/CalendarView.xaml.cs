using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using CustomCalendar;
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
            get => (DateRange)GetValue(SelectedDatesProperty);
            set => SetValue(SelectedDatesProperty, value);
        }

        static void HandleSelectedDatesPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarView = bindable as CalendarView;

            if (newValue != null)
            {
                calendarView.calendar.SelectedDates = newValue as DateRange;
            }
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

        void Past30DaysButton_Tapped(object sender, EventArgs e)
        {
            calendar.SelectedDates = new DateRange(DateTime.Now.AddDays(-30), DateTime.Now);
            SelectedDates = new DateRange(DateTime.Now.AddDays(-30), DateTime.Now);
            OnPresetRangeUpdate?.Invoke(sender, e);
        }

        void Handle_chevron(DateRange dates)
        {
            SelectedDate = dates.StartDate;
            SelectedDates = dates;
            OnSelectedDatesUpdate?.Invoke(this, EventArgs.Empty);
        }

        void Handle_OnMonthYearChanged(DateTime date)
        {
            titleLabel.Text = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(date.Month)}, {date.Year}";
        }
    }
}
