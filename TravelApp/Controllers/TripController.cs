using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TravelApp.ViewModels;
using TravelApp.Services;
using TravelApp.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Pioneer.Pagination;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace TravelApp.Controllers
{
    public class TripController : Controller
    {
        private readonly TravelAppContext db;
        private readonly IImageService _imageService;

        public TripController(TravelAppContext context, IImageService imageService)
        {
            db = context;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            return await GetTripList(page, TripListType.Index);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyTrips(int? page)
        {
            return await GetTripList(page, TripListType.MyTrip);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            var model = new TripCreateViewModel {
                IsPublic = true
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
                if (user == null)
                    return RedirectToAction("Logout", "Account");

                var model = new Trip();

                if (viewModel.ImageFile != null)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                    var photoUrl = await _imageService.SaveImage(viewModel.ImageFile, uploadDir);

                    if (photoUrl != "error")
                    {
                        var pathToImage = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                        var minUploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos", @"min");
                        _imageService.SaveMinifiedImage(photoUrl, pathToImage, minUploadDir, 300);

                        Photo photo = new Photo
                        {
                            IsBasic = true,
                            Trip = model,
                            Url = photoUrl
                        };
                        db.Photos.Add(photo);

                        model.Photos.Add(photo);
                    }
                }

                model.Title = viewModel.Title;
                model.Description = viewModel.Description;
                model.IsPublic = viewModel.IsPublic;
                model.User = user;
                model.DateAdd = DateTime.Now;

                db.Trips.Add(model);
                await db.SaveChangesAsync();

                return RedirectToAction("Overview", new { id = model.Id });
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Overview(int id)
        {
            Trip trip = await db.Trips.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null || trip != null && 
                (!User.Identity.IsAuthenticated || User.Identity.IsAuthenticated && User.Identity.Name != trip.User.Email) && 
                trip.IsPublic == false)
                return NotFound();

            string allowEditing = User.Identity.IsAuthenticated && User.Identity.Name == trip.User.Email? "true" : "false";

            ViewData["authenticated"] = User.Identity.IsAuthenticated ? "true" : "false";
            ViewData["allow_editing"] = allowEditing;
            ViewData["trip_id"] = id;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TripInfo(int id)
        {
            Trip trip = await db.Trips
                .Include(t => t.Photos)
                .Include(t => t.Locations)
                .Include(t => t.Locations).ThenInclude(l => l.Photos)
                .Include(t => t.Locations).ThenInclude(l => l.Comments)
                .Include(t => t.Locations).ThenInclude(l => l.Comments).ThenInclude(c => c.User)
                .Include(t => t.Locations).ThenInclude(l => l.Events)
                .Include(t => t.Locations).ThenInclude(l => l.Events).ThenInclude(e => e.Photos)
                .Include(t => t.Locations).ThenInclude(l => l.Events).ThenInclude(e => e.Comments)
                .Include(t => t.Locations).ThenInclude(l => l.Events).ThenInclude(e => e.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null)
                return NotFound();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == trip.UserId);
            if (user != null)
                trip.User = new User {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    AvatarUrl = user.AvatarUrl
                };

            try
            {
                trip.StarsAvg = await db.Stars.Where(s => s.Trip == trip).AverageAsync(s => s.Value);
                trip.StarsCount = await db.Stars.CountAsync(s => s.Trip == trip);
                if (User.Identity.IsAuthenticated)
                {
                    trip.StarredValue = await db.Stars
                            .Include(s => s.User)
                            .FirstOrDefaultAsync(s => s.Trip == trip && s.User.Email == User.Identity.Name);
                }
                else trip.StarredValue = null;
            }
            catch (Exception)
            {
                trip.StarsAvg = 0;
                trip.StarsCount = 0;
                trip.StarredValue = null;
            }

            if (trip.Locations != null && trip.Locations.Count > 0)
            {
                foreach (var l in trip.Locations)
                {
                    try
                    {
                        l.StarsAvg = await db.Stars.Where(s => s.Location == l).AverageAsync(s => s.Value);
                        if (User.Identity.IsAuthenticated)
                        {
                            l.StarredValue = await db.Stars
                                .Include(s => s.User)
                                .FirstOrDefaultAsync(s => s.Location == l && s.User.Email == User.Identity.Name);
                        }
                        else l.StarredValue = null;
                    }
                    catch (Exception)
                    {
                        l.StarsAvg = 0;
                        l.StarredValue = null;
                    }

                    if (l.Events != null && l.Events.Count > 0)
                    {
                        foreach (var e in l.Events)
                        {
                            try
                            {
                                e.StarsAvg = await db.Stars.Where(s => s.Event == e).AverageAsync(s => s.Value);
                                if (User.Identity.IsAuthenticated)
                                {
                                    e.StarredValue = await db.Stars
                                        .Include(s => s.User)
                                        .FirstOrDefaultAsync(s => s.Event == e && s.User.Email == User.Identity.Name);
                                }
                                else e.StarredValue = null;
                            }
                            catch (Exception)
                            {
                                e.StarsAvg = 0;
                                e.StarredValue = null;
                            }
                        }
                    }
                }
            }

            return Json(trip);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrip(int id, [FromBody] Trip trip)
        {
            Trip dbTrip = await db.Trips.FirstOrDefaultAsync(l => l.Id == id);

            if (dbTrip != null)
            {
                dbTrip.IsPublic = trip.IsPublic;
                dbTrip.Title = trip.Title;
                dbTrip.Description = trip.Description;
                await db.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            Trip trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == id);

            if (trip != null)
            {

                var locationPhotos = await db.Photos
                    .Include(p => p.Location)
                    .Where(p => p.Location.TripId == trip.Id)
                    .ToListAsync();
                var eventPhotos = await db.Photos
                    .Include(p => p.Event)
                    .Include(p => p.Event).ThenInclude(e => e.Location)
                    .Where(p => p.Event.Location.TripId == trip.Id)
                    .ToListAsync();
                db.Photos.RemoveRange(locationPhotos);
                db.Photos.RemoveRange(eventPhotos);

                var locationComments = await db.Comments
                    .Include(c => c.Location)
                    .Where(c => c.Location.TripId == trip.Id)
                    .ToListAsync();
                var eventComments = await db.Comments
                    .Include(c => c.Event)
                    .Include(c => c.Event).ThenInclude(e => e.Location)
                    .Where(c => c.Event.Location.TripId == trip.Id)
                    .ToListAsync();
                db.Comments.RemoveRange(locationComments);
                db.Comments.RemoveRange(eventComments);

                var tripStars = await db.Stars.Where(s => s.Trip == trip).ToListAsync();
                var locationStars = await db.Stars.Include(s => s.Location).Where(s => s.Location.TripId == trip.Id).ToListAsync();
                var eventStars = await db.Stars
                    .Include(s => s.Event)
                    .Include(s => s.Event).ThenInclude(e => e.Location)
                    .Where(s => s.Event.Location.TripId == trip.Id)
                    .ToListAsync();

                db.Stars.RemoveRange(tripStars);
                db.Stars.RemoveRange(locationStars);
                db.Stars.RemoveRange(eventStars);

                db.Trips.Remove(trip);
                await db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        public class CommentDeleteDTO { public int commentId { get; set; } }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveComment(int id, [FromBody] CommentDeleteDTO data)
        {
            Comment comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == data.commentId);

            if (comment != null)
            {
                db.Comments.Remove(comment);
                await db.SaveChangesAsync();
                return Ok();
            }
            else return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPhotos(int id)
        {
            Trip trip = await db.Trips.Include(t => t.Photos).FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            var photos = new List<Photo>();
            foreach (var file in Request.Form.Files)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                var photoUrl = await _imageService.SaveImage(file, uploadDir);

                if (photoUrl != "error")
                {
                    var pathToImage = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                    var minUploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos", @"min");
                    _imageService.SaveMinifiedImage(photoUrl, pathToImage, minUploadDir, 300);

                    Photo photo = new Photo
                    {
                        IsBasic = false,
                        Trip = trip,
                        Url = photoUrl
                    };
                    trip.Photos.Add(photo);
                    db.Photos.Add(photo);
                    photos.Add(photo);
                }
            }

            await db.SaveChangesAsync();

            return Json(photos);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetBasicPhoto(int id, [FromBody] Photo photo)
        {
            Photo basicPhoto = await db.Photos.FirstOrDefaultAsync(p => p.IsBasic == true && p.TripId == id);

            if (basicPhoto != null)
            {
                basicPhoto.IsBasic = false;
            }

            Photo reqPhoto = await db.Photos.FirstOrDefaultAsync(p => p.Id == photo.Id);
            if (reqPhoto != null)
            {
                reqPhoto.IsBasic = true;
                await db.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeletePhoto(int id, [FromBody] Photo photo)
        {
            Photo dbPhoto = await db.Photos.FirstOrDefaultAsync(p => p.Id == photo.Id);

            if (dbPhoto != null)
            {
                db.Photos.Remove(dbPhoto);
                await db.SaveChangesAsync();
            }

            return Ok();
        }

        public class TripStarDTO { public int value { get; set; } }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddStarRecord(int id, [FromBody] TripStarDTO data)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            Trip trip = await db.Trips.FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null || user == null) return BadRequest();

            Star star = new Star {
                Trip = trip,
                User = user,
                Value = data.value,
                DateAdd = DateTime.Now
            };

            db.Stars.Add(star);
            await db.SaveChangesAsync();

            return Ok();
        }

        public class SortingDTO
        {
            public class Event
            {
                public int? eventId { get; set; }
                public int position { get; set; }
            }

            public class Location
            {
                public int? locationId { get; set; }
                public int position { get; set; }
                public List<Event> events { get; set; }
            }

            public List<Location> sorting { get; set; }
        }
        public async Task<IActionResult> SaveSortingState(int id, [FromBody] SortingDTO data)
        {
            Trip trip = await db.Trips
                                .Include(t => t.Locations)
                                .Include(t => t.Locations).ThenInclude(l => l.Events)
                                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            foreach (var location in data.sorting)
            {
                if (location.locationId == null) continue;

                var dbLocation = trip.Locations.FirstOrDefault(l => l.Id == location.locationId.Value);
                if (dbLocation == null) continue;

                dbLocation.Position = location.position;

                foreach (var sevent in location.events)
                {
                    if (sevent.eventId == null) continue;

                    var dbevent = dbLocation.Events.FirstOrDefault(e => e.Id == sevent.eventId.Value);
                    if (dbevent == null) continue;

                    dbevent.Position = sevent.position;
                }
            }

            await db.SaveChangesAsync();

            return Ok();
        }



        #region Location actions
        [HttpGet]
        [Authorize]
        public IActionResult DefaultLocation()
        {
            Location defaultLocation = new Location
            {
                IsPublic = true,
                Position = 0,
                PlaceTitle = "Новое местоположение",
                Description = "Введите описание местоположения",
                Address = "",
                Country = "",
                State = "",
                City = "",
                Latitude = 0,
                Longitude = 0,
                ArrivalDate = DateTime.Now,
                DateAdd = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            return Json(defaultLocation);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateLocation(int id, [FromBody] Location location)
        {
            location.TripId = id;
            location.DateAdd = DateTime.Now;
            location.DateUpdate = DateTime.Now;
            await db.Locations.AddAsync(location);
            await db.SaveChangesAsync();

            return Json(location);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveLocation(int id, [FromBody] Location location)
        {
            Location dbLocation = await db.Locations.FirstOrDefaultAsync(l => l.Id == location.Id);

            if (dbLocation != null)
            {
                dbLocation.IsPublic = location.IsPublic;
                dbLocation.Position = location.Position;
                dbLocation.PlaceTitle = location.PlaceTitle;
                dbLocation.Description = location.Description;
                dbLocation.Address = location.Address;
                dbLocation.Country = location.Country;
                dbLocation.State = location.State;
                dbLocation.City = location.City;
                dbLocation.Latitude = location.Latitude;
                dbLocation.Longitude = location.Longitude;
                dbLocation.MapZoom = location.MapZoom;
                dbLocation.ArrivalDate = location.ArrivalDate;
                dbLocation.DateUpdate = DateTime.Now;
                await db.SaveChangesAsync();
            }

            return Json(dbLocation);
        }

        public class LocationDeleteDTO { public int locationId { get; set; } }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteLocation(int id, [FromBody] LocationDeleteDTO data)
        {
            Location location = await db.Locations.FirstOrDefaultAsync(l => l.TripId == id && l.Id == data.locationId);

            if (location != null)
            {
                var locationComments = await db.Comments.Where(c => c.Location == location).ToListAsync();
                var eventComments = await db.Comments
                    .Include(c => c.Event)
                    .Where(c => c.Event.LocationId == location.Id)
                    .ToListAsync();
                db.Comments.RemoveRange(locationComments);
                db.Comments.RemoveRange(eventComments);

                List<Photo> photos = await db.Photos
                    .Include(p => p.Event)
                    .Where(c => c.Location == location || c.Event.LocationId == location.Id).ToListAsync();
                photos.ForEach(p => {
                    p.Location = null;
                    p.Event = null;
                });

                var locationStars = await db.Stars.Where(s => s.Location == location).ToListAsync();
                var eventStars = await db.Stars
                    .Include(s => s.Event)
                    .Where(s => s.Event.LocationId == location.Id)
                    .ToListAsync();
                db.Stars.RemoveRange(locationStars);
                db.Stars.RemoveRange(eventStars);

                db.Locations.Remove(location);
                await db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        public class LocationCommentDTO {
            public int locationId { get; set; }
            public string comment { get; set; }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddLocationComment(int id, [FromBody] LocationCommentDTO data)
        {
            Location location = await db.Locations.FirstOrDefaultAsync(l => l.Id == data.locationId);
            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user != null && location != null)
            {
                Comment comment = new Comment {
                    Location = location,
                    User = user,
                    Content = data.comment,
                    DateAdd = DateTime.Now
                };
                await db.Comments.AddAsync(comment);
                await db.SaveChangesAsync();
                return Json(comment);
            }
            else return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddLocationPhotos(int id)
        {
            var locationIdStr = Request.Form.ContainsKey("locationId") ? Request.Form["locationId"].ToString() : "";
            if (locationIdStr == "") return BadRequest();

            var locationId = 0;
            if (!int.TryParse(locationIdStr, out locationId)) return BadRequest();

            Trip trip = await db.Trips.Include(t => t.Photos).FirstOrDefaultAsync(t => t.Id == id);
            if (trip == null) return NotFound();

            Location location = await db.Locations.FirstOrDefaultAsync(l => l.Id == locationId);
            if (location == null) return NotFound();

            var photos = new List<Photo>();
            foreach (var file in Request.Form.Files)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                var photoUrl = await _imageService.SaveImage(file, uploadDir);

                if (photoUrl != "error")
                {
                    var pathToImage = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                    var minUploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos", @"min");
                    _imageService.SaveMinifiedImage(photoUrl, pathToImage, minUploadDir, 300);

                    Photo photo = new Photo
                    {
                        IsBasic = false,
                        Trip = trip,
                        Location = location,
                        Url = photoUrl
                    };
                    trip.Photos.Add(photo);
                    db.Photos.Add(photo);
                    photos.Add(photo);
                }
            }

            await db.SaveChangesAsync();

            return Json(new {
                locationId = locationId,
                photos = photos
            });
        }

        public class LocationStarDTO {
            public int value { get; set; }
            public int locationId { get; set; }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddLocationStarRecord(int id, [FromBody] LocationStarDTO data)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            Location location = await db.Locations.FirstOrDefaultAsync(l => l.Id == data.locationId);

            if (location == null || user == null) return BadRequest();

            Star star = new Star
            {
                Location = location,
                User = user,
                Value = data.value,
                DateAdd = DateTime.Now
            };

            db.Stars.Add(star);
            await db.SaveChangesAsync();

            return Ok();
        }
        #endregion



        #region Event actions
        [HttpGet]
        [Authorize]
        public IActionResult DefaultEvent(string type)
        {
            Event defaultEvent = new Event
            {
                IsPublic = true,
                Position = 0,
                Title = "Новое событие",
                Description = "Введите описание события",
                EventType = type,
                Address = "",
                Latitude = 0,
                Longitude = 0,
                EventDate = DateTime.Now,
                DateAdd = DateTime.Now,
                DateUpdate = DateTime.Now
            };

            return Json(defaultEvent);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent(int id, [FromBody] Event newEvent)
        {
            newEvent.DateAdd = DateTime.Now;
            newEvent.DateUpdate = DateTime.Now;
            await db.Events.AddAsync(newEvent);
            await db.SaveChangesAsync();

            return Json(newEvent);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveEvent(int id, [FromBody] Event ev)
        {
            Event dbEvent = await db.Events.FirstOrDefaultAsync(l => l.Id == ev.Id);

            if (dbEvent != null)
            {
                dbEvent.IsPublic = ev.IsPublic;
                dbEvent.Position = ev.Position;
                dbEvent.Title = ev.Title;
                dbEvent.Description = ev.Description;
                dbEvent.Address = ev.Address;
                dbEvent.Latitude = ev.Latitude;
                dbEvent.Longitude = ev.Longitude;
                dbEvent.MapZoom = ev.MapZoom;
                dbEvent.EventDate = ev.EventDate;
                dbEvent.DateUpdate = DateTime.Now;
                await db.SaveChangesAsync();
            }

            return Json(dbEvent);
        }

        public class EventDeleteDTO { public int eventId { get; set; }  }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(int id, [FromBody] EventDeleteDTO data)
        {
            Event dbev = await db.Events.FirstOrDefaultAsync(l => l.Id == data.eventId);

            if (dbev != null)
            {
                List<Comment> comments = await db.Comments.Where(c => c.Event == dbev).ToListAsync();
                db.Comments.RemoveRange(comments);

                List<Photo> photos = await db.Photos.Where(c => c.Event == dbev).ToListAsync();
                photos.ForEach(p => {
                    p.Event = null;
                });

                List<Star> stars = await db.Stars.Where(s => s.Event == dbev).ToListAsync();
                db.Stars.RemoveRange(stars);

                db.Events.Remove(dbev);
                await db.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        public class EventCommentDTO
        {
            public int eventId { get; set; }
            public string comment { get; set; }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddEventComment(int id, [FromBody] EventCommentDTO data)
        {
            Event dbevent = await db.Events.FirstOrDefaultAsync(l => l.Id == data.eventId);
            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user != null && dbevent != null)
            {
                Comment comment = new Comment
                {
                    Event = dbevent,
                    User = user,
                    Content = data.comment,
                    DateAdd = DateTime.Now
                };
                await db.Comments.AddAsync(comment);
                await db.SaveChangesAsync();
                return Json(comment);
            }
            else return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddEventPhotos(int id)
        {
            var eventIdStr = Request.Form.ContainsKey("eventId") ? Request.Form["eventId"].ToString() : "";
            if (eventIdStr == "") return BadRequest();

            var eventId = 0;
            if (!int.TryParse(eventIdStr, out eventId)) return BadRequest();

            Trip trip = await db.Trips.Include(t => t.Photos).FirstOrDefaultAsync(t => t.Id == id);
            if (trip == null) return NotFound();

            Event dbevent = await db.Events.FirstOrDefaultAsync(l => l.Id == eventId);
            if (dbevent == null) return NotFound();

            var photos = new List<Photo>();
            foreach (var file in Request.Form.Files)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                var photoUrl = await _imageService.SaveImage(file, uploadDir);

                if (photoUrl != "error")
                {
                    var pathToImage = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos");
                    var minUploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"photos", @"min");
                    _imageService.SaveMinifiedImage(photoUrl, pathToImage, minUploadDir, 300);

                    Photo photo = new Photo
                    {
                        IsBasic = false,
                        Trip = trip,
                        Event = dbevent,
                        Url = photoUrl
                    };
                    trip.Photos.Add(photo);
                    db.Photos.Add(photo);
                    photos.Add(photo);
                }
            }

            await db.SaveChangesAsync();

            return Json(new
            {
                locationId = dbevent.LocationId,
                eventId = eventId,
                photos = photos
            });
        }

        public class EventStarDTO
        {
            public int value { get; set; }
            public int eventId { get; set; }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddEventStarRecord(int id, [FromBody] EventStarDTO data)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            Event dbevent = await db.Events.FirstOrDefaultAsync(l => l.Id == data.eventId);

            if (dbevent == null || user == null) return BadRequest();

            Star star = new Star
            {
                Event = dbevent,
                User = user,
                Value = data.value,
                DateAdd = DateTime.Now
            };

            db.Stars.Add(star);
            await db.SaveChangesAsync();

            return Ok();
        }
        #endregion



        #region Helpers

        private enum TripListType {
            Index, MyTrip
        };

        private async Task<IActionResult> GetTripList(int? page, TripListType type)
        {
            var count = 0;
            switch (type)
            {
                case TripListType.Index:
                    ViewData["Page"] = "trips";
                    count = await db.Trips.CountAsync(t => t.IsPublic == true);
                    break;
                case TripListType.MyTrip:
                    ViewData["Page"] = "mytrips";
                    count = await db.Trips.CountAsync(t => t.IsPublic == true && t.User.Email == User.Identity.Name);
                    break;
            }

            var offset = 0;
            var elementsPerPage = 9;
            var totalPages = (int)Math.Ceiling((double)count / elementsPerPage);

            if (page.HasValue)
            {
                if (page.Value > totalPages || page.Value < 0)
                    return NotFound();

                offset = (page.Value - 1) * elementsPerPage;
            }

            List<Trip> trips = new List<Trip>();

            switch (type)
            {
                case TripListType.Index:
                    ViewData["LPage"] = "trips";
                    ViewData["LTitle"] = "Все поездки";
                    trips = await db.Trips
                        .Include(t => t.Photos)
                        .Where(t => t.IsPublic)
                        .OrderByDescending(t => t.Id)
                        .Skip(offset)
                        .Take(elementsPerPage)
                        .ToListAsync();
                    break;
                case TripListType.MyTrip:
                    ViewData["LPage"] = "mytrips";
                    ViewData["LTitle"] = "Мои поездки";
                    if (User.Identity.IsAuthenticated)
                    {
                        trips = await db.Trips
                            .Include(t => t.Photos)
                            .Where(t => t.User.Email == User.Identity.Name)
                            .OrderByDescending(t => t.Id)
                            .Skip(offset)
                            .Take(elementsPerPage)
                            .ToListAsync();
                    }
                    else return NotFound();
                    break;
            }

            foreach (var t in trips)
            {
                t.BasicPhoto = await db.Photos.FirstOrDefaultAsync(p => p.Trip == t && p.IsBasic == true);
            }
            
            var viewModel = new TripIndexViewModel
            {
                Trips = trips,
                PageCount = totalPages,
                PageCurrent = page.HasValue ? page.Value : 1
            };

            return View("Index", viewModel);
        }

        #endregion

    }
}
