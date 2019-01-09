using System.Collections.Generic;
using FreshMvvm;
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
        public List<int> Categories { get; set; }
        public List<int> Buildings { get; set; }
        [JsonIgnore]
        public string UserEmailAddress { get; set; }
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

        public FreshAwaitCommand OnSelectTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    IsSelected = !IsSelected;
                    tcs?.SetResult(true);
                });
            }
        }

        [JsonIgnore]
        public bool IsEnabled { get; set; } = true;
    }
}
