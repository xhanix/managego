using System;
using Newtonsoft.Json;
using PropertyChanged;
using FreshMvvm;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class ExternalContact
    {
        public int ExternalID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }

        [JsonIgnore, AlsoNotifyFor("CheckBoxIcon")]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public string CheckBoxIcon
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
    }
}
