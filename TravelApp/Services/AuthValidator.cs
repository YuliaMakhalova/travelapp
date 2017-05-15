using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelApp.Models;

namespace TravelApp.Services
{
    public static class AuthValidator
    {
        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            TravelAppContext db = context.HttpContext.RequestServices.GetRequiredService<TravelAppContext>();

            if (context.Principal.Identity.IsAuthenticated)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == context.Principal.Identity.Name);
                if (user == null) context.RejectPrincipal();
            }
        }
    }
}
