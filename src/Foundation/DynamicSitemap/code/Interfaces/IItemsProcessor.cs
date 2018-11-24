using Sitecore.SharedSource.DynamicSitemap.Model;
using System.Collections.Generic;

namespace Sitecore.SharedSource.DynamicSitemap.Interfaces
{
    public interface IItemsProcessor
    {
        List<UrlElement> ProcessItems(SitemapSiteConfiguration sitemapSiteConfiguration);
    }
}
