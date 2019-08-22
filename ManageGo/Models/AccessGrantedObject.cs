using System;
using System.Collections.Generic;

namespace ManageGo.Models
{
    public class AccessGrantedObject
    {
        public int PetInUnit { get; set; }
        public string CustomDescription { get; set; }
        public string Note { get; set; }
        public int AccessGrantedType { get; set; }
        public IEnumerable<AccessGrantedDates> Dates { get; set; }
    }
}
