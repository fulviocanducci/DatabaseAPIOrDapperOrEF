using CslAppDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace CslAppDatabase.EF
{
    public class EFDatabase : DbContext
    {
        public EFDatabase(DbContextOptions options) : base(options)
        {

        }

        public DbSet<People> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
        }
    }
}
