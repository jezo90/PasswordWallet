﻿using Microsoft.EntityFrameworkCore;

namespace PasswordWallet.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<PasswdHistory> PasswdHistories { get; set; }
        public DbSet<ActionType> ActionTypes { get; set; }
        public DbSet<SharedPasswd> SharedPasswds { get; set; }
        public DbSet<Passwd> Passwds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<AddressIP> AddressIPs { get; set; }
    }
}
