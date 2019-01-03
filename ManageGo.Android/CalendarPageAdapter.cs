using Android.Views;
using System;

namespace CustomCalendar.Droid
{

    public class CalendarPageAdapter : Android.Support.V4.View.PagerAdapter
    {
        WeakReference<CalendarViewPage> _weakPager;

        public CalendarPageAdapter(CalendarViewPage pager)
        {
            _weakPager = new WeakReference<CalendarViewPage>(pager);
        }

        public override int Count
        {
            get
            {
                return 3;
            }
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            if (_weakPager.TryGetTarget(out CalendarViewPage pager))
            {
                var color = Android.Graphics.Color.Red;

                if (position == 1)
                {
                    color = Android.Graphics.Color.Blue;
                }
                else if (position == 2)
                {
                    color = Android.Graphics.Color.Gold;
                }

                View view;

                switch (position)
                {
                    case 0:
                        {
                            view = pager.Item0;
                            break;
                        }
                    case 1:
                        {
                            view = pager.Item1;
                            break;
                        }
                    default:
                        {
                            view = pager.Item2;
                            break;
                        }
                }

                container.RemoveView(view);
                container.AddView(view);

                return view;
            }

            return null;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        { }

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view == @object;
        }
    }
}
