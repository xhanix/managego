using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class PMCBuilding
    {

        public int BuildingId { get; set; }
        public string BuildingDescription { get; set; }
        public IEnumerable<Amenity> Amenities { get; set; }

    }
}
