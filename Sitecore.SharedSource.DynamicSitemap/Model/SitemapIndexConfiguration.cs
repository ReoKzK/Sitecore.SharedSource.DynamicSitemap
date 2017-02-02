using Sitecore.Links;
using Sitecore.SharedSource.DynamicSitemap.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DynamicSitemap.Model
{
    /// <summary>
    /// Sitemap index configuration
    /// </summary>
    public class SitemapIndexConfiguration
    {
        /// <summary>
        /// Server host
        /// </summary>
        public string ServerHost { get; set; }

        /// <summary>
        /// Forces generating https urls
        /// </summary>
        public bool ForceHttps { get; set; }

        /// <summary>
        /// Sitemap index file name
        /// </summary>
        public String FileName { get; set; }

        /// <summary>
        /// Url to sitemap index file
        /// </summary>
        public String Url
        {
            get
            {
                var url = !string.IsNullOrEmpty(ServerHost) ? ServerHost : TargetHostName;

                url = Sitecore.StringUtil.EnsurePostfix('/', url);
                url += FileName;    
                url = DynamicSitemapHelper.EnsureHttpPrefix(url, ForceHttps);

                return url;
            }
        }

        public string TargetHostName { get; set; }
    }
}