using System;
using PropertyChanged;

namespace ManageGo.Models
{
    [AddINotifyPropertyChangedInterface]
    public class AvailableTimes
    {
        public TimeSpan Time { get; set; }
        public bool IsAvailable { get; set; }
    }
}
