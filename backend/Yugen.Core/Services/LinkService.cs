using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml.Linq;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Yugen.Core.Factories;
using Yugen.Core.Helpers;
using Yugen.Data;
using Yugen.Domain.Data.Users;
using Yugen.Domain.Enums;
using Yugen.Domain.Interfaces;
using Yugen.Domain.Models.Linking;

namespace Yugen.Core.Services;

public class LinkService
{
    private static bool handlingImport = false;

    private readonly YugenContext _db;
    private readonly EndpointDeduplicator _endpointDeduplicator;

    private readonly LibraryFactory _library;

    public LinkService(YugenContext db, SettingsCache settings, ILogging logger)
    {
        _db = db;
        _endpointDeduplicator = new EndpointDeduplicator();

        _library = LibraryFactory.Create(settings, logger);
    }

    public async Task SaveManualLink(UserSession usr, LibraryProviderType provider, int mediaId, int linkedId, int? linkedSeason)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(RedownloadLinks));
        Model_Link? existingLink = await _db.links.FirstOrDefaultAsync(l => l.MediaId == mediaId);

        if (existingLink == null)
        {
            existingLink = new Model_Link() { MediaId = mediaId };
            _library.GetFactory(provider).EmbedLink(existingLink, linkedId, linkedSeason);

            await _db.AddAsync(existingLink);
        }
        else
        {
            _library.GetFactory(provider).EmbedLink(existingLink, linkedId, linkedSeason);
            await _db.SaveChangesAsync();
        }
    }

    public async Task RedownloadLinks(UserSession usr, bool force = false)
    {
        using var concurrentCheck = _endpointDeduplicator.TryAcquire(usr, nameof(RedownloadLinks));

        try
        {
            await RedownloadLinks(force);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task RedownloadLinks(bool force = false)
    {
        if (handlingImport)
            return;

        handlingImport = true;

        try
        {
            Model_Link[]? links = await new LinkDownloader_OfflineList().Download();

            if (links == null)
                throw new Exception("No links found!");

            Dictionary<int, Model_Link> existingLinks = await _db.links.ToDictionaryAsync(l => l.MediaId, l => l);
            Dictionary<int, int> anilistLinkLookup = existingLinks.Values.Where(e => e.anilist_id.HasValue).ToDictionary(l => l.anilist_id!.Value, l => l.MediaId);
            Dictionary<int, int> malLinkLookup = existingLinks.Values.Where(e => e.mal_id.HasValue).ToDictionary(l => l.mal_id!.Value, l => l.MediaId);

            ConcurrentBag<Model_Link> linkUpdates = new();
            ConcurrentBag<Model_Link> linkAdditions = new();

            Parallel.ForEach(links, (l) =>
            {
                if (!l.anilist_id.HasValue && !l.mal_id.HasValue)
                    return;

                Model_Link? existingLink = null;

                if (l.anilist_id.HasValue && anilistLinkLookup.TryGetValue(l.anilist_id.Value, out int anilistMap))
                {
                    existingLink = existingLinks[anilistMap];
                }
                else if (l.mal_id.HasValue && malLinkLookup.TryGetValue(l.mal_id.Value, out int malMap))
                {
                    existingLink = existingLinks[malMap];
                }

                if (existingLink != null)
                {
                    existingLink.anilist_id ??= l.anilist_id;
                    existingLink.type ??= l.type;
                    existingLink.anidb_id ??= l.anidb_id;
                    existingLink.animecountdown_id ??= l.animecountdown_id;
                    existingLink.animenewsnetwork_id ??= l.animenewsnetwork_id;
                    existingLink.anime_planet_id ??= l.anime_planet_id;
                    existingLink.anisearch_id ??= l.anisearch_id;
                    existingLink.imdb_id ??= l.imdb_id;
                    existingLink.kitsu_id ??= l.kitsu_id;
                    existingLink.livechart_id ??= l.livechart_id;
                    existingLink.mal_id ??= l.mal_id;
                    existingLink.simkl_id ??= l.simkl_id;
                    existingLink.themoviedb_id ??= l.themoviedb_id;
                    existingLink.tvdb_id ??= l.tvdb_id;
                    existingLink.tvdb_season ??= l.tvdb_season;
                    existingLink.tmdb_season ??= l.tmdb_season;

                    linkUpdates.Add(existingLink);
                }
                else
                {
                    linkAdditions.Add(l);
                }
            });

            await _db.BulkUpdateAsync(linkUpdates);
            await _db.BulkInsertAsync(linkAdditions);
        }
        finally
        {
            handlingImport = false;
        }
    }

    private abstract class LinkDownloader
    {
        public abstract Task<Model_Link[]?> Download();
    }

    private class LinkDownloader_Fribb : LinkDownloader
    {
        public override async Task<Model_Link[]?> Download()
        {
            int? anilist_id;
            string? type;
            int? anidb_id;
            int? animecountdown_id;
            int? animenewsnetwork_id;
            string? anime_planet_id;
            int? anisearch_id;
            string? imdb_id;
            int? kitsu_id;
            int? livechart_id;
            int? mal_id;
            int? simkl_id;
            int? tvdb_id;

            int? tvdbSeason;
            int? tmdbSeason;

            int? tmdbId;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage res = await client.GetAsync("https://raw.githubusercontent.com/Fribb/anime-lists/refs/heads/master/anime-list-full.json");
                res.EnsureSuccessStatusCode();

                List<Model_Link> newLinks = new List<Model_Link>();

                try
                {
                    using (Stream stream = await res.Content.ReadAsStreamAsync())
                    using (JsonDocument doc = await JsonDocument.ParseAsync(stream))
                    {
                        try
                        {
                            foreach (JsonElement element in doc.RootElement.EnumerateArray())
                            {
                                element.ExtractInt(nameof(anilist_id), out anilist_id);
                                element.ExtractString(nameof(type), out type);
                                element.ExtractInt(nameof(anidb_id), out anidb_id);
                                element.ExtractInt(nameof(animecountdown_id), out animecountdown_id);
                                element.ExtractInt(nameof(animenewsnetwork_id), out animenewsnetwork_id);
                                element.ExtractString(nameof(anime_planet_id), out anime_planet_id);
                                element.ExtractInt(nameof(anisearch_id), out anisearch_id);
                                element.ExtractInt(nameof(kitsu_id), out kitsu_id);
                                element.ExtractInt(nameof(livechart_id), out livechart_id);
                                element.ExtractInt(nameof(mal_id), out mal_id);
                                element.ExtractInt(nameof(simkl_id), out simkl_id);
                                element.ExtractInt(nameof(tvdb_id), out tvdb_id);

                                imdb_id = null;

                                if (element.TryGetProperty("imdb_id", out JsonElement imdbProp))
                                {
                                    List<string> imdbIds = new List<string>();

                                    foreach (JsonElement id in imdbProp.EnumerateArray())
                                        imdbIds.Add(id.GetString()!);

                                    if (imdbIds.Count == 0)
                                        continue;

                                    if (imdbIds.Count > 1)
                                        Console.WriteLine($"There were {imdbIds.Count} imdb ids present, only using the first");

                                    imdb_id = imdbIds[0];
                                }

                                tvdbSeason = null;
                                tmdbSeason = null;
                                tmdbId = null;


                                if (element.TryGetProperty("season", out JsonElement seasonProp))
                                {
                                    seasonProp.ExtractInt("tvdb", out tvdbSeason);
                                    seasonProp.ExtractInt("tmdb", out tmdbSeason);
                                }

                                if (element.TryGetProperty("themoviedb_id", out JsonElement tmdbProp))
                                {
                                    tmdbProp.ExtractInt("tv", out int? tvId);
                                    tmdbProp.ExtractInt("tv", out int? movieId);

                                    tmdbId = movieId ?? tvdb_id;
                                }

                                newLinks.Add(new Model_Link()
                                {
                                    anilist_id = anilist_id,
                                    anidb_id = anidb_id,
                                    animecountdown_id = animecountdown_id,
                                    animenewsnetwork_id = animenewsnetwork_id,
                                    anime_planet_id = anime_planet_id,
                                    anisearch_id = anisearch_id,
                                    imdb_id = imdb_id,
                                    kitsu_id = kitsu_id,
                                    livechart_id = livechart_id,
                                    mal_id = mal_id,
                                    simkl_id = simkl_id,
                                    themoviedb_id = tmdbId,
                                    tmdb_season = tmdbSeason,
                                    tvdb_id = tvdb_id,
                                    tvdb_season = tvdbSeason,
                                    type = type
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to process link - {e.Message}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return newLinks.ToArray();
            }
        }
    }

    private class LinkDownloader_OfflineList : LinkDownloader
    {
        public override async Task<Model_Link[]?> Download()
        {
            Dictionary<string, Action<Model_Link, string>> lookup = new()
            {
                { "https://kitsu.app/anime/", (link, s) => link.kitsu_id = int.Parse(s) },
                { "https://anidb.net/anime/", (link, s) => link.anidb_id = int.Parse(s) },
                { "https://simkl.com/anime/", (link, s) => link.simkl_id = int.Parse(s) },
                { "https://anilist.co/anime/", (link, s) => link.anilist_id = int.Parse(s) },
                { "https://anime-planet.com/anime/", (link, s) => link.anime_planet_id = s },
                { "https://myanimelist.net/anime/", (link, s) => link.mal_id = int.Parse(s) },
                { "https://livechart.me/anime/", (link, s) => link.livechart_id = int.Parse(s) },
                { "https://anisearch.com/anime/", (link, s) => link.anisearch_id = int.Parse(s) },
                { "https://animecountdown.com/", (link, s) => link.animecountdown_id = int.Parse(s) },
                { "https://animenewsnetwork.com/encyclopedia/anime.php?id=", (link, s) => link.animenewsnetwork_id = int.Parse(s) },
            };

            ConcurrentDictionary<int, Model_Link> anidbCentricLinks = new();
            ConcurrentBag<Model_Link> remainingLinks = new();

            using (HttpClient client = new HttpClient())
            {

                try
                {
                    HttpResponseMessage res = await client.GetAsync("https://github.com/manami-project/anime-offline-database/releases/download/latest/anime-offline-database-minified.json");
                    res.EnsureSuccessStatusCode();

                    // get offline anidb

                    using (Stream stream = await res.Content.ReadAsStreamAsync())
                    using (JsonDocument doc = await JsonDocument.ParseAsync(stream))
                    {
                        try
                        {
                            JsonElement[] entries = [.. doc.RootElement.GetProperty("data").EnumerateArray()];

                            Parallel.ForEach(entries, (el) =>
                            {
                                Model_Link link = new();
                                JsonElement[] sources = [.. el.GetProperty("sources").EnumerateArray()];

                                foreach (JsonElement source in sources)
                                    ExtractLinkBasedUrl(link, source.GetString());

                                if (link.anidb_id.HasValue)
                                    anidbCentricLinks.AddOrUpdate(link.anidb_id.Value, link, (a, b) => link);
                                else
                                    remainingLinks.Add(link);
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to process link - {e.Message}");
                        }
                    }

                    // fetch tvdb data

                    res = await client.GetAsync("https://raw.githubusercontent.com/Anime-Lists/anime-lists/refs/heads/master/anime-list-master.xml");
                    string xml = await res.Content.ReadAsStringAsync();
                    XDocument xmlDoc = XDocument.Parse(xml);

                    Parallel.ForEach(xmlDoc.Root!.Elements("anime"), (el) =>
                    {
                        int? anidb_id = TryParse("anidbid");

                        if (anidb_id.HasValue && anidbCentricLinks.TryGetValue(anidb_id.Value, out Model_Link? link))
                        {
                            link!.tvdb_id = TryParse("tvdbid");
                            link!.tvdb_season = TryParse("defaulttvdbseason");
                            link!.tmdb_season = TryParse("tmdbseason");
                            link!.themoviedb_id = TryParse("tmdbid");
                            link!.imdb_id = el.Attribute("imdbid")?.Value;
                            //link!.tvdb_id = TryParse("tmdbtv");
                        }

                        int? TryParse(string attributeName)
                        {
                            string? str = el.Attribute(attributeName)?.Value;

                            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out int _temp))
                                return _temp;

                            return null;
                        }
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return [.. anidbCentricLinks.Values, .. remainingLinks];
            }

            void ExtractLinkBasedUrl(Model_Link link, string? url)
            {
                if (string.IsNullOrEmpty(url))
                    return;

                foreach (KeyValuePair<string, Action<Model_Link, string>> entry in lookup)
                {
                    if (url.StartsWith(entry.Key))
                    {
                        try
                        {
                            entry.Value(link, url.Substring(entry.Key.Length));
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"failed to parse - {url}\n{e.Message}");
                            break;
                        }
                    }
                }
            }
        }
    }
}
