using Microsoft.EntityFrameworkCore;

namespace BankSimple.Infrastructure.Persistence;

public class BankSimpleDbContext : DbContext
{
    public BankSimpleDbContext(DbContextOptions<BankSimpleDbContext> options)
        : base(options) { }
}