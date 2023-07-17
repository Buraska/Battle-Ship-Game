using Microsoft.EntityFrameworkCore;

namespace WebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext (DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<DAL.GameConfigDb> GameConfigDb { get; set; } = null!;
    }
}
