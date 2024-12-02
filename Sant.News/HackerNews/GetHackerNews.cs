using AutoMapper;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Sant.News.HackerNews
{
    [ApiController]
    public class GetHackerNews : ControllerBase
    {
        public const string Url = "/api/hackerNews/{storiesCount}";
        private readonly IMediator _mediator;

        public GetHackerNews(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(Url)]
        public async Task<Result> Get(int storiesCount, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new Query(storiesCount)
                , cancellationToken);
        }

        public class Query(int storiesCount) : IRequest<Result<List<GetHackerNewsDto>>>
        {
            public int StoriesCount { get; set; } = storiesCount;
        }

        public class GetHackerNewsDto
        {
            public string? Title { get; set; }
            public string? Uri { get; set; }
            public string? PostedBy { get; set; }
            public DateTimeOffset Time { get; set; }
            public int Score { get; set; }
            public int CommentCount { get; set; }
        }

        public class HackerNewsRaw
        {
            [JsonProperty("by")] public string By { get; set; }

            [JsonProperty("descendants")] public int Descendants { get; set; }

            [JsonProperty("id")] public int Id { get; set; }

            [JsonProperty("kids")] public List<int> Kids { get; set; }

            [JsonProperty("score")] public int Score { get; set; }

            [JsonProperty("text")] public string Text { get; set; }

            [JsonProperty("time")] public int Time { get; set; }

            [JsonProperty("title")] public string Title { get; set; }

            [JsonProperty("type")] public string Type { get; set; }

            [JsonProperty("url")] public string Url { get; set; }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<HackerNewsRaw, GetHackerNewsDto>()
                    .ForMember(dest => dest.Uri,
                        opt => opt.MapFrom(src => src.Url))
                    .ForMember(dest => dest.PostedBy,
                        opt => opt.MapFrom(src => src.By))
                    .ForMember(dest => dest.Time,
                        opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.Time).DateTime))
                    .ForMember(dest => dest.CommentCount,
                        opt => opt.MapFrom(src => src.Descendants));
            }
        }

        public class Handler : IRequestHandler<Query, Result<List<GetHackerNewsDto>>>
        {
            private readonly IBackgroundJobClient _client;
            private readonly IIdsProcessing _idsProcessing;
            private readonly IStoryDetailsProcessing _storyDetailsProcessing;
            private readonly IMapper _mapper;
            private readonly ILogger<Handler> _logger;

            public Handler(IBackgroundJobClient client, IIdsProcessing idsProcessing, IStoryDetailsProcessing storyDetailsProcessing, IMapper mapper, ILogger<Handler> logger)
            {
                _client = client;
                _idsProcessing = idsProcessing;
                _storyDetailsProcessing = storyDetailsProcessing;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<List<GetHackerNewsDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Fetching Hacker News' ids started");

                var idsJobId = _client.Enqueue("hackernews", () => _idsProcessing.AddIds());
                _logger.LogInformation("Processing ids enqueued");

                var detailsJobId = _client.ContinueJobWith(idsJobId, "hackernews", ()=>_storyDetailsProcessing.AddDetails());
                _logger.LogInformation("Processing details enqueued");

                var detailsJobIdStatus = GetStatus(detailsJobId);

                if (detailsJobIdStatus != JobIdStatus.Succeeded.ToString())
                {
                    _logger.LogInformation("Processing failed");
                    return Result.BadRequest<List<GetHackerNewsDto>>("The process was not successful please try again");
                }

                _logger.LogInformation("Getting Details from cache started");
                var rawStoriesDetails = _storyDetailsProcessing.GetAllStoryDetails();
                _logger.LogInformation("Getting Details from cache completed");

                _logger.LogInformation("Mapping to DTO started");
                var result = _mapper.Map<List<GetHackerNewsDto>>(rawStoriesDetails);
                _logger.LogInformation("Mapping to DTO completed");

                
                result = result.OrderByDescending(c => c.Score).ToList();
                _logger.LogInformation("Ordered descending");

                var storiesCount = CalculateStorieCount(request.StoriesCount, result);
                _logger.LogInformation("Number of stories to be taken");

                result = result.Take(storiesCount).ToList();
                _logger.LogInformation("Ranged to the number of requested stories");

                return Result.Ok(result);
            }

            private int CalculateStorieCount(int storiesCount, List<GetHackerNewsDto> storiesDetail)
            {
                var maxNumberOfStories = storiesDetail.Count;
                if (storiesCount > maxNumberOfStories)
                {
                    storiesCount = maxNumberOfStories;
                }

                return storiesCount;
            }
            private string GetStatus(string jobId)
            {
                int maxRetries = 50;
                int retryDelayMilliseconds = 2000;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    string status = GetJobState(jobId);

                    if (status == "Succeeded")
                    {
                        return status;
                    }

                    if (attempt < maxRetries)
                    {
                        Task.Delay(retryDelayMilliseconds).Wait();
                    }
                }

                return "Failed to retrieve status";
            }
            public string GetJobState(string jobId)
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var stateData = connection.GetStateData(jobId);
                    return stateData?.Name ?? "State not available";
                }
            }
        }
    }
}
