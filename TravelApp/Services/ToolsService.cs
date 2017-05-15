using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TravelApp.Services
{
    public class ToolsService : IToolsService
    {
        Random random = new Random();

        public string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            StringBuilder pwd = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                pwd.Append(hash[i].ToString("x2"));
            return pwd.ToString();
        }

        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
