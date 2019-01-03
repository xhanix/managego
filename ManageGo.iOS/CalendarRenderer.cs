using System;
using System.Collections.Generic;
using CoreGraphics;
using CustomCalendar;
using Foundation;
using ManageGo.Controls;
using ManageGo.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Calendar), typeof(CalendarRenderer))]
namespace ManageGo.iOS
{
    //[Preserve(AllMembers = true)]
    public class CalendarRenderer : ViewRenderer<Calendar, CustomCalendar.iOS.CalendarView>
    {
        CustomCalendar.iOS.CalendarView _calendarView;

        double elementWidth;
        double elementHeight;

        bool disposed;

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
                Element.UpdateHighlightedDates = null;
            }

            if (e.NewElement != null)
            {
                Element.SizeChanged += ElementSizeChanged;
                Element.UpdateSelectedDates = UpdateSelectedDates;
                Element.UpdateHighlightedDates = UpdateHighlightedDates;
            }

            InitializeNativeView();
        }

        void UpdateSelectedDates(DateRange dates)
        {
            _calendarView?.UpdateSelectedDates(dates);
        }

        void UpdateHighlightedDates(List<DateTime> dates)
        {
            _calendarView?.UpdateHighlatedDates(dates);
        }

        void InitializeNativeView()
        {
            if (elementWidth <= 0 || elementHeight <= 0)
            {
                return;
            }

            ResetNativeView();

            //Element.HighlightedDates = new List<DateTime> { new DateTime(2018, 10, 10), new DateTime(2018, 10, 21), new DateTime(2018, 10, 27) };

            _calendarView = new CustomCalendar.iOS.CalendarView(new CGRect(0, 0, elementWidth, elementHeight),
                                                                Element.AllowMultipleSelection, Element.SelectedDates, Element.HighlightedDates);

            _calendarView.OnCurrentMonthYearChange += Element.OnCurrentMonthYearChanged;
            _calendarView.OnSelectedDatesChange += Element.OnDatesChanged;
            Element.OnNextMonthRequested += _calendarView.GoToNextMonth;
            Element.OnPreviousMonthRequested += _calendarView.GoToPreviousMonth;
            SetNativeControl(_calendarView);
        }

        void ResetNativeView()
        {
            if (_calendarView != null)
            {
                _calendarView.OnCurrentMonthYearChange -= Element.OnCurrentMonthYearChanged;
                _calendarView.OnSelectedDatesChange -= Element.OnDatesChanged;
                Element.OnNextMonthRequested -= _calendarView.GoToNextMonth;
                Element.OnPreviousMonthRequested -= _calendarView.GoToPreviousMonth;
                _calendarView.RemoveFromSuperview();
                _calendarView.Dispose();
                _calendarView = null;
            }
        }

        void ElementSizeChanged(object sender, EventArgs e)
        {
            if (Element == null)
            {
                return;
            }

            var rect = this.Element.Bounds;

            if (rect.Height > 0)
            {
                elementWidth = rect.Width;
                elementHeight = rect.Height;

                InitializeNativeView();
            }
        }

        public override void MovedToSuperview()
        {
            if (Control == null)
            {
                ElementSizeChanged(Element, null);
            }

            base.MovedToSuperview();
        }

        public override void MovedToWindow()
        {
            if (Control == null)
            {
                ElementSizeChanged(Element, null);
            }

            base.MovedToWindow();
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
                    Element.UpdateSelectedDates = null;
                }

                if (_calendarView != null)
                {
                    _calendarView.OnCurrentMonthYearChange -= Element.OnCurrentMonthYearChanged;
                    _calendarView.OnSelectedDatesChange -= Element.OnDatesChanged;
                    if (Element != null)
                    {
                        Element.OnNextMonthRequested += _calendarView.GoToNextMonth;
                        Element.OnPreviousMonthRequested += _calendarView.GoToPreviousMonth;
                    }
                }
            }

            base.Dispose(disposing);
        }
    }
}
