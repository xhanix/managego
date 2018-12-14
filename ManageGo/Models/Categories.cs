using System;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class Categories
    {
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public string Color { get; set; }

        [JsonIgnore, AlsoNotifyFor("CheckBoxImage")]
        public bool IsSelectedForFiltering { get; set; }
        //for filtering multiple can be selected

        [JsonIgnore, AlsoNotifyFor("RadioImage")]
        public bool IsSelectedForTicket { get; set; }
        //only one assigned per ticket

        [JsonIgnore]
        public string CheckBoxImage
        {
            get
            {
                return IsSelectedForFiltering ? "checked.png" : "unchecked.png";
            }
        }

        [JsonIgnore]
        public string RadioImage
        {
            get
            {
                return IsSelectedForFiltering ? "radio_selected.png" : "radio_unselected.png";
            }
        }
    }
}
