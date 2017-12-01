using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.SharedSource.DynamicSitemap.Model;

namespace Sitecore.SharedSource.DynamicSitemap.Services
{
    public interface IItemsProcessingService
    {
        List<UrlElement> ProcessItems(List<Item> items, SitemapSiteConfiguration sitemapSiteConfiguration, UrlOptions options);
    }
}