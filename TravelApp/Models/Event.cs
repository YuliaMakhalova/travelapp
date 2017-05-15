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
    public class Event
    {
        public int Id { get; set; }

        public bool IsPublic { get; set; }

        public int Position { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string EventType { get; set; }

        public string Address { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int MapZoom { get; set; }

        [JsonConverter(typeof(CustomDateOnlyConverter))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime DateAdd { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime DateUpdate { get; set; }
        
        [NotMapped]
        public double StarsAvg { get; set; }
        [NotMapped]
        public Star StarredValue { get; set; }

        public List<Photo> Photos { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Star> Stars { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}
