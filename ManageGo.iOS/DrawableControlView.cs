using System;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;

namespace CustomCalendar.iOS
{
	public class DrawableControlView<T> : SKCanvasView where T : IDrawableControlDelegate
	{
		readonly T _controlDelegate;

		public T ControlDelegate
		{
			get
			{
				return _controlDelegate;
			}
		}

		public DrawableControlView(T controlDelegate)
		{
			_controlDelegate = controlDelegate;
		}

		public override void DrawInSurface(SKSurface surface, SKImageInfo info)
		{
			base.DrawInSurface(surface, info);
			_controlDelegate.Draw(surface, info);
		}
	}
}
