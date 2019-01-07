using System;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo.Models
{
    [AddINotifyPropertyChangedInterface]
    public class PendingApprovalItem
    {
        public string ApprovalType { get; set; }
        public int LeaseID { get; set; }
        public Tenant Tenant { get; set; }
        public Building Building { get; set; }
        public Unit Unit { get; set; }

        [JsonIgnore]
        public string Title
        {
            get
            {
                if (ApprovalType.ToLower() == "tenant")
                    return Tenant?.TenantFirstName + " " + Tenant?.TenantLastName;
                else
                    return Building.BuildingShortAddress + ", " + Unit.UnitName;
            }
        }

        [JsonIgnore]
        public string SubTitle
        {
            get
            {
                if (ApprovalType.ToLower() == "tenant")
                    return Building.BuildingShortAddress + ", " + Unit.UnitName;
                else
                    return Tenant?.TenantFirstName + " " + Tenant?.TenantLastName;
            }
        }

        [JsonIgnore]
        public string ListIcon
        {
            get
            {
                if (ApprovalType.ToLower() == "tenant")
                    return "profile_red.png";
                else
                    return "building_red.png";
            }
        }
        [JsonIgnore, AlsoNotifyFor("ShowDetailsIcon")]
        public bool DetailsShown { get; set; }
        [JsonIgnore]
        public string ShowDetailsIcon
        {
            get
            {
                if (!DetailsShown)
                    return "chevron_right.png";
                return "chevron_down.png";
            }
        }

    }
}
