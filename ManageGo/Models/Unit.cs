using System.Collections.Generic;

namespace ManageGo
{
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
    }
}
