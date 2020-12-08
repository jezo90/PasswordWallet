using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Infrastructure;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PasswordWallet.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IMemoryCache _cache;
        public string error;
        public string error2;
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
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            AddressIP address = _db.AddressIPs.Where(a => a.Address == ip).FirstOrDefault();

            if (address == null)
            {
                address = new AddressIP()
                {
                    Address = ip,
                    Correct = 0,
                    Incorrect = 1
                };
                _db.AddressIPs.Add(address);
            }

            if (Functions.IsIpBlocked(address))
            {
                if (_db.Users.Where(a => a.Nickname == App.User.Nickname).FirstOrDefault<User>() is User user)
                {
                    var lastAttempt = _db.LoginAttempts.Where(a => a.AddressIp == ip && a.UserId == user.Id)
                                                        .OrderByDescending(a => a.Date).FirstOrDefault();
                    if (lastAttempt == null)
                    {
                        lastAttempt = new LoginAttempt()
                        { Attempt = 0 };
                    }

                    if (Functions.IsUserBlocked(user))
                    {
                        user = Functions.UnblockUser(user);
                    }

                    if (Functions.Login(user, App.User.Password))
                    {
                        LoginAttempt loginAttempt = new LoginAttempt()
                        {
                            UserId = user.Id,
                            AddressIp = ip,
                            Attempt = 0,
                            Date = DateTime.Now,
                            Successful = true
                        };

                        address.Correct = 1;
                        address.Incorrect = 0;

                        _db.LoginAttempts.Add(loginAttempt);
                        _db.SaveChanges();

                        _cache.Set(CacheNames.readMode, "0");  // set cache variables 
                        _cache.Set(CacheNames.user, user);  
                        _cache.Set(CacheNames.logged, "1");
                        _cache.Set(CacheNames.masterPassword, user.Password);
                        _cache.Set(CacheNames.getMasterPassword, "0");
                        return RedirectToAction("Index", "Home");
                    }
                    else if (Functions.IsUserBlocked(user))
                    {
                        error = "\n\nKonto jest zablokowane do " + user.AccountBlockDate;
                    }
                    else
                    {
                        LoginAttempt failedLoginAttempt = new LoginAttempt()
                        {
                            UserId = user.Id,
                            AddressIp = ip,
                            Attempt = lastAttempt.Attempt + 1,
                            Date = DateTime.Now,
                            Successful = false
                        };
                        switch (failedLoginAttempt.Attempt)
                        {
                            case 1: break;
                            case 2: user.AccountBlockDate = DateTime.Now.AddSeconds(15); user.IsAccountBlocked = true; break;
                            case 3: user.AccountBlockDate = DateTime.Now.AddSeconds(30); user.IsAccountBlocked = true; break;
                            case 4: user.AccountBlockDate = DateTime.Now.AddMinutes(2); user.IsAccountBlocked = true; break;
                            default: user.AccountBlockDate = DateTime.Now.AddYears(30); user.IsAccountBlocked = true; break;
                        }

                        address.Incorrect++;
                        switch (address.Incorrect)
                        {
                            case 1: break;
                            case 2: break;
                            case 3: break;
                            case 4: break;
                            case 5: address.IpBlockDate = DateTime.Now.AddSeconds(15); break;
                            case 6: address.IpBlockDate = DateTime.Now.AddSeconds(30); break;
                            default: address.IpBlockDate = DateTime.Now.AddMinutes(1); break;
                        }

                        _db.LoginAttempts.Add(failedLoginAttempt);
                        _db.SaveChanges();

                        if (Functions.IsUserBlocked(user))
                        {
                            error = "Konto jest zablokowane do " + user.AccountBlockDate;
                        }
                    }
                }
            }
            if (address.IpBlockDate > DateTime.Now)
            {
                error2 = "Twoje IP jest zablokowane do " + address.IpBlockDate;
            }


            AppViewModel appViewModel = new AppViewModel
            {
                User = App.User,
                Logged = Functions.getLogged(_cache),
                Error = error,
                Error2 = error2
            };
            return View("Login", appViewModel);
        }

        public IActionResult Logout()
        {
            _cache.Remove(CacheNames.readMode);
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
