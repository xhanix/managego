using System;
using System.Collections.Generic;
using Foundation;

namespace CustomCalendar.iOS
{
    public interface IInfiniteScrollViewDelegate<T> where T : InfiniteScrollViewCell
    {
        void InitializeCell(InfiniteScrollView<T> infiniteScrollView, T cell, int index);

        void UpdateCell(InfiniteScrollView<T> infiniteScrollView, T cell, int index);

        void OnCurrentIndexChanged(InfiniteScrollView<T> infiniteScrollView, int currentIndex);
    }
}
