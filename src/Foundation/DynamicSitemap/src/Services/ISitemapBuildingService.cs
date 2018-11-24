using Sitecore.SharedSource.DynamicSitemap.Model;
using System.Collections.Generic;

namespace Sitecore.SharedSource.DynamicSitemap.Services
{
    public interface ISitemapBuildingService
    {
        string BuildSitemap(SitemapSiteConfiguration sitemapSiteConfiguration, List<UrlElement> elements);
    }
}