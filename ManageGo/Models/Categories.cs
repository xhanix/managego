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
        public bool IsSelected { get; set; }
        //for filtering multiple can be selected


        //only one assigned per ticket

        [JsonIgnore]
        public string CheckBoxImage
        {
            get
            {
                return IsSelected ? "checked.png" : "unchecked.png";
            }
        }

        private bool isEnabled = true;

        [JsonIgnore]
        public bool IsEnabled
        {
            get { return isEnabled; }
            internal set
            {
                isEnabled = value;
                IsSelected &= value;
            }
        }
    }
}
