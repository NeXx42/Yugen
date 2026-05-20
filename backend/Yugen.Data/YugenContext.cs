using Microsoft.EntityFrameworkCore;
using Yugen.Domain.Enums;
using Yugen.Domain.Models;
using Yugen.Domain.Models.Bookmarks;
using Yugen.Domain.Models.History;
using Yugen.Domain.Models.Library;
using Yugen.Domain.Models.Linking;
using Yugen.Domain.Models.Media;

namespace Yugen.Data;

public class YugenContext : DbContext
{
    public YugenContext(DbContextOptions<YugenContext> options) : base(options) { }

    public DbSet<Model_Config> config { get; set; }

    // Site
    public DbSet<Model_Tag> tags { get; set; }
    public DbSet<Model_Genre> genres { get; set; }

    // from sonnar
    public DbSet<Model_Media> media { get; set; }
    public DbSet<Model_MediaEpisode> mediaEpisodes { get; set; }
    public DbSet<Model_Link> links { get; set; }

    // from sonnar
    public DbSet<Model_DownloadedMedia> downloadedMedia { get; set; }
    public DbSet<Model_DownloadedEpisode> sonarrEpisodes { get; set; }

    // History
    public DbSet<Model_WatchHistory> watchHistory { get; set; }
    public DbSet<Model_WatchedEpisode> watchedEpisodes { get; set; }

    // Bookmarks
    public DbSet<Model_Bookmark> bookmarkTypes { get; set; }
    public DbSet<Model_UserBookmark> userBookmarks { get; set; }

    public DbSet<UserModel> user { get; set; }
    public DbSet<Model_Notification> notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model_Config>().HasKey(m => m.Key);

        modelBuilder.Entity<Model_MediaEpisode>().HasKey(e => new { e.MediaId, e.EpisodeNumber });
        modelBuilder.Entity<Model_MediaEpisode>().HasOne(e => e.Media).WithMany(e => e.Episodes);

        modelBuilder.Entity<Model_DownloadedEpisode>().HasKey(e => new { e.MediaId, e.EpisodeNumber });
        modelBuilder.Entity<Model_DownloadedEpisode>().HasOne(e => e.DownloadedMedia).WithMany(e => e.downloadedEpisodes);

        modelBuilder.Entity<Model_WatchedEpisode>().HasKey(w => new { w.MediaId, w.EpisodeNumber });
        modelBuilder.Entity<Model_WatchedEpisode>().HasOne(w => w.WatchedHistory).WithMany(w => w.WatchedEpisodes);

        modelBuilder.Entity<Model_UserBookmark>().HasKey(e => new { e.MediaId, e.UserId });

        modelBuilder.Entity<Model_Bookmark>().HasData(
            new Model_Bookmark { Id = (int)BookmarkType.Watching, Title = "Watching" },
            new Model_Bookmark { Id = (int)BookmarkType.OnHold, Title = "OnHold" },
            new Model_Bookmark { Id = (int)BookmarkType.Planning, Title = "Planning" },
            new Model_Bookmark { Id = (int)BookmarkType.Completed, Title = "Completed" },
            new Model_Bookmark { Id = (int)BookmarkType.Dropped, Title = "Dropped" }
        );
    }
}
