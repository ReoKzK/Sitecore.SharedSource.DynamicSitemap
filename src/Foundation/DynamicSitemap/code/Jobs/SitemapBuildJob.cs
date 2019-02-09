using Sitecore.Jobs;

namespace Sitecore.SharedSource.DynamicSitemap.Jobs
{
    public class SitemapBuildJob
    {
        protected string _jobName = "SitemapBuildJob";

        public Job Job
        {
            get { return JobManager.GetJob(_jobName); }
        }

        public string StartJob()
        {
            Sitecore.Diagnostics.Log.Info("SitemapBuildJob: Starting", this);

            JobOptions options = new JobOptions(_jobName,
                "DynamicSitemap",
                Context.Site.Name,
                this,
                "RebuildSitemap");

            JobManager.Start(options);

            return _jobName;
        }

        public void RebuildSitemap()
        {
            var generator = new DynamicSitemapGenerator();
            generator.RegenerateSitemap();
            
            if (Job != null)
            {
                Job.Status.State = JobState.Finished;
            }

            Sitecore.Diagnostics.Log.Info("SitemapBuildJob: Finished", this);
        }
    }

    
}
