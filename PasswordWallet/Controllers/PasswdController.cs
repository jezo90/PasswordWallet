using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Infrastructure;
using PasswordWallet.Models;

namespace PasswordWallet.Controllers
{
    public class PasswdController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IMemoryCache _cache;
        [BindProperty]
        public Passwd Passwd { get; set; }
        [BindProperty]
        public AppViewModel App { get; set; }
        public PasswdController(IMemoryCache memoryCache, ApplicationDbContext db)
        {
            _db = db;
            _cache = memoryCache;
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

        public IActionResult Upsert()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Passwd = new Passwd()
            };
            return View(appViewModel);
        }

        public IActionResult MasterPassword()
        {
            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Passwd = new Passwd()
            };
            return View(appViewModel);
        }

        public IActionResult Decrypt(int id)
        {
            if (_cache.Get(CacheNames.getMasterPassword).ToString() != "1") // ask user for masterpassword first time
            {
                return RedirectToAction("MasterPassword");
            }
            Passwd toDecrypt = _db.Passwds.Where(a => a.Id == id).FirstOrDefault(); // get password to decrypt by id
            var decrypt = Convert.FromBase64String(toDecrypt.Password); // covnvert string into byte[] to decrypt 
            toDecrypt.Password = AESHelper.DecryptToString(decrypt, _cache.Get(CacheNames.masterPassword).ToString());  // decrypting password

            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Passwd = toDecrypt
            };

            return View(appViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Master(string master)
        {
            var masterCheck = _cache.Get(CacheNames.masterPassword).ToString();
            var currentUser = Functions.getUser(_cache);

            if (_db.Users.Where(a => a.Nickname == currentUser.Nickname).FirstOrDefault<User>() is User user)
            {
                if (user.isPasswordKeptAsHash == "SHA512")
                {
                    var salt = Functions.StringToBytes(user.Salt);
                    var pepper = Functions.StringToBytes(CacheNames.pepper);
                    var passWithSalt = Functions.SHA512(master, salt);
                    var passWithSaltAndPepper = Functions.SHA512(passWithSalt, pepper);

                    if (passWithSaltAndPepper == masterCheck)
                    {
                        _cache.Set(CacheNames.getMasterPassword, "1");
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    var salt = Functions.StringToBytes(user.Salt);
                    HMAC hmac = new HMACSHA256(salt);
                    if (masterCheck == Functions.GenerateHMAC(master, hmac))
                    {
                        _cache.Set(CacheNames.getMasterPassword, "1");
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("MasterPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add()
        {
            if (ModelState.IsValid)
            {
                Passwd.UserId = Functions.getUser(_cache).Id;   // get logged user
                var masterPassword = _cache.Get(CacheNames.masterPassword).ToString(); // get current user masterpassword
                var encrypted = AESHelper.EncryptString(Passwd.Password, masterPassword); // encrypt with masterpassword
                Passwd.Password = Convert.ToBase64String(encrypted); // add encypted password as a string to variable 
                _db.Passwds.Add(Passwd); // add var to database
                _db.SaveChanges(); // save database
                return RedirectToAction("Index");
            }

            return View(Passwd);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            User usr = Functions.getUser(_cache); // get logged user 
            return Json(new { data = await _db.Passwds.Where(a => a.UserId == usr.Id).ToListAsync() }); // return his passwords
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var passwdFromDb = await _db.Passwds.FirstOrDefaultAsync(a => a.Id == id); // get password with delivered id 
            if (passwdFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _db.Passwds.Remove(passwdFromDb);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
