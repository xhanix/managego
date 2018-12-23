using System.Collections.Generic;
using System.Linq;
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

        [JsonProperty("ShorAddress")]
        public string ShortAddress { get; set; }

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
        [JsonIgnore]
        public string FormattedTenantNames
        {
            get
            {
                if (Tenants is null || !Tenants.Any())
                {
                    return "Not Found";
                }
                return string.Join(", ", Tenants.Select(t => t.FullName));
            }
        }
    }
}
