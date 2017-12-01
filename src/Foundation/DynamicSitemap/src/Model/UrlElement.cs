using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DynamicSitemap.Model
{
    /// <summary>
    /// Url Element
    /// </summary>
    public class UrlElement
    {
        /// <summary>
        /// The &lt;loc&gt; element
        /// </summary>
        public String Location { get; set; }

        /// <summary>
        /// The &lt;lastmod&gt; element
        /// </summary>
        public DateTime LastModification { get; set; }

        /// <summary>
        /// The &lt;changefreq&gt; element
        /// </summary>
        public String ChangeFrequency { get; set; }

        /// <summary>
        /// The &lt;priority&gt; element
        /// </summary>
        public String Priority { get; set; }
    }
}
