using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Serialization;
using PasswordWallet.Infrastructure;
using PasswordWallet.Models;

namespace PasswordWallet.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IMemoryCache _cache;
        [BindProperty]
        public AppViewModel App { get; set; }

        public AccountController(IMemoryCache memoryCache, ApplicationDbContext db)
        {
            _cache = memoryCache;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache)
            };
            return View(appViewModel);
        }

        public IActionResult ResetPassword()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache)
            };
            return View(appViewModel);
        }

        public IActionResult ChangePassword()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache)
            };
            return View(appViewModel);
        }

        public IActionResult Register()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache)
            };
            return View(appViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reg()
        {
            var usersList = _db.Users.ToList();
            if (Functions.isNickAvailable(usersList, App.User.Nickname))
            {
                _db.Users.Add(Functions.createUser(App.User));
                _db.SaveChanges();
                return RedirectToAction("Login");
            }

            return RedirectToAction("Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Signin()
        {
            if (_db.Users.Where(a => a.Nickname == App.User.Nickname).FirstOrDefault<User>() is User user)
            {
                if (Functions.Login(user, App.User.Password))
                {
                    _cache.Set(CacheNames.user, user);  // set cache variables 
                    _cache.Set(CacheNames.logged, "1");
                    _cache.Set(CacheNames.masterPassword, user.Password);
                    _cache.Set(CacheNames.getMasterPassword, "0");
                    return RedirectToAction("Index", "Home");
                }
            }
            AppViewModel appViewModel = new AppViewModel
            {
                User = App.User,
                Logged = Functions.getLogged(_cache)
            };
            return View("Login", appViewModel);
        }

        public IActionResult Logout()
        {
            _cache.Remove(CacheNames.user);
            _cache.Remove(CacheNames.logged);
            _cache.Remove(CacheNames.masterPassword);
            _cache.Remove(CacheNames.getMasterPassword);
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Change(string currentPassword, string newPassword)
        {
            User currentUser = Functions.getUser(_cache);
            var userToChange = _db.Users.Where(a => a.Id == currentUser.Id).FirstOrDefault();
            if (currentUser.isPasswordKeptAsHash == "SHA512")
            {
                if (Functions.Login(currentUser, currentPassword))
                {
                    userToChange = Functions.ChangePasswordSHA(newPassword, userToChange);

                    _cache.Set(CacheNames.user, userToChange);
                    _cache.Set(CacheNames.masterPassword, userToChange.Password);
                    _cache.Set(CacheNames.getMasterPassword, "0");

                    // rehash passwords
                    List<Passwd> passwds = _db.Passwds.Where(a => a.UserId == currentUser.Id).ToList();
                    passwds = AESHelper.rehashPasswds(passwds, currentUser.Password, userToChange.Password);

                    _db.SaveChanges();
                    return RedirectToAction("Index", "Passwd");
                }
            }
            else
            {
                if (Functions.Login(currentUser, currentPassword))
                {
                    userToChange = Functions.ChangePasswordHMAC(newPassword, userToChange);

                    List<Passwd> passwds = _db.Passwds.Where(a => a.UserId == currentUser.Id).ToList();
                    passwds = AESHelper.rehashPasswds(passwds, currentUser.Password, userToChange.Password);

                    _cache.Set(CacheNames.user, userToChange);
                    _cache.Set(CacheNames.masterPassword, userToChange.Password);
                    _cache.Set(CacheNames.getMasterPassword, "0");

                    _db.SaveChanges();
                    return RedirectToAction("Index", "Passwd");
                }
            }

            return RedirectToAction("ChangePassword");
        }
    }
}
