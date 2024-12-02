
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

        async Task IIdsProcessingJob.IdsJob()
        {
            _backgroundJobs.Enqueue<IIdsProcessing>(s =>
                s.AddIds());
        }
    }

    public interface IIdsProcessingJob
    {
        Task IdsJob();
    }
}
