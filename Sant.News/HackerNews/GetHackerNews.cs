using AutoMapper;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.StoriesCount)
                    .GreaterThan(0)
                    .WithMessage("StoriesCount must be greater than zero.");
            }
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
            private readonly IValidator<Query> _validator;
 
            public Handler(IBackgroundJobClient client, IIdsProcessing idsProcessing, IStoryDetailsProcessing storyDetailsProcessing, IMapper mapper, ILogger<Handler> logger, IValidator<Query> validator)
            {
                _client = client;
                _idsProcessing = idsProcessing;
                _storyDetailsProcessing = storyDetailsProcessing;
                _mapper = mapper;
                _logger = logger;
                _validator = validator;
            }

            public async Task<Result<List<GetHackerNewsDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Validation of storiesCount started");

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (validationResult.IsValid == false)
                {
                    return Result.BadRequest<List<GetHackerNewsDto>>("Stories count should be greater than 0");
                }
                _logger.LogInformation("Validation of storiesCount completed");

                _logger.LogInformation("Fetching Hacker News' ids started");
                var addIdsJobId = _client.Enqueue("hackernews", () => _idsProcessing.AddIds(cancellationToken));
                _logger.LogInformation("Processing ids enqueued");

                _logger.LogInformation($"Waiting for AddIds job to be completed");
                WaitUntilStatusIsCompleted(addIdsJobId);
                _logger.LogInformation($"AddIds job status completed");

                await _storyDetailsProcessing.AddDetails(cancellationToken);
                _logger.LogInformation("Processing details");

                _logger.LogInformation("Waiting for AddDetails jobs to be completed");
                var detailJobsIds = _storyDetailsProcessing.GetDetailJobsIds();
                WaitTillAllStatusesAreCompleted(detailJobsIds);
                _logger.LogInformation("AddDetails jobs completed");

                _logger.LogInformation("Getting Details from cache started");
                var rawStoriesDetails = _storyDetailsProcessing.GetAllStoryDetails();
                _logger.LogInformation("Getting Details from cache completed");

                _logger.LogInformation("Mapping to DTO started");
                var result = _mapper.Map<List<GetHackerNewsDto>>(rawStoriesDetails);
                _logger.LogInformation("Mapping to DTO completed");

                result = result.OrderByDescending(c => c.Score).ToList();
                _logger.LogInformation("Ordered descending");

                var storiesCount = CalculateStoriesCount(request.StoriesCount, result);
                _logger.LogInformation("Number of stories to be taken");

                result = result.Take(storiesCount).ToList();
                _logger.LogInformation("Ranged to the number of requested stories");

                return Result.Ok(result);
            }

            private void WaitTillAllStatusesAreCompleted(List<string> jobsIds)
            {
                foreach (var jobsId in jobsIds)
                {
                    WaitUntilStatusIsCompleted(jobsId);
                }
            }

            private int CalculateStoriesCount(int storiesCount, List<GetHackerNewsDto> storiesDetail)
            {
                var maxNumberOfStories = storiesDetail.Count;
                if (storiesCount > maxNumberOfStories)
                {
                    storiesCount = maxNumberOfStories;
                }

                return storiesCount;
            }
            private void WaitUntilStatusIsCompleted(string jobId)
            {
                int maxRetries = 50;
                int retryDelayMilliseconds = 500;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    string status = GetJobState(jobId);

                    if (status != "Succeeded")
                    {
                        if (attempt < maxRetries)
                        {
                            Task.Delay(retryDelayMilliseconds).Wait();
                        }
                    }
                }
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
