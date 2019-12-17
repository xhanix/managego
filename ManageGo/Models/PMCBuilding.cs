using System;
using PropertyChanged;

namespace ManageGo.Models
{
    [AddINotifyPropertyChangedInterface]
    public class PMCBuilding : MGDataAccessLibrary.Models.Amenities.Responses.PMCBuilding
    {
        public bool IsSelected { get; set; }
    }
}
