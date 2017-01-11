using Sitecore.Links;

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
                var url = ServerHost;

                url = !url.StartsWith("http://") ? "http://" + url : url;

                url = Sitecore.StringUtil.EnsurePostfix('/', url);
                url += FileName;

                return url;
            }
        }
    }
}