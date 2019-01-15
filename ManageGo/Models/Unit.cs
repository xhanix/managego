using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface, Serializable]
    public class Unit
    {
        /// <summary>
        /// Building Id not populated when getting units from BuildingDetails API call
        /// </summary>
        /// <value>The building identifier.</value>
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
                    return "Empty";
                }
                return string.Join(", ", Tenants.Select(t => t.FullName));
            }
        }
    }
}
