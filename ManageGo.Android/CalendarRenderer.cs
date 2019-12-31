using System;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Xamarin.Forms;

using ManageGo.UI.Droid.Renderers;
using Xamarin.Forms.Platform.Android;

using ManageGo.Controls;
using CustomCalendar.Droid;
using CustomCalendar;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(Calendar), typeof(CalendarRenderer))]
namespace ManageGo.UI.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class CalendarRenderer : ViewRenderer<Calendar, CalendarViewPage>
    {
        CalendarViewPage _calendarView;

        public bool ShowDisabledDays { get; private set; }

        public CalendarRenderer(Context context) : base(context)
        { }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Calendar> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                //  e.OldElement.SizeChanged -= ElementSizeChanged;
                e.OldElement.UpdateSelectedDates = null;
                e.OldElement.UpdateEnabledDates = null;
                e.OldElement.UpdateHighlightedDates = null;
                e.OldElement.OnNextMonthRequested -= _calendarView.GoToNextMonth;
                e.OldElement.OnPreviousMonthRequested -= _calendarView.GoToPreviousMonth;
                if (_calendarView != null)
                {
                    _calendarView.OnCurrentMonthYearChange -= e.OldElement.OnCurrentMonthYearChanged;
                    _calendarView.OnSelectedDatesChange -= e.OldElement.OnDatesChanged;
                }
            }

            if (e.NewElement != null)
            {
                //  e.NewElement.SizeChanged += ElementSizeChanged;
                e.NewElement.UpdateSelectedDates = UpdateSelectedDates;
                e.NewElement.UpdateHighlightedDates = UpdateHighlightedDates;
                e.NewElement.UpdateEnabledDates = UpdateAvailableDays;
                e.NewElement.OnNextMonthRequested += (_sender, _e) => _calendarView?.GoToNextMonth((object)_sender, _e);
                e.NewElement.OnPreviousMonthRequested += (_sender, _e) => _calendarView?.GoToPreviousMonth((object)_sender, _e);
                ShowDisabledDays = e.NewElement.ShowDisabledDates;
                _calendarView = new CalendarViewPage(Context, e.NewElement.AllowMultipleSelection, e.NewElement.SelectedDates, e.NewElement.HighlightedDates, e.NewElement.AvailableDays?.ToList(), ShowDisabledDays);
                _calendarView.OnCurrentMonthYearChange += e.NewElement.OnCurrentMonthYearChanged;
                _calendarView.OnSelectedDatesChange += e.NewElement.OnDatesChanged;
                LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                _calendarView.LayoutParameters = layoutParams;
                _calendarView.ShowDisabledDays = ShowDisabledDays;
                SetNativeControl(_calendarView);
            }


        }



        int ConvertToDensityIndependentPixels(double pixelValue)
        {
            return (int)(Context.ToPixels(pixelValue));
        }

        void UpdateSelectedDates(DateRange dates)
        {
            _calendarView?.UpdateSelectedDates(dates);
        }

        void UpdateHighlightedDates(List<DateTime> dates)
        {
            _calendarView?.UpdateHighlightedDates(dates);
        }

        void UpdateAvailableDays(IEnumerable<DateTime> days)
        {
            if (days != null)
                _calendarView?.UpdateAvailableDays(days.ToList());
        }



    }
}