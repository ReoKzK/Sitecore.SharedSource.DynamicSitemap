using Sitecore.SharedSource.DynamicSitemap.Jobs;

namespace Sitecore.SharedSource.DynamicSitemap.Events
{
    public class EventManager
    {
        /// <summary>
        /// Regenerates sitemap for all configured sites
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void RegenerateSitemapOnEvent(object sender, System.EventArgs args)
        {
            if (args == null)
            {
                return;
            }

            var job = new SitemapBuildJob();
            job.StartJob();
        }
    }
}
