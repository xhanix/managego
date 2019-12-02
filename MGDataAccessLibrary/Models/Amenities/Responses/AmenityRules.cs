using System;
namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class AmenityRules
    {
        public int Id { get; set; }
        public double FeeValue { get; set; }
        public double SecurityDepositAmount { get; set; }
        public bool IsSecurityDepositEnabled { get; set; }
        public string Description { get; set; }
    }
}
