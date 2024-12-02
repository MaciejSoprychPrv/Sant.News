

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Sant.News.HackerNews
{
    public class IdsProcessing
    {
        private readonly HackerNewsConnectionOptions _hackerNewsConnectionOptions;
        private readonly IMemoryCache _cache;

        public IdsProcessing(IOptions<HackerNewsConnectionOptions> hackerNewsConnectionOptions, IMemoryCache cache)
        {
            _hackerNewsConnectionOptions = hackerNewsConnectionOptions.Value;
            _cache = cache;
        }
    }
}
