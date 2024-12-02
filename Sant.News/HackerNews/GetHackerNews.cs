using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

       
    }
}
