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

[assembly: ExportRenderer(typeof(Calendar), typeof(CalendarRenderer))]
namespace ManageGo.UI.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class CalendarRenderer : ViewRenderer<Calendar, CalendarViewPage>
    {
        CalendarViewPage _calendarView;

        int elementWidth;
        int elementHeight;

        bool disposed;

        public bool ShowDisabledDays { get; private set; }

        public CalendarRenderer(Context context) : base(context)
        { }

        protected override void OnElementChanged(ElementChangedEventArgs<Calendar> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                if (Element == null)
                {
                    return;
                }

                Element.SizeChanged -= ElementSizeChanged;
                Element.UpdateSelectedDates = null;
                Element.UpdateEnabledDates = null;
                Element.UpdateHighlightedDates = null;
                Element.OnNextMonthRequested -= _calendarView.GoToNextMonth;
                Element.OnPreviousMonthRequested -= _calendarView.GoToPreviousMonth;
            }

            if (e.NewElement != null)
            {
                Element.SizeChanged += ElementSizeChanged;
                Element.UpdateSelectedDates = UpdateSelectedDates;
                Element.UpdateHighlightedDates = UpdateHighlightedDates;
                Element.UpdateEnabledDates = UpdateAvailableDays;
                Element.OnNextMonthRequested += (_sender, _e) => _calendarView?.GoToNextMonth((object)_sender, _e);
                Element.OnPreviousMonthRequested += (_sender, _e) => _calendarView?.GoToPreviousMonth((object)_sender, _e);
                ShowDisabledDays = Element.ShowDisabledDates;
            }

            InitializeNativeView();
        }

        void ElementSizeChanged(object sender, EventArgs e)
        {
            if (Element == null)
            {
                return;
            }

            //Resize RecyclerView to match size of container
            var rect = this.Element.Bounds;

            if (rect.Height > 0)
            {
                elementWidth = ConvertToDensityIndependentPixels(rect.Width);
                elementHeight = ConvertToDensityIndependentPixels(rect.Height);

                InitializeNativeView();
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

        void InitializeNativeView()
        {
            if (elementWidth <= 0 || elementHeight <= 0)
            {
                return;
            }

            ResetNativeView();

            _calendarView = new CalendarViewPage(Context, Element.AllowMultipleSelection, Element.SelectedDates, Element.HighlightedDates, Element.AvailableDays?.ToList(), ShowDisabledDays);

            _calendarView.OnCurrentMonthYearChange += Element.OnCurrentMonthYearChanged;
            _calendarView.OnSelectedDatesChange += Element.OnDatesChanged;

            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

            _calendarView.LayoutParameters = layoutParams;
            _calendarView.ShowDisabledDays = ShowDisabledDays;
            SetNativeControl(_calendarView);
        }

        void ResetNativeView()
        {
            if (_calendarView == null)
            {
                return;
            }

            if (_calendarView != null)
            {
                _calendarView.RemoveFromParent();
                _calendarView.OnCurrentMonthYearChange -= Element.OnCurrentMonthYearChanged;
                _calendarView.OnSelectedDatesChange -= Element.OnDatesChanged;
                _calendarView.Adapter = null;
                _calendarView = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;

                if (Element != null)
                {
                    ResetNativeView();

                    Element.SizeChanged -= ElementSizeChanged;
                }

                if (_calendarView != null)
                {
                    _calendarView.OnCurrentMonthYearChange -= Element.OnCurrentMonthYearChanged;
                    _calendarView.OnSelectedDatesChange -= Element.OnDatesChanged;
                }
            }

            base.Dispose(disposing);
        }
    }
}