using System;
using System.Linq;
using UIKit;
using SkiaSharp;
using SkiaSharp.Views.iOS;

namespace CustomCalendar.iOS
{
	public class CalendarViewCell : InfiniteScrollViewCell
	{
		readonly DrawableControlView<CalendarMonthControl> _control;

		public CalendarViewCell(IntPtr ptr) : base(ptr)
		{
			Console.WriteLine(ptr);
			_control = new DrawableControlView<CalendarMonthControl>(new CalendarMonthControl());
			this.Add(_control);
			this.BackgroundColor = UIColor.White;
		}

		public CalendarMonthControl ControlDelegate
		{
			get
			{
				return _control.ControlDelegate;
			}
		}

		public override void SetNeedsDisplay()
		{
			base.SetNeedsDisplay();
			_control.SetNeedsDisplay();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = Bounds;
			_control.Frame = bounds;
		}

		public override void TouchesEnded(Foundation.NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			var touch = touches.AnyObject as UITouch;
			if (touch == null)
				return;

			var touchPoint = touch.LocationInView(this);
			var screenScale = (float)UIScreen.MainScreen.Scale;

			var points = new SKPoint[] { new SKPoint((float)touchPoint.X * screenScale, (float)touchPoint.Y * screenScale) };
			ControlDelegate.EndInteractions(points);
		}
	}
}
