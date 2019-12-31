using System;
using PropertyChanged;

namespace ManageGo.Models
{
    //Pending = 0, Approved = 1, Declined = 2, Canceled = 3
    [AddINotifyPropertyChangedInterface]
    public class AmenityStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
