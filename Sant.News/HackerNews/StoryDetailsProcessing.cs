

using System.Text.Json;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using static Sant.News.HackerNews.GetHackerNews;

namespace Sant.News.HackerNews
{
    public class StoryDetailsProcessing : IStoryDetailsProcessing
    {
        private readonly HackerNewsConnectionOptions _hackerNewsConnectionOptions;
        private readonly IMemoryCache _cache;

        public StoryDetailsProcessing(IOptions<HackerNewsConnectionOptions> hackerNewsConnectionOptions, IMemoryCache cache)
        {
            _cache = cache;
            _hackerNewsConnectionOptions = hackerNewsConnectionOptions.Value;
        }

        public async Task AddDetails()
        {
            var idsRaw = GetIds();
            var ids = Convert(idsRaw);
            foreach (var id in ids)
            {
                await AddDetail(id);
            }
        }

        private async Task AddDetail(int id)
        {
            var result = await _hackerNewsConnectionOptions.Url
                .AppendPathSegment($"v0/item/{id}.json")
                .GetJsonAsync<HackerNewsRaw>();

            _cache.Set($"Detail_{id}", result, TimeSpan.FromMinutes(30));

        }

        private List<int> Convert(string? ids)
        {
            List<int> result = JsonSerializer.Deserialize<List<int>>(ids);
            return result;
        }

        private string? GetIds()
        {
            if (_cache.TryGetValue("Ids", out string? result))
            {
                return result;
            }

            return null;
        }
    }

    public interface IStoryDetailsProcessing
    {
        Task AddDetails();
    }
}
