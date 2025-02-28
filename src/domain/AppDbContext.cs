using Microsoft.EntityFrameworkCore;

namespace ADAM.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> dbContext)
        : base(dbContext)
    {
        
    }
}
