using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DynamicSitemap.Model
{
    /// <summary>
    /// Configuration in Sitecore item
    /// </summary>
    public class DynamicSitemapSitecoreConfiguration
    {
        /// <summary>
        /// Configuration that will be used as main
        /// </summary>
        public SitemapSiteConfiguration MainSiteConfiguration { get; set; }
        
        /// <summary>
        /// Search engines
        /// </summary>
        public List<String> SearchEngines { get; set; }
    }
}
