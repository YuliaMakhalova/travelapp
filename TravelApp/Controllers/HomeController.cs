using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelApp.ViewModels;
using TravelApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TravelApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly TravelAppContext db;
        public HomeController(TravelAppContext context)
        {
            this.db = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeIndexViewModel();

            viewModel.SliderPhotos = new List<string> {
                "images/templatemo_slide_1.jpg",
                "images/templatemo_slide_2.jpg",
                "images/templatemo_slide_3.jpg"
            };

            List<Trip> trips = await db.Trips
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.Id).Take(15).ToListAsync();
            foreach (var t in trips)
            {
                t.BasicPhoto = await db.Photos.FirstOrDefaultAsync(p => p.Trip == t && p.IsBasic == true);
            }

            viewModel.Trips = trips;

            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
