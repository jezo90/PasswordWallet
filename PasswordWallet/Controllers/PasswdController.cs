using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Infrastructure;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
            string readMode = Functions.getMode(_cache);
            if (readMode == "1")
            {
                return RedirectToAction("EditMode");
            }

            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Error = Functions.getError(_cache, CacheNames.error4)
            };

            _cache.Remove(CacheNames.error4);
            return View(appViewModel);
        }

        public IActionResult SetEditMode()
        {
            _cache.Set(CacheNames.readMode, "1");
            return RedirectToAction("EditMode");
        }

        public IActionResult SetReadMode()
        {
            _cache.Set(CacheNames.readMode, "0");
            return RedirectToAction("Index");
        }

        public IActionResult EditMode()
        {
            string readMode = Functions.getMode(_cache);
            if (readMode != "1")
            {
                return RedirectToAction("Index");
            }

            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Error = Functions.getError(_cache, CacheNames.error4)
            };

            _cache.Remove(CacheNames.error4);
            return View(appViewModel);
        }

        public IActionResult Share(int id)
        {
            User userOwner = Functions.getUser(_cache);
            Passwd passwd = _db.Passwds.Where(a => a.Id == id).FirstOrDefault();

            if (userOwner.Id != passwd.UserId)
            {
                _cache.Set(CacheNames.error4, "You are not the owner");
                return RedirectToAction("Index");
            }

            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Passwd = _db.Passwds.Where(a => a.Id == id).FirstOrDefault(),
                Error = Functions.getError(_cache, CacheNames.error3)
            };
            _cache.Remove(CacheNames.error3);
            return View(appViewModel);
        }

        public IActionResult ShareToUser(string username, int id)
        {
            User userOwner = Functions.getUser(_cache);
            Passwd passwd = _db.Passwds.Where(a => a.Id == id).FirstOrDefault();
            User userToShare = _db.Users.Where(a => a.Nickname == username).FirstOrDefault();
            SharedPasswd shared = _db.SharedPasswds.Where(a => a.PasswdId == id && a.UserSharedId == userToShare.Id).FirstOrDefault();
            if (userToShare == null)
            {
                _cache.Set(CacheNames.error3, "There is no users with that nickname");
                return RedirectToAction("Share", new { id });
            }
            if (shared != null)
            {
                _cache.Set(CacheNames.error3, "You are already sharing password with that user");
                return RedirectToAction("Share", new { id });
            }
            if (username == userOwner.Nickname)
            {
                _cache.Set(CacheNames.error3, "You can't share password with that user");
                return RedirectToAction("Share", new { id });
            }
            if (userOwner.Id == passwd.UserId)
            {
                SharedPasswd passToShare = new SharedPasswd()
                {
                    PasswdId = passwd.Id,
                    UserOwnerId = userOwner.Id,
                    UserSharedId = userToShare.Id
                };
                _db.SharedPasswds.Add(passToShare);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
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
            User usr = Functions.getUser(_cache);
            if (toDecrypt.UserId != usr.Id)
            {
                var sharedPass = _db.SharedPasswds.Where(a => a.UserSharedId == usr.Id).ToList();
                List<int> ids = new List<int>();

                foreach (SharedPasswd sPass in sharedPass)
                {
                    ids.Add(sPass.PasswdId);
                }

                if (!ids.Contains(toDecrypt.Id))
                {
                    return RedirectToAction("Index");
                }
            }

            ActionType actionType = new ActionType()
            {
                Action = "Decrypt password id=" + id,
                UserId = usr.Id,
                Time = DateTime.Now
            };
            Functions.AddActionToDatabase(_db, actionType);

            var decrypt = Convert.FromBase64String(toDecrypt.Password); // covnvert string into byte[] to decrypt 
            var passwordOwner = _db.Users.Where(a => a.Id == toDecrypt.UserId).FirstOrDefault();
            toDecrypt.Password = AESHelper.DecryptToString(decrypt, passwordOwner.Password);  // decrypting password

            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Passwd = toDecrypt
            };

            return View(appViewModel);
        }

        public IActionResult EditPassword(int id)
        {
            if (_cache.Get(CacheNames.getMasterPassword).ToString() != "1") // ask user for masterpassword first time
            {
                return RedirectToAction("MasterPassword");
            }
            Passwd toEdit = _db.Passwds.Where(a => a.Id == id).FirstOrDefault(); // get password to decrypt by id
            User usr = Functions.getUser(_cache);
            if (toEdit.UserId != usr.Id)
            {
                _cache.Set(CacheNames.error4, "You can't edit that password");
                return RedirectToAction("Index");
            }
            var edit = Convert.FromBase64String(toEdit.Password); // covnvert string into byte[] to decrypt 
            var passwordOwner = _db.Users.Where(a => a.Id == toEdit.UserId).FirstOrDefault();
            toEdit.Password = AESHelper.DecryptToString(edit, passwordOwner.Password);  // decrypting password

            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Passwd = toEdit
            };

            return View(appViewModel);
        }

        public IActionResult History(int id)
        {
            Passwd toEdit = _db.Passwds.Where(a => a.Id == id).FirstOrDefault(); // get password to decrypt by id
            User usr = Functions.getUser(_cache);
            if (toEdit.UserId != usr.Id)
            {
                _cache.Set(CacheNames.error4, "You can't check history");
                return RedirectToAction("Index");
            }

            AppViewModel appViewModel = new AppViewModel
            {
                User = Functions.getUser(_cache),
                Logged = Functions.getLogged(_cache),
                Passwd = toEdit
            };

            return View(appViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit()
        {
            var password = _db.Passwds.Where(a => a.Id == Passwd.Id).FirstOrDefault();

            if (password.UserId == Functions.getUser(_cache).Id)
            {
                if (ModelState.IsValid)
                {
                    var oldPassword = password.Password;
                    var masterPassword = _cache.Get(CacheNames.masterPassword).ToString(); // get current user masterpassword
                    var encrypted = AESHelper.EncryptString(Passwd.Password, masterPassword); // encrypt with masterpassword
                    password.Password = Convert.ToBase64String(encrypted); // add encypted password as a string to variable
                    password.WebAddress = Passwd.WebAddress;
                    password.Login = Passwd.Login;

                    ActionType actionType = new ActionType()
                    {
                        Action = "Edit password id=" + password.Id,
                        UserId = password.UserId,
                        Time = DateTime.Now
                    };
                    Functions.AddActionToDatabase(_db, actionType);

                    PasswdHistory passwdHistory = new PasswdHistory()
                    {
                        NewPasswd = password.Password,
                        OldPasswd = oldPassword,
                        Time = DateTime.Now,
                        UserId = password.UserId,
                        PasswdId = password.Id
                    };


                    Functions.AddHistoryToDatabase(_db, passwdHistory);

                    _db.SaveChanges(); // save database
                    return RedirectToAction("Index");
                }
                return View(Passwd);
            }
            _cache.Set(CacheNames.error4, "You are not the owner");
            return RedirectToAction("Index");
        }

        public IActionResult Recover(int id, int passwordId)
        {
            var password = _db.Passwds.Where(a => a.Id == passwordId).FirstOrDefault();
            var passToUpdate = _db.PasswdHistories.Where(a => a.Id == id).FirstOrDefault();

            if (password.UserId == Functions.getUser(_cache).Id)
            {
                PasswdHistory passwdHistory = new PasswdHistory()
                {
                    NewPasswd = passToUpdate.OldPasswd,
                    OldPasswd = password.Password,
                    Time = DateTime.Now,
                    UserId = password.UserId,
                    PasswdId = password.Id
                };
                Functions.AddHistoryToDatabase(_db, passwdHistory);

                password.Password = passToUpdate.OldPasswd;


                _db.SaveChanges(); // save database
                return RedirectToAction("Index");
            }
            _cache.Set(CacheNames.error4, "You are not the owner");
            return RedirectToAction("Index");
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

                ActionType actionType = new ActionType()
                {
                    Action = "Add password to database",
                    UserId = Passwd.UserId,
                    Time = DateTime.Now
                };
                Functions.AddActionToDatabase(_db, actionType);


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
            var data1 = await _db.Passwds.Where(a => a.UserId == usr.Id).ToListAsync();
            var data2 = await _db.SharedPasswds.Where(a => a.UserSharedId == usr.Id).ToListAsync();
            foreach (SharedPasswd pass in data2)
            {
                data1.Add(_db.Passwds.Where(a => a.Id == pass.PasswdId).FirstOrDefault());
            }
            return Json(new { data = data1.ToList() }); // return his passwords
        }

        [HttpGet]
        public async Task<IActionResult> GetOne(int id)
        {
            User usr = Functions.getUser(_cache); // get logged user 
            var data1 = await _db.PasswdHistories.Where(a => a.PasswdId == id).ToListAsync();
            List<PasswdTimes> passwdTimes = new List<PasswdTimes>();
            foreach (PasswdHistory passwd in data1)
            {
                PasswdTimes passwdTime = new PasswdTimes()
                {
                    Id = passwd.Id,
                    NewPasswd = passwd.NewPasswd,
                    OldPasswd = passwd.OldPasswd,
                    PasswdId = passwd.PasswdId,
                    Date = passwd.Time.ToString("dddd, dd MMMM yyyy HH:mm:ss"),
                };

                passwdTimes.Add(passwdTime);
            }
            return Json(new { data = passwdTimes.ToList() }); // return his history
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            User usr = Functions.getUser(_cache); // get logged user 
            var passwdFromDb = await _db.Passwds.FirstOrDefaultAsync(a => a.Id == id); // get password with delivered id 
            SharedPasswd sharedPassword = _db.SharedPasswds.Where(a => a.PasswdId == passwdFromDb.Id &&
                                                                  a.UserSharedId == usr.Id).FirstOrDefault();

            if (passwdFromDb.UserId == usr.Id)
            {
                if (passwdFromDb == null)
                {
                    return Json(new { success = false, message = "Error while deleting" });
                }
                _db.Passwds.Remove(passwdFromDb);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Delete successful" });
            }
            else if (sharedPassword.UserSharedId == usr.Id)
            {
                if (sharedPassword == null)
                {
                    return Json(new { success = false, message = "Error while deleting" });
                }
                _db.SharedPasswds.Remove(sharedPassword);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Password is no longer shared" });
            }
            return Json(new { success = false, message = "Error while deleting" });
        }

        [HttpDelete]
        public async Task<IActionResult> Read(int id)
        {
            return Json(new { success = false, message = "You have to switch to the edit mode to modify" });
        }
        #endregion
    }
}
