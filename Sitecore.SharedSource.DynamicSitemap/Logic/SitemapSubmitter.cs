using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using Sitecore.SharedSource.DynamicSitemap.Constants;
using Sitecore.SharedSource.DynamicSitemap.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Sitecore.SharedSource.DynamicSitemap.Logic
{
    /// <summary>
    /// Sitemap submitter
    /// </summary>
    public class SitemapSubmitter
    {
        /// <summary>
        /// Configuration in Sitecore
        /// </summary>
        protected DynamicSitemapSitecoreConfiguration SitecoreConfiguration;

        /// <summary>
        /// Database
        /// </summary>
        protected Database Database;

        /// <summary>
        /// Site configurations list
        /// </summary>
        protected List<SitemapSiteConfiguration> SiteConfigurations { get; set; }
        
        /// <summary>
        /// Sitemap index
        /// </summary>
        protected SitemapIndexConfiguration SitemapIndex { get; set; }

        protected List<SubmissionUrlsConfig> SubmissionUrlsConfig { get; set; }

        public SitemapSubmitter(DynamicSitemapSitecoreConfiguration config, List<SitemapSiteConfiguration> siteConfigurations, Database database)
        {
            SitecoreConfiguration = config;
            SiteConfigurations = siteConfigurations;

            Database = database;
        }

        protected void PrepareSubmissionUrls()
        {
            SubmissionUrlsConfig = new List<SubmissionUrlsConfig>();

            if (DynamicSitemapConfiguration.UseSitemapsIndexFile)
            {
                var submissionConfig = new SubmissionUrlsConfig();
                submissionConfig.SitemapUrl = SitemapIndex.Url;

                foreach (var searchEngineId in SitecoreConfiguration.SearchEngines)
                {
                    Item searchEngineItem = Database.GetItem(searchEngineId);

                    if (searchEngineItem != null)
                    {
                        submissionConfig.SearchEngines.Add(searchEngineItem["Sitemap Submission Uri"]);
                    }
                }

                SubmissionUrlsConfig.Add(submissionConfig);
            }

            else
            {
                foreach (var configuration in this.SiteConfigurations)
                {
                    var submissionConfig = new SubmissionUrlsConfig();
                    submissionConfig.SitemapUrl = configuration.SitemapUrl;

                    foreach (var searchEngineId in configuration.SearchEngines)
                    {
                        Item searchEngineItem = Database.GetItem(searchEngineId);

                        if (searchEngineItem != null)
                        {
                            submissionConfig.SearchEngines.Add(searchEngineItem["Sitemap Submission Uri"]);
                        }
                    }

                    SubmissionUrlsConfig.Add(submissionConfig);
                }
            }
        }

        /// <summary>
        /// Submits sitemaps to search engines
        /// </summary>
        /// <returns></returns>
        public bool SubmitSitemapsToSearchEngines()
        {
            bool result = false;

            PrepareSubmissionUrls();

            foreach (var configuration in SubmissionUrlsConfig)
            {
                foreach (var searchEngineUrl in configuration.SearchEngines)
                {
                    this.SubmitEngine(searchEngineUrl, configuration.SitemapUrl);
                }
            }

            result = true;

            return result;
        }

        /// <summary>
        /// Submits sitemap to specified search engine url
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="sitemapUrl"></param>
        protected void SubmitEngine(string engine, string sitemapUrl)
        {
            if (!sitemapUrl.Contains("://localhost"))
            {
                String text = engine + HttpUtility.HtmlEncode(sitemapUrl);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(text);

                try
                {
                    WebResponse response = httpWebRequest.GetResponse();
                    HttpWebResponse httpWebResponse = (HttpWebResponse)response;

                    if (httpWebResponse.StatusCode != HttpStatusCode.OK)
                    {
                        Diagnostics.Log.Error(String.Format(Messages.SitemapSubmitterCannotSubmit, engine, httpWebResponse.StatusDescription), this);
                    }
                }

                catch (Exception exc)
                {
                    Diagnostics.Log.Warn(String.Format(Messages.SitemapSubmitterExceptionWhileSubmit, text, exc.Message, exc.StackTrace), this);
                }
            }
        }
    }
}
