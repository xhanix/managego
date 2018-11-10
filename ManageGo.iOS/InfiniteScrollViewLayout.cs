using System;
using CoreGraphics;
using UIKit;

namespace CustomCalendar.iOS
{
	public class InfiniteScrollViewLayout : UICollectionViewFlowLayout
	{
		public InfiniteScrollViewLayout(CGSize itemSize)
		{
			this.ItemSize = itemSize;
			this.MinimumLineSpacing = 0;
			this.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
		}
	}
}
