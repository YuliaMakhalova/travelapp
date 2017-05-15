using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Converters
{
    public class CustomDateOnlyConverter : IsoDateTimeConverter
    {
        public CustomDateOnlyConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
