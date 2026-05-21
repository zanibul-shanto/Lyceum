using Lyceum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lyceum.Services;

public class LyceumDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public LyceumDbContext(DbContextOptions<LyceumDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
