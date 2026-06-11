using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistance.Context
{
    // Design-time factory for EF tools (migrations, drop-database, etc.)
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<eVote360ProContext>
    {
        public eVote360ProContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<eVote360ProContext>();
            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=eVote360Pro;Trusted_Connection=True;TrustServerCertificate=True";
            optionsBuilder.UseSqlServer(connectionString);
            return new eVote360ProContext(optionsBuilder.Options);
        }
    }
}
