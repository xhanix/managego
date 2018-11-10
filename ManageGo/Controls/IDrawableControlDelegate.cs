using System;
using System.Linq;
using System.Collections.Generic;
using SkiaSharp;

namespace CustomCalendar
{
	public interface IDrawableControlDelegate
	{
		void Draw(SKSurface surface, SKImageInfo info);

		void EndInteractions(IEnumerable<SKPoint> points);
	}
}
