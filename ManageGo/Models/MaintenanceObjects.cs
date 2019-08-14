using System;
using System.Collections.Generic;

namespace ManageGo.Models
{
    public class MaintenanceObjects
    {
        public IEnumerable<Categories> Categories { get; set; }
        public IEnumerable<Tags> Tags { get; set; }
        public IEnumerable<ExternalContact> ExternalContacts { get; set; }
    }
}
