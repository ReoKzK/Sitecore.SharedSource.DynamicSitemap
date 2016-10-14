using Sitecore.SharedSource.DynamicSitemap.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.DynamicSitemap;
using Sitecore.SharedSource.DynamicSitemap.Model;

namespace Sitecore.SharedSource.DynamicSitemap.ItemsProcessor
{
    /// <summary>
    /// Sample items processor 
    /// Adds some custom elements to sitemap
    /// </summary>
    public class SampleItemsProcessor : IItemsProcessor
    {
        /// <summary>
        /// Method that will process your items
        /// </summary>
        /// <param name="sitemapSiteConfiguration">Sitemap site configuration to which items will be added</param>
        /// <returns>List of your items</returns>
        public List<UrlElement> ProcessItems(SitemapSiteConfiguration sitemapSiteConfiguration)
        {
            var items = new List<UrlElement>();

            // - Let's check for specific language -

            if (sitemapSiteConfiguration.LanguageName == "en")
            {
                // - Add your own elements packed into object of UrlElement class -

                items.Add(new UrlElement
                {
                    Location = "http://mysite.com/some-custom-static-page.html",
                    Priority = "0.7",
                    LastModification = new DateTime(2016, 03, 01),
                    ChangeFrequency = "yearly"
                });
            }
            
            // - Return collected items -

            return items;
        }
    }
}
