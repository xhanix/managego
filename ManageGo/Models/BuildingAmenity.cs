using System;
using PropertyChanged;

namespace ManageGo.Models
{
    [AddINotifyPropertyChangedInterface]
    public class BuildingAmenity : MGDataAccessLibrary.Models.Amenities.Responses.Amenity
    {
        public bool IsSelected { get; set; }
    }
}
