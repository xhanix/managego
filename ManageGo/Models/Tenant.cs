using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface, Serializable]
    public class Tenant
    {
        public int TenantID { get; set; }
        public string TenantFirstName { get; set; }
        public string TenantLastName { get; set; }
        public string TenantHomePhone { get; set; }
        public string TenantCellPhone { get; set; }
        public string TenantEmailAddress { get; set; }
        [AlsoNotifyFor("UnitsListHeight")]
        public List<Unit> TenantUnits { get; set; }
        [JsonIgnore]
        public double UnitsListHeight => TenantUnits.Count * 25;
        [JsonIgnore]
        public string FullName => TenantFirstName + " " + TenantLastName;

        [JsonIgnore]
        public string FirstUnitAddress => TenantUnits.Any() ? TenantUnits.Count > 1 ? TenantUnits.First().ShortAddress + $" +{TenantUnits.Count - 1} more buildings" : TenantUnits.First().ShortAddress : "No units for tenant!";

        [JsonIgnore]
        public string ShortDescription => FullName + ", " + FirstUnitAddress;

        [JsonIgnore, AlsoNotifyFor("CheckBoxImage")]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public string CheckBoxImage => IsSelected ? "checked.png" : "unchecked.png";


        [JsonIgnore]
        public bool DetailsShown { get; set; }

    }
}
