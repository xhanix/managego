using System;
using System.Collections.Generic;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class PMCUserInfo
    {
        public IEnumerable<PMCBuilding> BuildingsAccess { get; set; }

    }

    public class PMCInfo
    {
        /// <summary>
        /// Time Zone offset (hours)
        /// </summary>
        public int TimeZoneOffset { get; set; }
        public string CompanyUrl { get; set; }
    }
}
