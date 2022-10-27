using BH.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace BH.Data
{
    public class BhDataContext : DbContext
    {

        public BhDataContext(DbContextOptions<BhDataContext> options) : base(options)
        {
        }

        public BhDataContext()
        {

        }

        public DbSet<Book> Books { get; set; }

        public DbSet<Verse> Verses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //optionsBuilder.UseSqlite("Data Source=BH-Data.sqlite"); //UNCOMMENT FOR MIGRATIONS
        }
    }
}
