using Microsoft.EntityFrameworkCore;
using WebServices.Entities.Models;

namespace WebServices.DataAccess
{
    public class ConnectionContext : DbContext
    {
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public ConnectionContext(DbContextOptions<ConnectionContext> options) : base(options)
        {

        }
        //This method allows to overwrite and configure the BBDD context
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Entity>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
