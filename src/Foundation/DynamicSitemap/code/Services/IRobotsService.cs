using System.Collections.Generic;

namespace Sitecore.SharedSource.DynamicSitemap.Services
{
    public interface IRobotsService
    {
        void RegisterSitemapToRobotsFile(List<string> sitemapUrls);
    }
}