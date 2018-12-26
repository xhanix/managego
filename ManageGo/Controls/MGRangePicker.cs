using System;
using SkiaSharp.Views.Forms;
using TouchTracking;
using Xamarin.Forms;

namespace ManageGo.Controls
{
    public class MGRangePicker : Grid
    {
        readonly SKCanvasView Canvas;
        double? TouchX;
        double? TouchY;

        public bool InternalValueChange { get; private set; }

        public MGRangePicker()
        {

            Canvas = new SKCanvasView
            {
                IgnorePixelScaling = false,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            Canvas.PaintSurface += Canvas_PaintSurface;
            Children.Add(Canvas);

            var touchEffect = new TouchEffect
            {
                Capture = true
            };

            touchEffect.TouchAction += (object sender, TouchActionEventArgs args) =>
            {
                InternalValueChange = true;
                switch (args.Type)
                {
                    case TouchActionType.Pressed:
                        TouchX = args.Location.X;
                        TouchY = args.Location.Y;
                        Canvas.InvalidateSurface();
                        break;
                    case TouchActionType.Moved:
                        TouchX = args.Location.X;
                        TouchY = args.Location.Y;
                        Canvas.InvalidateSurface();
                        break;

                    case TouchActionType.Released:

                        break;
                    case TouchActionType.Cancelled:

                        break;
                }
                InternalValueChange = false;
            };
            TouchX = null;
            TouchY = null;
            this.Effects.Add(touchEffect);

        }

        void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {

        }

    }
}
