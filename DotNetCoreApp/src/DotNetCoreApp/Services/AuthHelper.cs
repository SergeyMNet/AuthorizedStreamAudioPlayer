using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace DotNetCoreApp.Services
{
    public static class AuthHelper
    {
        public static string Login { get; set; }
        public static string Password { get; set; }
        public static IMemoryCache MemoryCache { get; set; }
    }
}
