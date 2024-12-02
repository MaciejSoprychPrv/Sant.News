using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Sant.News.HackerNews
{
    public class IdsProcessing : IIdsProcessing
    {
        private readonly HackerNewsConnectionOptions _hackerNewsConnectionOptions;
        private readonly IMemoryCache _cache;

        public IdsProcessing(IOptions<HackerNewsConnectionOptions> hackerNewsConnectionOptions, IMemoryCache cache)
        {
            _hackerNewsConnectionOptions = hackerNewsConnectionOptions.Value;
            _cache = cache;
        }
        public async Task AddIds()
        {
            var result = await _hackerNewsConnectionOptions.Url
                .AppendPathSegment("v0/beststories.json")
                .GetStringAsync();

            _cache.Set("Ids", result, TimeSpan.FromMinutes(30));

        }

    }

    public interface IIdsProcessing
    {
        Task AddIds();
    }
}
