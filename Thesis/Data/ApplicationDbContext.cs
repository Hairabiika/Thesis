using Microsoft.EntityFrameworkCore;

namespace Thesis.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Models.Diplom> Diplom { get; set; }
        public DbSet<ViewModels.Users> Users { get; set; }
    }
}
