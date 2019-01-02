using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace CustomCalendar
{
    public static class CalendarMonthRenderer
    {
        static void DrawText(SKCanvas canvas, SKPaint paint, string text, float x, float y, int fontSize)
        {
            paint.TextSize = fontSize;
            paint.IsAntialias = true;
            paint.IsStroke = false;
            paint.TextAlign = SKTextAlign.Center;

            canvas.DrawText(text, x, y, paint);
        }

        static void DrawRectangleOutline(SKCanvas canvas, SKPaint paint, SKPath path, float x, float y, float width, float height)
        {
            var lineWidth = 2;

            paint.IsAntialias = false;
            paint.StrokeCap = SKStrokeCap.Square;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = lineWidth;
            paint.Color = SKColors.LightGray;

            canvas.DrawPath(path, paint);

            path.Reset();
        }

        static void DrawRectangle(SKCanvas canvas, SKPaint paint, float x, float y, float width, float height)
        {
            var rect = new SKRect(x, y, x + width, y + height);

            paint.IsAntialias = true;
            paint.StrokeCap = SKStrokeCap.Square;
            paint.Style = SKPaintStyle.Fill;
            paint.StrokeWidth = 0;

            canvas.DrawRoundRect(rect, 10, 10, paint);
        }

        static void DrawCalendarDay(SKCanvas canvas, SKPaint textPaint, SKPaint paint, SKPath path, CalendarDayModel calendarDay, bool isTargetMonth)
        {
            var rect = calendarDay.Rectangle;
            var date = calendarDay.DateTime;

            var x = rect.Left;
            var y = rect.Top;
            var width = rect.Width;
            var height = rect.Height;

            DrawRectangleOutline(canvas, paint, path, x, y, width, height);

            string text;

            if (!string.IsNullOrEmpty(calendarDay.Description))
            {
                text = calendarDay.Description;
                textPaint.Color = SKColor.Parse("#4E9AF5");
            }
            else
            {
                text = date.Day.ToString();

                if (calendarDay.Type != null)
                {
                    var adjusted_x = x + (width * .125f);
                    var adjusted_y = y + (height * .125f);

                    var adjusted_width = width * .75f;
                    var adjusted_height = height * .75f;

                    if (calendarDay.Type == HighlightType.Dark)
                    {
                        paint.Color = SKColor.Parse("#55C433");
                        textPaint.Color = SKColors.White;
                    }
                    else
                    {
                        paint.Color = SKColor.Parse("#E5F6DB");
                        textPaint.Color = SKColor.Parse("#737387");
                    }

                    DrawRectangle(canvas, paint, adjusted_x, adjusted_y, adjusted_width, adjusted_height);
                }
                else if (!isTargetMonth)
                {
                    textPaint.Color = SKColor.Parse("#DCDEDF");
                }
                else
                {
                    textPaint.Color = SKColor.Parse("#737387");
                }
            }

            DrawText(canvas, textPaint,
                     text: text,
                     x: x + (width / 2), y: y + (height / 2) + (height / 8),
                     fontSize: (int)(width / 3));
        }

        public static void Draw(SKSurface surface, SKImageInfo info, CalendarMonthModel calendarMonth)
        {
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            var gridSize = calendarMonth.GridSize;
            var dateTime = new DateTime(calendarMonth.Year, calendarMonth.Month, 1);
            var date = dateTime.AddDays(-dateTime.Day + 1);

            using (var textPaint = new SKPaint())
            {
                using (var paint = new SKPaint())
                {
                    using (var path = new SKPath())
                    {
                        foreach (var calendarDay in calendarMonth.Days)
                        {
                            if (calendarMonth.HighlightedDays?.Count() > 0)
                            {
                                foreach (var highlightDay in calendarMonth.HighlightedDays)
                                {
                                    if (highlightDay.Day == calendarDay.DateTime.Day && calendarDay.DateTime.Month == calendarMonth.Month)
                                    {
                                        calendarDay.Type = highlightDay.Type;
                                    }
                                }
                            }
                            DrawCalendarDay(canvas, textPaint, paint, path, calendarDay, calendarMonth.Month == calendarDay.DateTime.Month);
                        }
                    }
                }
            }
            canvas.Flush();
        }
    }
}
