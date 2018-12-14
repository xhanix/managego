using System;
using System.Collections.Generic;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class TicketDetails
    {
        public string Priority { get; set; }
        public int TenantID { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<Comments> Comments { get; set; }

        public List<Tags> Tags { get; set; }
        public List<Tenant> Tenants { get; set; }
        public List<Categories> Categories { get; set; }

    }
}
