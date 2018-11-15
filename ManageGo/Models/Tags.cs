using System;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class Tags
    {
        public string TagName { get; set; }
        public int TagID { get; set; }
        public string Color { get; set; }

        [JsonIgnore, AlsoNotifyFor("CheckBoxImage")]
        public bool IsSelectedForFiltering { get; set; }


        [JsonIgnore]
        public string CheckBoxImage
        {
            get
            {
                return IsSelectedForFiltering ? "checked.png" : "unchecked.png";
            }
        }
    }
}
