using System;
using CoreGraphics;
using UIKit;

namespace CustomCalendar.iOS
{
	public class InfiniteScrollView<T> : UICollectionView where T : InfiniteScrollViewCell
	{
		public InfiniteScrollView(IInfiniteScrollViewDelegate<T> del, CGRect frame) : base(frame, new InfiniteScrollViewLayout(frame.Size))
		{
			this.RegisterClassForCell(typeof(T), InfiniteScrollViewCell.Key);
			this.ShowsVerticalScrollIndicator = false;
			this.ShowsHorizontalScrollIndicator = false;
			this.PagingEnabled = true;

			this.Source = new InfiniteScrollViewSource<T>(del);

			this.ScrollToItem(Foundation.NSIndexPath.FromItemSection(1, 0),
							  UICollectionViewScrollPosition.None, false);
			this.Source.DecelerationEnded(this);
		}

		public Foundation.NSIndexPath TryGetVisibleIndexPath()
		{
			var visibleRect = new CGRect(this.ContentOffset, this.Bounds.Size);

			var visiblePoint = new CGPoint(visibleRect.GetMidX(), visibleRect.GetMidY());
			var visibleIndexPath = this.IndexPathForItemAtPoint(visiblePoint);

			return visibleIndexPath;
		}

		public int CurrentIndex
		{
			get
			{
				return (this.Source as InfiniteScrollViewSource<T>).CurrentIndex;
			}
		}
	}
}
