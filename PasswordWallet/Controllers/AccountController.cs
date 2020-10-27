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
            if (_db.Users.Where(a => a.Nickname == App.User.Nickname).FirstOrDefault<User>() == null)
            {
                var salt = Functions.GenerateSalt();    // generate salt
                var salt1 = Functions.StringToBytes(Functions.BytesToString(salt));
                App.User.Salt = Functions.BytesToString(salt);  // set user salt
                if (App.User.isPasswordKeptAsHash == "SHA512")
                {
                    var pepper = Functions.StringToBytes(CacheNames.pepper);    // get pepper 
                    var passWithSalt = Functions.SHA512(App.User.Password, salt1);  // hash wit salt 
                    var passWithSaltAndPepper = Functions.SHA512(passWithSalt, pepper); // hash with pepper
                    App.User.Password = passWithSaltAndPepper;  // set hashed with sha user password
                }
                else if (App.User.isPasswordKeptAsHash == "HMAC")
                {
                    HMAC hmac = new HMACSHA256(salt1);  //  hash hmac
                    App.User.Password = Functions.GenerateHMAC(App.User.Password, hmac); // set hased with hmac user password 
                }
                else
                {
                    return RedirectToAction("Register");
                }
                _db.Users.Add(App.User);
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
                if (user.isPasswordKeptAsHash == "SHA512")
                {
                    var salt = Functions.StringToBytes(user.Salt);  
                    var pepper = Functions.StringToBytes(CacheNames.pepper);    
                    var passWithSalt = Functions.SHA512(App.User.Password, salt);
                    var passWithSaltAndPepper = Functions.SHA512(passWithSalt, pepper);

                    if (user.Password == passWithSaltAndPepper)
                    {
                        _cache.Set(CacheNames.user, user);  // set cache variables 
                        _cache.Set(CacheNames.logged, "1");
                        _cache.Set(CacheNames.masterPassword, user.Password);
                        _cache.Set(CacheNames.getMasterPassword, "0");
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    var salt = Functions.StringToBytes(user.Salt);
                    HMAC hmac = new HMACSHA256(salt);
                    if (user.Password == Functions.GenerateHMAC(App.User.Password, hmac))
                    {
                        _cache.Set(CacheNames.user, user);
                        _cache.Set(CacheNames.logged, "1");
                        _cache.Set(CacheNames.masterPassword, user.Password);
                        _cache.Set(CacheNames.getMasterPassword, "0");
                        return RedirectToAction("Index", "Home");
                    }
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
            if (currentUser.isPasswordKeptAsHash == "SHA512")
            {
                var salt = Functions.StringToBytes(currentUser.Salt);
                var pepper = Functions.StringToBytes(CacheNames.pepper);
                var passWithSalt = Functions.SHA512(currentPassword, salt);
                var passWithSaltAndPepper = Functions.SHA512(passWithSalt, pepper);

                if (currentUser.Password == passWithSaltAndPepper)
                {
                    var newsalt = Functions.GenerateSalt();
                    var newsalt1 = Functions.StringToBytes(Functions.BytesToString(newsalt));
                    var newPassWithSalt = Functions.SHA512(newPassword, newsalt1);
                    var newPassWithSaltAndPepper = Functions.SHA512(newPassWithSalt, pepper);
                    var userToChange = _db.Users.Where(a => a.Id == currentUser.Id).FirstOrDefault();
                    userToChange.Salt = Functions.BytesToString(newsalt);
                    userToChange.Password = newPassWithSaltAndPepper;
                    _cache.Set(CacheNames.user, userToChange);
                    _cache.Set(CacheNames.masterPassword, newPassWithSaltAndPepper);
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
                var salt = Functions.StringToBytes(currentUser.Salt);
                HMAC hmac = new HMACSHA256(salt);
                if (currentUser.Password == Functions.GenerateHMAC(currentPassword, hmac))
                {
                    var newsalt = Functions.GenerateSalt();
                    var newsalt1 = Functions.StringToBytes(Functions.BytesToString(newsalt));
                    HMAC newhmac = new HMACSHA256(newsalt1);
                    string passwordToChange = Functions.GenerateHMAC(newPassword, newhmac);

                    List<Passwd> passwds = _db.Passwds.Where(a => a.UserId == currentUser.Id).ToList();
                    passwds = AESHelper.rehashPasswds(passwds, currentUser.Password, passwordToChange);

                    var userToChange = _db.Users.Where(a => a.Id == currentUser.Id).FirstOrDefault();
                    userToChange.Salt = Functions.BytesToString(newsalt);
                    userToChange.Password = passwordToChange;
                    _cache.Set(CacheNames.user, userToChange);
                    _cache.Set(CacheNames.masterPassword, passwordToChange);
                    _cache.Set(CacheNames.getMasterPassword, "0");

                    _db.SaveChanges();
                    return RedirectToAction("Index", "Passwd");
                }
            }

            return RedirectToAction("ChangePassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reset(string nickname, string newPassword)
        {
            User currentUser = _db.Users.Where(a => a.Nickname == nickname).FirstOrDefault<User>();
            var pepper = Functions.StringToBytes(CacheNames.pepper);
            if (currentUser.isPasswordKeptAsHash == "SHA512")
            {
                var newsalt = Functions.GenerateSalt();
                var newsalt1 = Functions.StringToBytes(Functions.BytesToString(newsalt));
                var newPassWithSalt = Functions.SHA512(newPassword, newsalt1);
                var newPassWithSaltAndPepper = Functions.SHA512(newPassWithSalt, pepper);

                List<Passwd> passwds = _db.Passwds.Where(a => a.UserId == currentUser.Id).ToList();
                passwds = AESHelper.rehashPasswds(passwds, currentUser.Password, newPassWithSaltAndPepper);

                var userToChange = _db.Users.Where(a => a.Id == currentUser.Id).FirstOrDefault();
                userToChange.Salt = Functions.BytesToString(newsalt);
                userToChange.Password = newPassWithSaltAndPepper;

                _db.SaveChanges();
            }
            else
            {
                var newsalt = Functions.GenerateSalt();
                var newsalt1 = Functions.StringToBytes(Functions.BytesToString(newsalt));
                HMAC newhmac = new HMACSHA256(newsalt1);
                string nPass = Functions.GenerateHMAC(newPassword, newhmac);

                List<Passwd> passwds = _db.Passwds.Where(a => a.UserId == currentUser.Id).ToList();
                passwds = AESHelper.rehashPasswds(passwds, currentUser.Password, nPass);

                var userToChange = _db.Users.Where(a => a.Id == currentUser.Id).FirstOrDefault();
                userToChange.Salt = Functions.BytesToString(newsalt);
                userToChange.Password = nPass;

                _db.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
