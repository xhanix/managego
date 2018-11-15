using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class User
    {
        public int UserID { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserFullName => $"{UserFirstName} {UserLastName}".Trim();
        [JsonIgnore]
        public string UserEmailAddress { get; set; }


        public List<int> Categories { get; set; }


        [JsonIgnore, AlsoNotifyFor("CheckBoxImage")]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public string CheckBoxImage
        {
            get
            {
                return IsSelected ? "checked.png" : "unchecked.png";
            }
        }
    }
}
