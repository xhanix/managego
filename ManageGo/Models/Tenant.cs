using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
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
        public string FullName { get { return TenantFirstName + " " + TenantLastName; } }

        [JsonIgnore]
        public string FirstUnitAddress
        {
            get
            {
                return TenantUnits != null && TenantUnits.Any(t => !string.IsNullOrWhiteSpace(t.UnitName)) ? TenantUnits.First(t => !string.IsNullOrWhiteSpace(t.UnitName)).ShortAddress : "[Address not available]";
            }
        }

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


        [JsonIgnore]
        public bool DetailsShown { get; set; }

    }
}
