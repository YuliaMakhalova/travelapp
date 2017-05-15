using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.ViewModels
{
    public class TripCreateViewModel
    {
        [Required(ErrorMessage = "Необходимо добавить основную фотографию")]
        public IFormFile ImageFile { get; set; }

        [Required(ErrorMessage = "Не указан тип")]
        public bool IsPublic { get; set; }

        [Required(ErrorMessage = "Не указано название поездки")]
        public string Title { get; set; }

        public string Description { get; set; }
    }
}
