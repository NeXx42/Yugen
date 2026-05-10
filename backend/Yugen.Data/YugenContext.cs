using Microsoft.EntityFrameworkCore;
using Yugen.Domain.Models;

namespace Yugen.Data;

public class YugenContext : DbContext
{
    public YugenContext(DbContextOptions<YugenContext> options) : base(options) { }

    public DbSet<MediaModel> media { get; set; }
    public DbSet<MediaExternalProviderModel> mediaExternalProvider { get; set; }

    public DbSet<UserModel> user { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaExternalProviderModel>().HasKey(x => new { x.MediaId, x.ProviderType });
        modelBuilder.Entity<MediaExternalProviderModel>().HasOne(x => x.Media).WithMany(x => x.externalProviders).HasForeignKey(x => x.MediaId);
    }
}
