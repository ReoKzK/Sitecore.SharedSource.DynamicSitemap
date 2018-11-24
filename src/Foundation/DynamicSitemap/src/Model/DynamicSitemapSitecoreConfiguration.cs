using Sitecore.Data.Items;
using System;
using System.Collections.Generic;

namespace Sitecore.SharedSource.DynamicSitemap.Model
{
    /// <summary>
    /// Configuration in Sitecore item
    /// </summary>
    public class DynamicSitemapSitecoreConfiguration
    {
        /// <summary>
        /// If disable whole sitemap generation
        /// </summary>
        public bool DisableSitemap { get; set; }

        /// <summary>
        /// Configuration that will be used as main
        /// </summary>
        public SitemapSiteConfiguration MainSiteConfiguration { get; set; }

        /// <summary>
        /// Configuration Item that will be used as main
        /// </summary>
        public Item MainSiteConfigurationItem { get; set; }

        /// <summary>
        /// Search engines
        /// </summary>
        public List<String> SearchEngines { get; set; }

        /// <summary>
        /// Languages that will be processed
        /// </summary>
        public List<String> ProcessedLanguages { get; set; }
    }
}
