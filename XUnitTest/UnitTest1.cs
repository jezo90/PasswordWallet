using System;
using System.Reflection.Metadata;
using Xunit;
using PasswordWallet.Models;
using PasswordWallet.Infrastructure;
using NuGet.Frameworks;
using System.Collections.Generic;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Linq;

namespace XUnitTest
{
    public class UnitTest1
    {
        User user = new User()
        {
            Nickname = "test",
            isPasswordKeptAsHash = "SHA512",
            Password = "MlRVDmwySY/N7L8MjqwnTZ7G4B+mHpUuzRtTln1YvyLytqcwr+/PYCQ0q9EC5DSY/wqV40tmcQDEWXJyNWr7X1BvbGx1Yg==", // zahashowane "test"
            Salt = "JPDzbg5KR/s=",
            IsAccountBlocked = false
        };

        User userToRehash = new User()
        {
            Nickname = "testy",
            isPasswordKeptAsHash = "SHA512",
            Password = "GHCKQEc+8WFjaPqq4NBSPC79h/Ls5JNQ8ZT5wtZeSfbldn0L9PUcxFJExOFsyUaCv46Ql9O3cEs5Uu0KtwQK7lBvbGx1Yg==", // zahashowane "testy"
            Salt = "FKZhvi0ndJA=",
            IsAccountBlocked = false
        };

        List<Passwd> passwdToRehash = new List<Passwd>
        {
            new Passwd() { Password = "JO1O5k6mf3RtJ+vEB7tSeQ==" }, // testy
            new Passwd() { Password = "Seq7eCtPce3xHg82u0rwCQ==" } // adam
        };

        User userHMAC = new User()
        {
            Nickname = "123",
            isPasswordKeptAsHash = "HMAC",
            Password = "eeY69/QXJwVinvy1IznoBsC3tFRdgckVGK+d6cznHqI=",
            Salt = "JPDzbg5KR/s=",
            IsAccountBlocked = false
        };

        User UserToRegister = new User()
        {
            Nickname = "nick",
            isPasswordKeptAsHash = "SHA512",
            Password = "Lab1",
            IsAccountBlocked = false
        };

        List<User> usersList = new List<User>
        {
            new User() { Nickname = "Adam"},
            new User() { Nickname = "Adrian"},
            new User() { Nickname = "Test"},
            new User() { Nickname = "Lab1"},
            new User() { Nickname = "Admin"}
        };

        List<Passwd> passwdList = new List<Passwd>
        {
            new Passwd() { Password = "8+CXQe8tSNT4iEkVOQHTdQ==" }, // a1
            new Passwd() { Password = "BWg43BefvihjT7N5jqi5jg==" } // test
        };

        AddressIP addressIP = new AddressIP()
        { 
            Address = "192.168.1.1", 
            Correct = 0, 
            Incorrect = 4, 
            IpBlockDate = DateTime.Now.AddDays(7) 
        };

        User blockedUser = new User()
        {
            Nickname = "test",
            isPasswordKeptAsHash = "SHA512",
            Password = "MlRVDmwySY/N7L8MjqwnTZ7G4B+mHpUuzRtTln1YvyLytqcwr+/PYCQ0q9EC5DSY/wqV40tmcQDEWXJyNWr7X1BvbGx1Yg==", // zahashowane "test"
            Salt = "JPDzbg5KR/s=",
            IsAccountBlocked = true,
            AccountBlockDate = DateTime.Now.AddDays(-7)
        };

        [Fact]
        public void ShouldReturnTrueWhenIpIsBlocked()
        {
            Assert.False(Functions.IsIpBlocked(addressIP));
        }

        [Fact]
        public void ShouldReturnTrueWhenAccountIsBlocked()
        {
            Assert.False(Functions.IsUserBlocked(user));
        }

        [Fact]
        public void ShouldUnblockUserWhenBlockedExpired()
        {
            Assert.True(blockedUser.IsAccountBlocked);
            Functions.UnblockUser(blockedUser);
            Assert.False(blockedUser.IsAccountBlocked);
        }

        [Fact]
        public void ShouldReturnTrueWhenNicknameIsAvailable()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "PasswordWallet").Options;

            using (var mock = new ApplicationDbContext(options))
            {
                mock.Users.Add(user);
                mock.Users.Add(userToRehash);
                mock.Passwds.Add(passwdList[0]);
                mock.Passwds.Add(passwdList[1]);
                mock.SaveChanges();

                Assert.False(Functions.isNickAvailable(mock.Users.ToList(), "test"));
                Assert.True(Functions.isNickAvailable(mock.Users.ToList(), "wolnynick"));
            }

        }

        [Fact]
        public void ShouldReturnTrueWhenUserIsRegisteredRight()
        {
            User CreatedUser = Functions.createUser(UserToRegister);
            Assert.Equal(UserToRegister.Nickname, CreatedUser.Nickname);
            Assert.Equal(UserToRegister.isPasswordKeptAsHash, CreatedUser.isPasswordKeptAsHash);
            Assert.True(Functions.Login(CreatedUser, "Lab1"));
        }

        [Fact]
        public void ShouldLoginWhenUserPassRightPassword()
        {
            Assert.True(Functions.Login(user, "test"));
            Assert.False(Functions.Login(user, "test1"));
        }

        [Fact]
        public void ShouldPasswordDecryptWhenRight()
        {
            string password0 = AESHelper.DecryptToString(Convert.FromBase64String(passwdList[0].Password), user.Password);
            Assert.Equal("a1", password0);
        }

        [Fact]
        public void ShouldEncryptAndDecryptPasswordWhenRight()
        {
            string hashedpassword0 = Convert.ToBase64String(AESHelper.EncryptString("abcd", user.Password));
            string password0 = AESHelper.DecryptToString(Convert.FromBase64String(hashedpassword0), user.Password);
            Assert.Equal("abcd", password0);
        }

        [Fact]
        public void ShouldChangePasswordSHA()
        {
            var userWithChangedPassword = Functions.ChangePasswordSHA("Lab1", user);
            Assert.True(Functions.Login(userWithChangedPassword, "Lab1"));
            Assert.False(Functions.Login(userWithChangedPassword, "test"));
        }

        [Fact]
        public void ShouldChangePasswordHMAC()
        {
            var userWithChangedPassword = Functions.ChangePasswordHMAC("Lab2", userHMAC);
            Assert.True(Functions.Login(userWithChangedPassword, "Lab2"));
            Assert.False(Functions.Login(userWithChangedPassword, "123"));
        }

        [Fact]
        public void ShouldRehashPasswordWhenMasterPasswordChanged()
        {
            string password0 = AESHelper.DecryptToString(Convert.FromBase64String(passwdToRehash[0].Password), userToRehash.Password);
            Assert.Equal("testy", password0);
            string password1 = AESHelper.DecryptToString(Convert.FromBase64String(passwdToRehash[1].Password), userToRehash.Password);
            Assert.Equal("adam", password1);

            var userWithChangedPassword = Functions.ChangePasswordSHA("Lab1", user);
            Assert.True(Functions.Login(userWithChangedPassword, "Lab1"));

            passwdToRehash = AESHelper.rehashPasswds(passwdToRehash, userToRehash.Password, userWithChangedPassword.Password);

            string rehashedpassword0 = AESHelper.DecryptToString(Convert.FromBase64String(passwdToRehash[0].Password), userWithChangedPassword.Password);
            Assert.Equal(password0, rehashedpassword0);
            string rehashedpassword1 = AESHelper.DecryptToString(Convert.FromBase64String(passwdToRehash[1].Password), userWithChangedPassword.Password);
            Assert.Equal(password1, rehashedpassword1);
        }

        [Fact]
        public void ShouldReturnTrueWhenSaltLengthIs8()
        {
            Assert.Equal(8, Functions.GenerateSalt().Length);
        }
    }
}
