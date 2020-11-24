using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Infrastructure;
using PasswordWallet.Models;

namespace PasswordWallet.Controllers
{
    public class IpController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IMemoryCache _cache;
        [BindProperty]
        public AppViewModel App { get; set; }

        public IpController(IMemoryCache memoryCache, ApplicationDbContext db)
        {
            _cache = memoryCache;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UnblockIP()
        {
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            AddressIP address = _db.AddressIPs.Where(a => a.Address == ip).FirstOrDefault();
            if (address == null)
            {
                address = new AddressIP()
                {
                    Address = ip,
                    Correct = 0,
                    Incorrect = 0
                };
                _db.AddressIPs.Add(address);
            }
            address = Functions.UnblockIP(address);
            _db.SaveChanges();

            return RedirectToAction("Login", "Account");
        }
    }
}
