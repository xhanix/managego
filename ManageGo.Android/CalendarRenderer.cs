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

[assembly: ExportRenderer(typeof(Calendar), typeof(CalendarRenderer))]
namespace ManageGo.UI.Droid.Renderers
{
    [Preserve(AllMembers = true)]
    public class CalendarRenderer : ViewRenderer<Calendar, CalendarViewPager>
    {
        CalendarViewPager _calendarView;

        int elementWidth;
        int elementHeight;

        bool disposed;

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
            }

            if (e.NewElement != null)
            {
                Element.SizeChanged += ElementSizeChanged;
                Element.UpdateSelectedDates = UpdateSelectedDates;
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

        void InitializeNativeView()
        {
            if (elementWidth <= 0 || elementHeight <= 0)
            {
                return;
            }
            
            ResetNativeView();

            _calendarView = new CalendarViewPager(Context, Element.AllowMultipleSelection, Element.SelectedDates, Element.HighlightedDates);

			_calendarView.OnCurrentMonthYearChange += Element.OnCurrentMonthYearChanged;
            _calendarView.OnSelectedDatesChange += Element.OnDatesChanged;

            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
          
            _calendarView.LayoutParameters = layoutParams;

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