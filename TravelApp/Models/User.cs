using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(126)]
        public string Name { get; set; }

        [MaxLength(126)]
        public string Surname { get; set; }
        
        public string Email { get; set; }
        
        public string Password { get; set; }

        public string AvatarUrl { get; set; }

        public List<Trip> Trips { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Star> Stars { get; set; }
    }                                              
}
