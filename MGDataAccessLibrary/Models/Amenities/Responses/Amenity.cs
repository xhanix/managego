using System;
using System.Collections.Generic;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public IEnumerable<Building> Buildings { get; set; }

    }
}
