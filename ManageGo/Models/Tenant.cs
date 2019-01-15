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
        public List<Unit> TenantUnits { get; set; }

        [JsonIgnore]
        public string FullName => TenantFirstName + " " + TenantLastName;

        private string firstUnitAddress;
        [JsonIgnore]
        public string FirstUnitAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(firstUnitAddress))
                    return firstUnitAddress;
                if (TenantUnits is null || !TenantUnits.Any())
                    return "No units for tenant!";
                return TenantUnits.First().ShortAddress;
            }
            set => firstUnitAddress = value;
        }
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
