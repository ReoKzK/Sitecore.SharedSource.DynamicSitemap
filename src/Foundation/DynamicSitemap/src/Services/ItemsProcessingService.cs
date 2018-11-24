using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.SharedSource.DynamicSitemap.Helpers;
using Sitecore.SharedSource.DynamicSitemap.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.SharedSource.DynamicSitemap.Services
{
    public class ItemsProcessingService : IItemsProcessingService
    {
        /// <summary>
        /// Processes all items in site under root path
        /// </summary>
        /// <param name="items">List of Items</param>
        /// <param name="sitemapSiteConfiguration">Current sitemap configuration</param>
        /// <param name="options">Url Options</param>
        public List<UrlElement> ProcessItems(List<Item> items, SitemapSiteConfiguration sitemapSiteConfiguration, UrlOptions options)
        {
            var urlElements = new List<UrlElement>();

            foreach (var item in items)
            {
                if (item.Versions.Count > 0)
                {
                    //if (DynamicSitemapHelper.IsWildcard(item))
                    //{
                    //    PrepareDynamicItems(item, sitemapSiteConfiguration, xml);
                    //}

                    if (IsIncluded(item, sitemapSiteConfiguration))
                    {
                        sitemapSiteConfiguration.ItemsCount++;

                        var url = LinkManager.GetItemUrl(item, options);
                        url = DynamicSitemapHelper.EnsureHttpPrefix(url, sitemapSiteConfiguration.ForceHttps);

                        if (!String.IsNullOrEmpty(sitemapSiteConfiguration.ServerHost))
                        {
                            url = DynamicSitemapHelper.ReplaceHost(url, sitemapSiteConfiguration.ServerHost);
                        }

                        urlElements.Add(
                            new UrlElement()
                            {
                                Location = url,
                                LastModification = item.Statistics.Updated,
                                ChangeFrequency = sitemapSiteConfiguration.GetChangeFrequency(item),
                                Priority = sitemapSiteConfiguration.GetPriority(item)
                            });
                    }
                }
            }

            if (sitemapSiteConfiguration.ItemsProcessor != null)
            {
                var urlItems = sitemapSiteConfiguration.ItemsProcessor.ProcessItems(sitemapSiteConfiguration);

                urlItems.AddRange(urlItems);
            }

            return urlElements;
        }

        /// <summary>
        /// Checks if Item can be included in sitemap
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="isDataSourceItem">Is item used only in wildcard</param>
        /// <returns>true if included</returns>
        protected virtual bool IsIncluded(Item item, SitemapSiteConfiguration sitemapSiteConfiguration, bool isDataSourceItem = false)
        {
            var result = false;

            if (!sitemapSiteConfiguration.ExcludedItems.Any(x => x == item.ID.ToString())
                && sitemapSiteConfiguration.IncludedTemplates.Contains(item.TemplateID.ToString())
                && !sitemapSiteConfiguration.ExcludedItemPaths.Any(x => item.Paths.FullPath.ToLower().StartsWith(x.Paths.FullPath.ToLower()) || item.Paths.FullPath.ToLower().Equals(x.Paths.FullPath.ToLower()))
                && (item.Paths.FullPath.StartsWith(sitemapSiteConfiguration.RootItem.Paths.FullPath)
                    || item.Paths.FullPath.Equals(sitemapSiteConfiguration.RootItem.Paths.FullPath)
                    || isDataSourceItem)) // - datasource items can be out of root item
            {
                result = true;
            }

            return result;
        }
    }
}
