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

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);
            _controlDelegate.Draw(e.Surface, e.Info);
        }
    }
}
