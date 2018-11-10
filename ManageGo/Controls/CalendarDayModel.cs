using System;
using System.Collections.Generic;
using SkiaSharp;

namespace CustomCalendar
{
	public class CalendarDayModel
	{
		public string Description { get; private set; }

		public DateTime DateTime { get; private set; }

		public SKRect Rectangle { get; private set; }

		public HighlightType? Type { get; set; }

		internal CalendarDayModel(int year, int month, int day, float x, float y, float width, float height)
		{
			DateTime = new DateTime(year, month, day);
			Rectangle = new SKRect(x, y, x + width, y + height);
		}

		internal CalendarDayModel(string text, float x, float y, float width, float height)
        {
            Description = text;
            Rectangle = new SKRect(x, y, x + width, y + height);
        }
	}
}
