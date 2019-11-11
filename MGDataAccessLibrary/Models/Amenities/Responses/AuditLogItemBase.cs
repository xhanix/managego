using System;
namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class AuditLogItemBase
    {
        public DateTime Date { get; set; }
        public AuditLogUser User { get; set; }
    }
}
