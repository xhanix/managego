using System;
using System.Collections.Generic;
using PropertyChanged;
using System.Linq;
using Newtonsoft.Json;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class MaintenanceTicket
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; }
        public string TicketStatus { get; set; }
        public DateTime TicketCreateTime { get; set; }
        public string TicketSubject { get; set; }
        public Tenant Tenant { get; set; }
        public Unit Unit { get; set; }
        public Building Building { get; set; }
        public string FirstComment { get; set; }
        public List<MaintenanceCategory> Categories { get; set; }
        public string DueDate { get; set; }
        public int NumberOfReplies { get; set; }
        public bool HasWorkorder { get; set; }
        public bool HasEvent { get; set; }
        public bool HasPet { get; set; }
        public bool HasAccess { get; set; }
        public List<MaintenanceTicketComment> Comments { get; set; }
        [JsonIgnore]
        public string NumberOfRepliesString
        {
            get
            {
                return $"{NumberOfReplies}";
            }
        }
        [JsonIgnore]
        public string CommentImage
        {
            get
            {
                return TicketStatus != "Open" ? "chat_green.png" : "chat_red.png";
            }
        }
        [JsonIgnore]
        public string FormattedDate
        {
            get
            {
                return TicketCreateTime.ToString("MMM dd - h:mm tt");
            }
        }
        [JsonIgnore]
        public string Category
        {
            get
            {
                return Categories.Count > 0 ? Categories.First().CategoryName : "";
            }
        }
        [JsonIgnore]
        public string TenantDetails
        {
            get
            {
                return Tenant == null ? "" : $"{Tenant.TenantFirstName} {Tenant.TenantLastName}, {Building.BuildingName} #{Unit.UnitName}";
            }
        }
        [JsonIgnore]
        public bool FirstCommentShown { get; set; }


    }
}
