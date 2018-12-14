using System.Collections.Generic;
using Newtonsoft.Json;

namespace ManageGo
{
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
    }
}
