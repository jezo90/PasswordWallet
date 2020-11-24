using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Infrastructure;
using PasswordWallet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordWallet.Controllers
{
    public class AttemptController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IMemoryCache _cache;
        public string error;
        public string error2;
        [BindProperty]
        public AppViewModel App { get; set; }

        public AttemptController(IMemoryCache memoryCache, ApplicationDbContext db)
        {
            _cache = memoryCache;
            _db = db;
        }

        public IActionResult Index()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache)
            };
            return View(appViewModel);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            User usr = Functions.getUser(_cache); // get logged user
            var attempts = await _db.LoginAttempts.Where(a => a.UserId == usr.Id).OrderByDescending(a => a.Date).ToListAsync();
            List<LoginHistory> loginHistories = new List<LoginHistory>();
            foreach (LoginAttempt login in attempts)
            {
                LoginHistory history = new LoginHistory()
                {
                    AddressIp = login.AddressIp,
                    Date = login.Date.ToString("dddd, dd MMMM yyyy"),
                    Time = login.Date.ToString("HH:mm:ss"),
                };
                if (login.Successful)
                    history.Successful = "Successful";
                else
                    history.Successful = "Error";
                loginHistories.Add(history);
            }

            return Json(new { data = loginHistories }); // return his login attempts
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            User usr = Functions.getUser(_cache); // get logged user 
            var attemptsFromDb = await _db.LoginAttempts.Where(a => a.UserId == usr.Id).ToListAsync(); // get password with delivered id 
            if (attemptsFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            foreach (LoginAttempt login in attemptsFromDb)
            {
                _db.LoginAttempts.Remove(login);
            }
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion

    }
}
