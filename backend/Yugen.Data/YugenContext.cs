using Microsoft.EntityFrameworkCore;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Media;

namespace Yugen.Data;

public class YugenContext : DbContext
{
    public YugenContext(DbContextOptions<YugenContext> options) : base(options) { }

    public DbSet<Model_Media> media { get; set; }
    public DbSet<Model_MediaEpisode> mediaEpisodes { get; set; }

    // from sonnar
    public DbSet<Model_DownloadedMedia> downloadedMedia { get; set; }

    public DbSet<UserModel> user { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model_MediaEpisode>().HasKey(e => new { e.MediaId, e.EpisodeNumber });
        modelBuilder.Entity<Model_MediaEpisode>().HasOne(e => e.Media).WithMany(e => e.Episodes);

        modelBuilder.Entity<Model_Media>()
            .HasOne(m => m.Successor)
            .WithOne(m => m.Predecessor)
            .HasForeignKey<Model_Media>(m => m.SuccessorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
