using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        //API switched to placing amenities in buildings. No need to get buildings from amenities.
        // public IEnumerable<Building> Buildings { get; set; }
        public AmenityRules Rules { get; set; }
    }
}
