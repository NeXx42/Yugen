using Microsoft.EntityFrameworkCore;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;

namespace Yugen.Data;

public class YugenContext : DbContext
{
    public YugenContext(DbContextOptions<YugenContext> options) : base(options) { }

    public DbSet<Model_Media> media { get; set; }
    public DbSet<Model_MediaEpisode> mediaEpisodes { get; set; }
    public DbSet<Model_Link> links { get; set; }

    // from sonnar
    public DbSet<Model_DownloadedMedia> downloadedMedia { get; set; }
    public DbSet<Model_DownloadedEpisode> sonarrEpisodes { get; set; }

    public DbSet<UserModel> user { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model_MediaEpisode>().HasKey(e => new { e.MediaId, e.EpisodeNumber });
        modelBuilder.Entity<Model_MediaEpisode>().HasOne(e => e.Media).WithMany(e => e.Episodes);

        modelBuilder.Entity<Model_DownloadedEpisode>().HasKey(e => new { e.MediaId, e.EpisodeNumber });
        modelBuilder.Entity<Model_DownloadedEpisode>().HasOne(e => e.DownloadedMedia).WithMany(e => e.downloadedEpisodes);
    }
}
