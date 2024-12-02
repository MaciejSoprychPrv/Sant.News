

using System.Text.Json;
using Flurl;
using Flurl.Http;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using static Sant.News.HackerNews.GetHackerNews;

namespace Sant.News.HackerNews
{
    public class StoryDetailsProcessing : IStoryDetailsProcessing
    {
        private readonly HackerNewsConnectionOptions _hackerNewsConnectionOptions;
        private readonly IMemoryCache _cache;
        private readonly IBackgroundJobClient _client;
        private readonly ILogger<StoryDetailsProcessing> _logger;

        public StoryDetailsProcessing(IOptions<HackerNewsConnectionOptions> hackerNewsConnectionOptions, IMemoryCache cache, IBackgroundJobClient client, ILogger<StoryDetailsProcessing> logger)
        {
            _cache = cache;
            _client = client;
            _logger = logger;
            _hackerNewsConnectionOptions = hackerNewsConnectionOptions.Value;
        }

        public List<HackerNewsRaw> GetAllStoryDetails()
        {
            var rawId = GetIds();
            var ids = Convert(rawId);
            var allStories = new List<HackerNewsRaw>();
            foreach (var id in ids)
            {
                if (_cache.TryGetValue($"Detail_{id}", out HackerNewsRaw storyDetail))
                {
                    allStories.Add(storyDetail);
                }
            }
            return allStories;
        }

        public async Task AddDetails()
        {
            var idsRaw = GetIds();
            var ids = Convert(idsRaw);
            var detailJobsIds = new List<string>();
            foreach (var id in ids)
            {
                var detailJobId = _client.Enqueue("hackernews", () => AddDetail(id));
                detailJobsIds.Add(detailJobId);
                _logger.LogInformation($"Getting detail for story {id} enqueued");
            }
            _cache.Set("DetailJobIds", detailJobsIds, TimeSpan.FromMinutes(30));
        }

        public async Task AddDetail(int id)
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
        List<HackerNewsRaw> GetAllStoryDetails();
        Task AddDetails();
    }
}
