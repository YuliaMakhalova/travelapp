using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Models
{
    public class Photo
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public bool IsBasic { get; set; }

        public int TripId { get; set; }
        public Trip Trip { get; set; }
        
        public Location Location { get; set; }
        public Event Event { get; set; }
    }
}
