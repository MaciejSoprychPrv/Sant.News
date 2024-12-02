using AutoMapper;
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
            public Task<Result<List<GetHackerNewsDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
