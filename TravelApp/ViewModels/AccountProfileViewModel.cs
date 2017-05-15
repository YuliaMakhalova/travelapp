using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TravelApp.Models;

namespace TravelApp.ViewModels
{
    public class AccountProfileViewModel
    {
        public AccountProfileViewModel() {}

        public AccountProfileViewModel(User user)
        {
            this.AvatarUrl = user.AvatarUrl;
            this.Name = user.Name;
            this.Surname = user.Surname;
        }

        public string AvatarUrl { get; set; }

        public IFormFile AvatarFile { get; set; }

        [Required(ErrorMessage = "Не указано имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указана фамилия")]
        public string Surname { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get; set; }
    }
}
