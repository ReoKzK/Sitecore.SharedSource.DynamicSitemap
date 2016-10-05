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
        public String Location { get; set; }

        public DateTime LastModification { get; set; }

        public String ChangeFrequency { get; set; }

        public String Priority { get; set; }
    }
}
