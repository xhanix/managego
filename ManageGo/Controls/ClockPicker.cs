using System;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using TouchTracking;
using Xamarin.Essentials;

namespace ManageGo
{
    public class ClockPicker : Grid
    {
        double? TouchX;
        double? TouchY;
        double StartX;
        double StartY;
        double current_aaX;
        double cuurent_aaY;
        double current_aX;
        double current_aY;
        double current_dX;
        double current_dY;
        double ClockScale;
        bool newTouches;
        bool didSetupValues;
        readonly SKPaint handPaint;
        readonly SKPaint fillPaint;
        readonly SKPaint grayIndicatorPaint;
        readonly SKPaint grayLinePaint;
        DateTime currentTime;
        readonly SKPaint blueIndicatorPaint;
        SKPath path1;
        SKPath path2;
        readonly float HandCenterCircleRadius = 15f;
        readonly float HandEndCircleRadius = 7.5f;
        readonly static int divisions = (4 * 12);
        readonly double degrees_per_iter = 360 / divisions;
        readonly double start_angle_deg = 0;
        readonly static int hourDivisions = 12;
        readonly double hour_degrees_per_iter = 360 / hourDivisions;
        readonly double hour_start_angle_deg = -90;
        readonly SKCanvasView Canvas;

        public static readonly BindableProperty TimeProperty =
              BindableProperty.Create("Time", typeof(DateTime), typeof(DateTime), null, BindingMode.TwoWay);

        public DateTime Time
        {
            get { return (DateTime)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }


        public bool Justpassed12 { get; private set; }
        public bool WentPast11 { get; private set; }


        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (Math.Abs(height - width) > float.Epsilon)
                HeightRequest = width;
        }

        public ClockPicker()
        {
            Padding = 0;
            handPaint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColor.Parse("#378ef7"),
                StrokeWidth = 5
            };
            path1 = new SKPath { FillType = SKPathFillType.EvenOdd };
            path2 = new SKPath { FillType = SKPathFillType.EvenOdd }; fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse("#a7a9ac")
            };
            grayIndicatorPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse("#a7a9ac"),
                StrokeWidth = 2,
                TextAlign = SKTextAlign.Left
            };
            blueIndicatorPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#378ef7"),
                StrokeWidth = 6
            };

            grayLinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#a7a9ac"),
                StrokeWidth = 2
            };

            var mainDisplayInfo = DeviceDisplay.ScreenMetrics;
            WidthRequest = mainDisplayInfo.Width * 0.80;

            Canvas = new SKCanvasView
            {
                IgnorePixelScaling = false,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            Canvas.PaintSurface += ClockPicker_PaintSurface;
            this.Children.Add(Canvas);
            var touchEffect = new TouchEffect
            {
                Capture = true
            };

            touchEffect.TouchAction += (object sender, TouchActionEventArgs args) =>
            {
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
            };
            this.Effects.Add(touchEffect);
        }



        //drawing
        void ClockPicker_PaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            var halfWidth = info.Width / 2;
            var halfHeight = info.Height / 2;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            ClockScale = info.Width / this.Width;
            var fontSize = (float)Device.GetNamedSize(NamedSize.Medium, typeof(Label)) * (float)Math.Abs(ClockScale);
            grayIndicatorPaint.TextSize = fontSize;
            var divFactor = (float)Math.PI / 180f;
            canvas.Clear();
            path1.Reset();
            path2.Reset();

            canvas.DrawCircle(halfWidth, halfHeight, info.Width / 2.1f, grayLinePaint);

            for (int i = 0; i < divisions; i++)
            {
                var angle_deg = ((start_angle_deg + (i * 0.5)) + (i * degrees_per_iter)) % 360;
                var multFactor = angle_deg * divFactor;
                var x = (info.Width / 2.2) * Math.Cos(multFactor);
                var y = (info.Height / 2.2) * Math.Sin(multFactor);
                var endX = (info.Width / 2.5) * Math.Cos(multFactor);
                var endY = (info.Height / 2.5) * Math.Sin(multFactor);
                var xStart = (float)x + halfWidth;
                var yStart = (float)y + halfHeight;
                var xEnd = (float)endX + halfWidth;
                var yEnd = (float)endY + halfHeight;
                canvas.DrawLine(xStart, yStart, xEnd, yEnd, grayIndicatorPaint);
            }

            //draw hour
            for (int i = 0; i < hourDivisions; i++)
            {
                var angle_deg = (hour_start_angle_deg + (i * hour_degrees_per_iter)) % 360;
                var x = (info.Width / 2.2) * Math.Cos(angle_deg * divFactor);
                var y = (info.Height / 2.2) * Math.Sin(angle_deg * divFactor);
                var endX = (info.Width / 2.5) * Math.Cos(angle_deg * divFactor);
                var endY = (info.Height / 2.5) * Math.Sin(angle_deg * divFactor);
                var tX = (info.Width / 2.7) * Math.Cos((angle_deg) * divFactor);
                var tY = (info.Height / 2.7) * Math.Sin((angle_deg) * divFactor);
                var xStart = (float)x + halfWidth;
                var yStart = (float)y + halfHeight;
                var xEnd = (float)endX + halfWidth;
                var yEnd = (float)endY + halfHeight;
                var xText = (float)tX + (halfWidth - 18);
                var yText = (float)tY + (halfHeight + 15);
                canvas.DrawLine(xStart, yStart, xEnd, yEnd, blueIndicatorPaint);
                canvas.DrawText($"{(i == 0 ? 12 : i)}", xText, yText, grayIndicatorPaint);
            }

            if (TouchY.HasValue && TouchX.HasValue)
            {
                //distance: touch point to clock center
                double vX = (TouchX.Value * ClockScale) - halfWidth;
                double vY = (TouchY.Value * ClockScale) - halfWidth;
                double magV = Math.Sqrt(vX * vX + vY * vY);
                //point on circle at the end of the clock hand
                double aX = Math.Round(halfWidth + vX / magV * (info.Width / 2.8));
                double aY = Math.Round(halfHeight + vY / magV * ((info.Height / 2.8)));

                //point on circle where triangle indicators will be
                double aaX = Math.Round(halfWidth + vX / magV * (info.Width / 3.50));
                double aaY = Math.Round(halfHeight + vY / magV * (info.Height / 3.50));

                double dy = (aY - halfHeight);
                double dx = (aX - halfWidth);
                if (!didSetupValues)
                {
                    current_dX = dx;
                    current_dY = dy;
                    current_aX = aX;
                    current_aY = aY;
                    current_aaX = aaX;
                    cuurent_aaY = aaY;
                }
                else
                {
                    dy = dy + current_dY;
                    dx = dx + current_dX;
                }
                double theta = Math.Atan2(dy, dx);
                double angle = Math.Round((90 - ((theta * 180) / Math.PI)) % 360, 1);

                var diff = Math.Abs(180 - angle) / 30;


                var hr = Math.Round(angle - 180 > 0 ? 12 - diff : diff, 2);
                var time = TimeSpan.FromTicks(DateTime.Today.Ticks).Add(new TimeSpan(0, (int)(hr * 60), 0));

                didSetupValues = true;
                if (time.Minutes % 15 < 5)
                {
                    newTouches = true;
                    current_aX = aX;
                    current_aY = aY;
                    current_aaX = aaX;
                    cuurent_aaY = aaY;
                    current_dX = 0;
                    current_dY = 0;
                    var timeSpan = new TimeSpan(time.Hours, time.Minutes / 15 * 15, 0);
                    DateTime _time = new DateTime(timeSpan.Ticks);
                    Time = _time;
                }

                //draw circles at beginning and start of hand clock
                canvas.DrawCircle((float)current_aX, (float)current_aY, HandEndCircleRadius, handPaint);
                canvas.DrawCircle(halfWidth, halfHeight, HandCenterCircleRadius, handPaint);
                //draw triangles for the hand
                // calculate distance between the two ends of the clock hand (A-B)
                double DT = Math.Sqrt(Math.Pow((current_aaX - halfWidth), 2) + Math.Pow((cuurent_aaY - halfHeight), 2));
                var offsetPixels = -15.0;
                double D = offsetPixels * 2.5; // base of triangle (C-D)
                double T = D / DT;
                //draw the hand
                canvas.DrawLine(halfWidth, halfHeight, (float)current_aX, (float)current_aY, handPaint);

                var newX = (1 - T) * current_aaX + T * halfWidth;
                var newY = (1 - T) * cuurent_aaY + T * halfHeight;
                //Point C
                var x1 = (float)current_aaX;
                var y1 = (float)cuurent_aaY;
                //Point D
                var x2 = (float)newX;
                var y2 = (float)newY;
                var L = Math.Sqrt((x1 - x2) * (x1 - x2) + ((float)cuurent_aaY - (float)newY) * ((float)cuurent_aaY - (float)newY));
                // offeset points C and D
                var x1p = x1 + offsetPixels * (y2 - y1) / L;
                var x2p = x2 + offsetPixels * (y2 - y1) / L;
                var y1p = y1 + offsetPixels * (x1 - x2) / L;
                var y2p = y2 + offsetPixels * (x1 - x2) / L;
                //tip of triangle (E)
                var midPointX = ((x1p + x2p) / 2) + (offsetPixels * 2) * (y2p - y1p) / L;
                var midPointY = ((y1p + y2p) / 2) + (offsetPixels * 2) * (x1p - x2p) / L;
                //draw triangle path
                //  E
                // / \
                // C-D
                path1.MoveTo((float)x1p, (float)y1p);
                path1.LineTo((float)x2p, (float)y2p);
                path1.LineTo((float)midPointX, (float)midPointY);
                path1.LineTo((float)x1p, (float)y1p);
                path1.Close();
                canvas.DrawPath(path1, fillPaint);
                //inverted triangle
                offsetPixels = offsetPixels * -1;
                x1p = x1 + offsetPixels * (y2 - y1) / L;
                x2p = x2 + offsetPixels * (y2 - y1) / L;
                y1p = y1 + offsetPixels * (x1 - x2) / L;
                y2p = y2 + offsetPixels * (x1 - x2) / L;
                midPointX = ((x1p + x2p) / 2) + (offsetPixels * 2) * (y2p - y1p) / L;
                midPointY = ((y1p + y2p) / 2) + (offsetPixels * 2) * (x1p - x2p) / L;
                path2.MoveTo((float)x1p, (float)y1p);
                path2.LineTo((float)x2p, (float)y2p);
                path2.LineTo((float)midPointX, (float)midPointY);
                path2.LineTo((float)x1p, (float)y1p);
                path2.Close();
                canvas.DrawPath(path2, fillPaint);
            }
            else
            {
                currentTime = DateTime.Now;
                var angle_deg = Math.Round(currentTime.Minute / 15d) * 7.5;
                var hrs = currentTime.Hour;//> 12 ? time.Hours - 12 : time.Hours;
                angle_deg = angle_deg + Math.Round((hrs * hour_degrees_per_iter) - 90);
                var normalizedTime = new TimeSpan(currentTime.Hour, (currentTime.Minute / 15) * 15, 0);
                DateTime _time = DateTime.Today.Add(normalizedTime);
                Time = _time;
                var _x = (info.Width / 2.8) * Math.Cos(angle_deg * divFactor);
                var _y = (info.Height / 2.8) * Math.Sin(angle_deg * divFactor);
                StartX = _x + halfWidth;
                StartY = _y + halfHeight;
                canvas.DrawLine(halfWidth, halfHeight, (float)StartX, (float)StartY, handPaint);
                canvas.DrawCircle((float)StartX, (float)StartY, HandEndCircleRadius, handPaint);
                canvas.DrawCircle(halfWidth, halfHeight, HandCenterCircleRadius, handPaint);
                //draw triangles for the hand
                // calculate distance between the two ends of the clock hand (A-B)
                double DT = Math.Sqrt(Math.Pow((StartX - halfWidth), 2) + Math.Pow((StartY - halfHeight), 2));
                var offsetPixels = -15.0;
                double D = offsetPixels * 3.5; // base of triangle (C-D)
                double T = D / DT;
                //MyLabel.Text = time.ToString(@"hh\:mm\:ss");
                //point on circle where triangle indicators will be
                double aaX = Math.Round(halfWidth + (info.Width / 3.50) * Math.Cos(angle_deg * divFactor));
                double aaY = Math.Round(halfHeight + (info.Height / 3.50) * Math.Sin(angle_deg * divFactor));
                var newX = (1 - T) * aaX + T * halfWidth;
                var newY = (1 - T) * aaY + T * halfHeight;
                //Point C
                var x1 = (float)aaX;
                var y1 = (float)aaY;
                //Point D
                var x2 = (float)newX;
                var y2 = (float)newY;
                var L = Math.Sqrt((x1 - x2) * (x1 - x2) + ((float)aaY - (float)newY) * ((float)aaY - (float)newY));
                // offeset points C and D
                var x1p = x1 + offsetPixels * (y2 - y1) / L;
                var x2p = x2 + offsetPixels * (y2 - y1) / L;
                var y1p = y1 + offsetPixels * (x1 - x2) / L;
                var y2p = y2 + offsetPixels * (x1 - x2) / L;
                //tip of triangle (E)
                var midPointX = ((x1p + x2p) / 2) + (offsetPixels * 2) * (y2p - y1p) / L;
                var midPointY = ((y1p + y2p) / 2) + (offsetPixels * 2) * (x1p - x2p) / L;
                //draw triangle path
                //  E
                // / \
                // C-D
                path1.MoveTo((float)x1p, (float)y1p);
                path1.LineTo((float)x2p, (float)y2p);
                path1.LineTo((float)midPointX, (float)midPointY);
                path1.LineTo((float)x1p, (float)y1p);
                path1.Close();
                canvas.DrawPath(path1, fillPaint);
                //inverted triangle
                offsetPixels = offsetPixels * -1;
                x1p = x1 + offsetPixels * (y2 - y1) / L;
                x2p = x2 + offsetPixels * (y2 - y1) / L;
                y1p = y1 + offsetPixels * (x1 - x2) / L;
                y2p = y2 + offsetPixels * (x1 - x2) / L;
                midPointX = ((x1p + x2p) / 2) + (offsetPixels * 2) * (y2p - y1p) / L;
                midPointY = ((y1p + y2p) / 2) + (offsetPixels * 2) * (x1p - x2p) / L;
                path2.MoveTo((float)x1p, (float)y1p);
                path2.LineTo((float)x2p, (float)y2p);
                path2.LineTo((float)midPointX, (float)midPointY);
                path2.LineTo((float)x1p, (float)y1p);
                path2.Close();
                canvas.DrawPath(path2, fillPaint);
            }
            newTouches = false;
        }

    }
}
