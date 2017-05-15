using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelApp.Models;
using TravelApp.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using TravelApp.Services;

namespace TravelApp.Controllers
{
    public class AccountController : Controller
    {
        private TravelAppContext db;
        private IToolsService _tools;
        private IImageService _imageService;

        public AccountController(TravelAppContext context, IToolsService tools, IImageService imageService)
        {
            db = context;
            _tools = tools;
            _imageService = imageService;
        }
        
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null)
                return await this.Logout();

            return View(new AccountProfileViewModel(user));
        }   

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AccountProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get user current info
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
                if (user == null)
                    return await this.Logout();

                // Save new avatar, if uploaded
                if (model.AvatarFile != null)
                {
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"avatars");
                    var avatarUrl = await _imageService.SaveImage(model.AvatarFile, uploadDir);
                    user.AvatarUrl = avatarUrl == "error" ? "" : avatarUrl;
                }

                // Check password change
                if (model.Password != null)
                {
                    var newPwd = _tools.GetHash(model.Password);
                    if (user.Password != newPwd)
                        user.Password = newPwd;
                }

                // Update name and surname
                user.Name = model.Name;
                user.Surname = model.Surname;

                await db.SaveChangesAsync();

                return View(new AccountProfileViewModel(user));
            } else return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == _tools.GetHash(model.Password));
                if (user != null)
                {
                    await Authenticate(model.Email);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("login", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    var newUser = new User
                    {
                        Email = model.Email,
                        Password = _tools.GetHash(model.Password),
                        Name = model.Name,
                        Surname = model.Surname
                    };

                    if (model.AvatarFile != null)
                    {
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot", @"avatars");
                        var avatarUrl = await _imageService.SaveImage(model.AvatarFile, uploadDir);
                        newUser.AvatarUrl = avatarUrl == "error" ? "" : avatarUrl;
                    }

                    db.Users.Add(newUser);
                    await db.SaveChangesAsync();

                    await Authenticate(model.Email);

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("email", "Введенный email уже существует");
            }
            return View(model);
        }

        private async Task Authenticate(string email)
        {
            var claims = new List<Claim> {
                new Claim(ClaimsIdentity.DefaultNameClaimType, email)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.Authentication.SignInAsync("Cookies", new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            return RedirectToAction("Login", "Account");
        }
    }
}
