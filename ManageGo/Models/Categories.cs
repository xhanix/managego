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

        [JsonIgnore]
        public bool IsSelectedForFiltering { get; set; }


        [JsonIgnore, AlsoNotifyFor("CheckBoxImage")]
        public string CheckBoxImage
        {
            get
            {
                return IsSelectedForFiltering ? "checked.png" : "unchecked.png";
            }
        }
    }
}
