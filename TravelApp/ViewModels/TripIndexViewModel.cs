using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelApp.Models;

namespace TravelApp.ViewModels
{
    public class TripIndexViewModel
    {
        public List<Trip> Trips { get; set; }

        public int PageCount { get; set; }

        public int PageCurrent { get; set; }
    }
}
