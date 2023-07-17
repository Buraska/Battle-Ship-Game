using Microsoft.EntityFrameworkCore;

namespace DAL
{

    public class ApplicationDbContext : DbContext
    {

        public DbSet<GameConfigDb?> GameConfigDb { get; set; } = default!;

        public DbSet<GameStateDb?> GameStateDbs { get; set; } = default!;

        private static string ConnectionString  = "";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}