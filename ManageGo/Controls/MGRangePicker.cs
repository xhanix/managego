using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using TouchTracking;
using Xamarin.Forms;

namespace ManageGo.Controls
{
    public class MGRangePicker : Grid
    {
        readonly SKCanvasView Canvas;
        readonly SKPaint endKnobPaint;
        readonly SKPaint linePaint;
        readonly float knobRadius = 18.0f;
        double? TouchX;
        double? TouchY;

        double? initialTouchX;
        double? initialTouchY;

        double? releaseTouchX;
        double? releaseTouchY;

        public bool InternalValueChange { get; private set; }
        public bool MovingTopKnob { get; private set; }
        public bool MovingBottomKnob { get; private set; }
        public Tuple<float, float> TopKnobCenter { get; private set; }
        public float RangeMax { get; private set; } = 5000;
        public string RangeMaxString { get; private set; } = "5000";
        public Tuple<float, float> BottomKnobCenter { get; private set; }
        public double CanvasScale { get; private set; }
        public float StepSize { get; private set; }
        public float StepDollarValue { get; private set; }
        public float RangeMin { get; private set; }
        public string RangeMinString { get; private set; } = "0";

        public MGRangePicker()
        {
            endKnobPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse("#388ef7"),
            };

            linePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#e5f1fd"),
                StrokeWidth = 10
            };

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
                        initialTouchX = args.Location.X;
                        initialTouchY = args.Location.Y;
                        TouchX = args.Location.X;
                        TouchY = args.Location.Y;
                        MovingTopKnob = false;
                        MovingBottomKnob = false;
                        break;
                    case TouchActionType.Moved:
                        TouchX = args.Location.X;
                        TouchY = args.Location.Y;
                        Canvas.InvalidateSurface();
                        break;

                    case TouchActionType.Released:
                        releaseTouchX = args.Location.X;
                        releaseTouchY = args.Location.Y;
                        MovingTopKnob = false;
                        MovingBottomKnob = false;
                        break;
                    case TouchActionType.Cancelled:
                        MovingTopKnob = false;
                        MovingBottomKnob = false;
                        break;
                }
                InternalValueChange = false;
            };
            TouchX = null;
            TouchY = null;
            this.Effects.Add(touchEffect);


        }

        void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            var halfWidth = info.Width / 2;
            var halfHeight = info.Height / 2;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            CanvasScale = info.Height / this.Height;



            if (!TouchY.HasValue)
            {
                // set the knob locations on first draw pass
                //get y for top of line
                TopKnobCenter = new Tuple<float, float>(halfWidth, 50);
                BottomKnobCenter = new Tuple<float, float>(halfWidth, info.Height - 50);
                var maxLength = BottomKnobCenter.Item2 - TopKnobCenter.Item2;
                //maxRange = $5000
                //minRnge  =  $0
                // stepSize = 20;
                // stepDollarValue = (float)Math.Round((stepSize * 5000) / maxLength);
                StepDollarValue = 100;
                StepSize = (StepDollarValue * maxLength) / 5000;
            }
            else
            {
                // set the knob locations on movement
                var newY = (float)(TouchY.Value * CanvasScale);
                if (newY < 50 && MovingTopKnob)
                    newY = 50;
                else if (newY > info.Height - 50 && MovingBottomKnob)
                    newY = info.Height - 50;
                if (MovingTopKnob || (Math.Abs(newY - TopKnobCenter.Item2) <= 50 && !MovingBottomKnob))
                {
                    MovingTopKnob = true;
                    MovingBottomKnob = false;
                    var numOfSteps = (float)Math.Round((newY - 50) / StepSize);
                    var topDollarValue = 5000 - (numOfSteps * StepDollarValue);
                    if (topDollarValue - RangeMin >= StepDollarValue)
                    {
                        TopKnobCenter = new Tuple<float, float>(halfWidth, 50 + (numOfSteps * StepSize));
                        RangeMax = Math.Max(RangeMin, topDollarValue);
                        RangeMaxString = RangeMax.ToString("C0");
                        Console.WriteLine($"Max value: {RangeMax}");

                    }
                    //get distance from topknobCenter to top of the view
                }
                else if (MovingBottomKnob || (Math.Abs(newY - BottomKnobCenter.Item2) <= 50 && !MovingTopKnob))
                {
                    MovingTopKnob = false;
                    MovingBottomKnob = true;
                    var numOfSteps = (float)Math.Round(Math.Abs(newY - (info.Height - 50)) / StepSize);
                    var bottomDollarValue = (numOfSteps * StepDollarValue);
                    if (RangeMax - bottomDollarValue >= StepDollarValue)
                    {
                        BottomKnobCenter = new Tuple<float, float>(halfWidth, info.Height - (numOfSteps * StepSize) - 50);
                        //get distance from topknobCenter to top of the view
                        RangeMin = Math.Min(RangeMax, bottomDollarValue);
                        Console.WriteLine($"Min value: {RangeMin}");
                        RangeMinString = RangeMin.ToString("C0");
                    }
                }
            }
            canvas.Clear();
            // draw line in between --order of drawing is important
            canvas.DrawLine(TopKnobCenter.Item1, TopKnobCenter.Item2,
                    BottomKnobCenter.Item1, BottomKnobCenter.Item2, linePaint);

            // draw top circle
            canvas.DrawCircle(TopKnobCenter.Item1, TopKnobCenter.Item2, knobRadius, endKnobPaint);

            // draw bottom circle
            canvas.DrawCircle(BottomKnobCenter.Item1, BottomKnobCenter.Item2, knobRadius, endKnobPaint);
        }

    }
}
