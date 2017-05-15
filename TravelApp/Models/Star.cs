using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TravelApp.Converters;

namespace TravelApp.Models
{
    public class Star
    {
        public int Id { get; set; }

        public int Value { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime DateAdd { get; set; }

        public Trip Trip { get; set; }
        public Location Location { get; set; }
        public Event Event { get; set; }
        public User User { get; set; }
    }
}
