using Microsoft.EntityFrameworkCore;

namespace PasswordWallet.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Passwd> Passwds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<AddressIP> AddressIPs { get; set; }
    }
}
