using System;
using System.Collections.Generic;
using Xamarin.Forms;
using PropertyChanged;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using Xamarin.Essentials;
using TouchTracking;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public partial class TicketDetailsPage : ContentPage
    {
        double pageHeight;
        double? TouchX = null;
        double? TouchY = null;
        double StartX;
        double StartY;
        double ClockScale;
        public double pageWidth { get; set; }
        bool WasFocused { get; set; }
        readonly double ReplyBoxHeigh = 250;
        public TicketDetailsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ReplyBox.HeightRequest = ReplyBoxHeigh;
            var mainDisplayInfo = DeviceDisplay.ScreenMetrics;
            var width = mainDisplayInfo.Width;
            MyGrid.WidthRequest = width * 0.9;
            MyGrid.HeightRequest = width * 0.25;

        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            pageHeight = height;
            pageWidth = width * 0.7;

        }




        void Handle_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            ReplyBox.HeightRequest = pageHeight * 0.65;
            MyScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
            WasFocused = true;

            //MyNavBar.IsVisible = false;
            //TopDetailsView.IsVisible = false;
        }



        async void Handle_Scrolled(object sender, Xamarin.Forms.ScrolledEventArgs e)
        {
            if ((WasFocused && e.ScrollY > 12) || (string.IsNullOrWhiteSpace(ReplyEditor.Text) && e.ScrollY > 12))
            {
                await MyScrollView.ScrollToAsync(0, 0, true);
                WasFocused = false;
            }
        }

        void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {
            var c = (Comments)e.Item;
            if (c.Files != null)
            {
                var f = c.Files.ToList();
                c.Files = new System.Collections.ObjectModel.ObservableCollection<File>(f);
            }
        }

        void Handle_Unfocused(object sender, FocusEventArgs e)
        {
            ReplyBox.HeightRequest = ReplyBoxHeigh;
            //MyNavBar.IsVisible = true;
            //TopDetailsView.IsVisible = true;
        }

        #region CLOCK
        void Handle_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            ClockScale = info.Width / MyCanvas.Width;
            canvas.Clear();

            SKPaint grayIndicatorPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#a7a9ac"),
                StrokeWidth = 2
            };

            SKPaint blueIndicatorPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse("#378ef7"),
                StrokeWidth = 3
            };

            var divisions = 12 * 3.3;
            var degrees_per_iter = 360 / divisions;
            var start_angle_deg = 3;

            //draw the indicators

            for (int i = 0; i < divisions; i++)
            {
                var angle_deg = (start_angle_deg + (i * degrees_per_iter)) % 360;
                Console.WriteLine(angle_deg);
                var x = (info.Width / 3) * Math.Cos(angle_deg * (float)Math.PI / 180f);
                var y = (info.Width / 3) * Math.Sin(angle_deg * (float)Math.PI / 180f);
                var endX = (info.Width / 3.5) * Math.Cos(angle_deg * (float)Math.PI / 180f);
                var endY = (info.Width / 3.5) * Math.Sin(angle_deg * (float)Math.PI / 180f);
                //canvas.DrawCircle((float)x + (info.Width / 2), (float)y + (info.Height / 2), 5, grayIndicatorPaint);
                var xStart = (float)x + (info.Width / 2);
                var yStart = (float)y + (info.Height / 2);
                var xEnd = (float)endX + ((info.Width / 2));
                var yEnd = (float)endY + ((info.Height / 2));

                canvas.DrawLine(xStart, yStart, xEnd, yEnd, grayIndicatorPaint);
            }

            var hourDivisions = 12;
            var hour_degrees_per_iter = 360 / hourDivisions;
            var hour_start_angle_deg = 0;
            for (int i = 0; i < hourDivisions; i++)
            {
                var angle_deg = (hour_start_angle_deg + (i * hour_degrees_per_iter)) % 360;

                var x = (info.Width / 3) * Math.Cos(angle_deg * (float)Math.PI / 180f);
                var y = (info.Width / 3) * Math.Sin(angle_deg * (float)Math.PI / 180f);
                var endX = (info.Width / 3.5) * Math.Cos(angle_deg * (float)Math.PI / 180f);
                var endY = (info.Width / 3.5) * Math.Sin(angle_deg * (float)Math.PI / 180f);
                //canvas.DrawCircle((float)x + (info.Width / 2), (float)y + (info.Height / 2), 5, grayIndicatorPaint);
                var xStart = (float)x + (info.Width / 2);
                var yStart = (float)y + (info.Height / 2);
                var xEnd = (float)endX + ((info.Width / 2));
                var yEnd = (float)endY + ((info.Height / 2));

                canvas.DrawLine(xStart, yStart, xEnd, yEnd, blueIndicatorPaint);
            }

            SKPaint handPaint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColor.Parse("#378ef7"),
                StrokeWidth = 5
            };

            if (TouchY.HasValue && TouchX.HasValue)
            {
                double vX = (TouchX.Value) * ClockScale - (info.Width / 2);
                double vY = (TouchY.Value) * ClockScale - (info.Height / 2);
                double magV = Math.Sqrt(vX * vX + vY * vY);
                double aX = (info.Width / 2) + vX / magV * (info.Width / 3.75);
                double aY = (info.Height / 2) + vY / magV * ((info.Width / 3.75));


                double dy = (aY - (info.Height / 2));
                double dx = (aX - (info.Width / 2));

                double theta = Math.Atan2(dy, dx);

                double angle = (90 - ((theta * 180) / Math.PI)) % 360;

                var hr = angle - 180 > 0 ? 12 - Math.Abs(180 - angle) / 30 : Math.Abs(angle - 180) / 30;
                System.Diagnostics.Debug.WriteLine(hr);
                var time = new TimeSpan(0, (int)(hr * 60), 0);
                System.Diagnostics.Debug.WriteLine(time.Hours);
                System.Diagnostics.Debug.WriteLine(time.Minutes);
                canvas.DrawLine(info.Width / 2, info.Height / 2, (float)aX, (float)aY, handPaint);
                canvas.DrawCircle((float)aX, (float)aY, 7, handPaint);
                canvas.DrawCircle(info.Width / 2, info.Height / 2, 15, handPaint);
                //MyLabel.Text = time.ToString(@"hh\:mm\:ss");
            }
            else
            {
                var _x = (info.Width / 3.75) * Math.Cos(start_angle_deg * (float)Math.PI / 180f);
                var _y = (info.Width / 3.75) * Math.Sin(start_angle_deg * (float)Math.PI / 180f);
                StartX = _x;
                StartY = _y;
                canvas.DrawLine(info.Width / 2, info.Height / 2, (float)_x + (info.Width / 2), (float)_y + (info.Height / 2), handPaint);
                canvas.DrawCircle((float)_x + (info.Width / 2), (float)_y + (info.Height / 2), 7, handPaint);
                canvas.DrawCircle(info.Width / 2, info.Height / 2, 15, handPaint);
            }
            if (false)
            {
                grayIndicatorPaint.Style = SKPaintStyle.Fill;
                grayIndicatorPaint.Color = SKColors.Blue;
                canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, grayIndicatorPaint);
            }
        }


        void Handle_TouchAction(object sender, TouchTracking.TouchActionEventArgs args)
        {
            Console.WriteLine("******" + args.Type + "******");
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!EventContainerScrollView.InputTransparent)
                        EventContainerScrollView.InputTransparent = true;

                    EventContainerScrollView.IsEnabled = false;
                    //FirstPoint = new SKPoint((float)args.Location.X, (float)args.Location.Y);
                    break;
                case TouchActionType.Moved:
                    if (!MyScrollView.InputTransparent)
                        MyScrollView.InputTransparent = true;
                    TouchX = args.Location.X;
                    TouchY = args.Location.Y;
                    MyCanvas?.InvalidateSurface();
                    break;

                case TouchActionType.Released:
                    if (EventContainerScrollView.InputTransparent)
                        EventContainerScrollView.InputTransparent = false;
                    EventContainerScrollView.IsEnabled = true;
                    break;
                case TouchActionType.Cancelled:
                    if (EventContainerScrollView.InputTransparent)
                        EventContainerScrollView.InputTransparent = false;
                    EventContainerScrollView.IsEnabled = true;
                    break;
            }
        }
        #endregion
    }
}
