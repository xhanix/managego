using System;
using System.Collections.Generic;
using PropertyChanged;
namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class MaintenanceTicketComment
    {
        public string Text { get; set; }
        public string CommentType { get; set; }
        public List<File> Files { get; set; }
        public List<MaintenanceTicketWorkOrder> WorkOrders { get; set; }
        public List<MaintenanceTicketEvent> Events { get; set; }
    }
}
