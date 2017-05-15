using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Services
{
    public interface IToolsService
    {
        string RandomString(int length);

        string GetHash(string input);
    }
}
