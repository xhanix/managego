using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class Unit
    {
        public int BuildingId { get; set; }
        public string Number { get; set; }

        public int PropertyId { get; set; }
        public int UnitId { get; set; }
        public string ShortName { get; set; }
        public string UnitName { get; set; }
        public int LeaseId { get; set; }

        public List<Tenant> Tenants { get; set; }

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
