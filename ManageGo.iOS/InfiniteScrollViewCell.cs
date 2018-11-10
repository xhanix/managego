using System;
using Foundation;
using UIKit;

namespace CustomCalendar.iOS
{
	public class InfiniteScrollViewCell : UICollectionViewCell
	{
		public static string Key = "Cell";

		public InfiniteScrollViewCell(IntPtr ptr) : base(ptr)
		{
		}

		internal bool IsInitialized { get; set; }
	}
}
