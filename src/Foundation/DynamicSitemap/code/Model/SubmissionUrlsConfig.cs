using System;
using System.Collections.Generic;

namespace Sitecore.SharedSource.DynamicSitemap.Model
{
    public class SubmissionUrlsConfig
    {
        public String SitemapUrl { get; set; }

        public List<String> SearchEngines { get; set; }

        public SubmissionUrlsConfig()
        {
            SitemapUrl = String.Empty;
            SearchEngines = new List<String>();
        }
    }
}
