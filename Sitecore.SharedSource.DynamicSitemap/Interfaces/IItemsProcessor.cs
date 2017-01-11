using Sitecore.SharedSource.DynamicSitemap.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DynamicSitemap.Interfaces
{
    public interface IItemsProcessor
    {
        List<UrlElement> ProcessItems(SitemapSiteConfiguration sitemapSiteConfiguration);
    }
}