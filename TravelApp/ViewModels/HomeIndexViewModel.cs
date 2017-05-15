using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelApp.Models;

namespace TravelApp.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Trip> Trips { get; set; }

        public List<string> SliderPhotos { get; set; }
    }
}
