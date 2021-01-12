using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Infrastructure;
using PasswordWallet.Models;

namespace PasswordWallet.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IMemoryCache _cache;
        [BindProperty]
        public AppViewModel App { get; set; }

        public HistoryController(IMemoryCache memoryCache, ApplicationDbContext db)
        {
            _cache = memoryCache;
            _db = db;
        }

        public IActionResult ActionList()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache)
            };
            return View(appViewModel);

            return View();
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAllActions()
        {
            User usr = Functions.getUser(_cache); // get logged user 
            var data1 = await _db.ActionTypes.Where(a => a.UserId == usr.Id).ToListAsync();
            List<ActionTime> actionTimes = new List<ActionTime>();
            foreach (ActionType action in data1)
            {
                ActionTime actionTime = new ActionTime()
                {
                    Id = action.Id,
                    Action = action.Action,
                    Date = action.Time.ToString("dddd, dd MMMM yyyy HH:mm:ss"),
                    UserId = action.UserId
                };

                actionTimes.Add(actionTime);
            }
            return Json(new { data = actionTimes.ToList() }); // return his actions
        }

        #endregion
    }
}