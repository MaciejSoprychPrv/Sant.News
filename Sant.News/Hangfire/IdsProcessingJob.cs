
using Hangfire;
using Sant.News.HackerNews;

namespace Sant.News.Hangfire
{
    public class IdsProcessingJob : IIdsProcessingJob
    {
        private readonly IBackgroundJobClient _backgroundJobs;

        public IdsProcessingJob(IIdsProcessing idsProcessing, IBackgroundJobClient backgroundJobs)
        {
            _backgroundJobs = backgroundJobs;
        }

        async Task IIdsProcessingJob.IdsJob(CancellationToken cancellationToken)
        {
            _backgroundJobs.Enqueue<IIdsProcessing>(s =>
                s.AddIds(cancellationToken));
        }
    }

    public interface IIdsProcessingJob
    {
        Task IdsJob(CancellationToken cancellationToken);
    }
}
