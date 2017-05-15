using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TravelApp.Converters;

namespace TravelApp.Models
{
    public class Trip
    {
        public int Id { get; set; }

        public bool IsPublic { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        [JsonConverter(typeof(CustomDateOnlyConverter))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime DateAdd { get; set; }

        [NotMapped]
        public Photo BasicPhoto { get; set; }
        [NotMapped]
        public int StarsCount { get; set; }
        [NotMapped]
        public double StarsAvg { get; set; }
        [NotMapped]
        public Star StarredValue { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<Location> Locations { get; set; }
        public List<Photo> Photos { get; set; }
        public List<Star> Stars { get; set; }
    }
}
