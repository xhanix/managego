using System;
namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class AuditLog
    {
        public AuditLogCreator Creator { get; set; }
        public AuditLogLastModifier LastModifier { get; set; }
    }
}
