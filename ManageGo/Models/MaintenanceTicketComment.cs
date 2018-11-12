using System;
using System.Collections.Generic;

namespace ManageGo
{
    public class MaintenanceTicketComment
    {
        public string Text { get; set; }
        public string CommentType { get; set; }
        public IList<File> Files { get; set; }
        public IList<MaintenanceTicketWorkOrder> WorkOrders { get; set; }
        public IList<MaintenanceTicketEvent> Events { get; set; }
    }
}
