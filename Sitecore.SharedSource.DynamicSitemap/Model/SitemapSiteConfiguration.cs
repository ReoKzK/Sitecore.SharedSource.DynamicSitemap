using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using Sitecore.SharedSource.DynamicSitemap.Constants;
using Sitecore.SharedSource.DynamicSitemap.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.SharedSource.DynamicSitemap.Helpers;

namespace Sitecore.SharedSource.DynamicSitemap.Model
{
    using static System.String;

    /// <summary>
    /// Sitemap Site Configuration
    /// </summary>
    public class SitemapSiteConfiguration
    {
        /// <summary>
        /// Custom server host
        /// </summary>
        public string ServerHost { get; private set; }

        /// <summary>
        /// Excluded Items
        /// </summary>
        public List<string> ExcludedItems { get; private set; }

        /// <summary>
        /// Excluded paths
        /// </summary>
        public List<string> ExcludedPaths { get; private set; }

        /// <summary>
        /// Included templates
        /// </summary>
        public List<Guid> IncludedTemplates { get; private set; }

        /// <summary>
        /// Included templates
        /// </summary>
        public List<Guid> IncludedBaseTemplates { get; private set; }

        /// <summary>
        /// Excluded Item paths
        /// </summary>
        public List<Item> ExcludedItemPaths { get; private set; }

        /// <summary>
        /// Search engines
        /// </summary>
        public List<string> SearchEngines { get; private set; }

        /// <summary>
        /// Root Item ID
        /// </summary>
        private string RootItemId;

        /// <summary>
        /// Root Item
        /// </summary>
        public Item RootItem { get; private set; }

        /// <summary>
        /// Site
        /// </summary>
        public Sites.SiteContext Site { get; private set; }

        /// <summary>
        /// Configuration Language
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Configuration Language
        /// </summary>
        public string LanguageName
        {
            get
            {
                return Language != null ? Language.Name : Empty;
            }
        }

        /// <summary>
        /// Sitemap file name
        /// </summary>
        public string SitemapFileName { get; set; }

        /// <summary>
        /// Sitemap file path
        /// </summary>
        public string SitemapFilePath { get; set; }

        /// <summary>
        /// Forces generating https urls
        /// </summary>
        public bool ForceHttps { get; set; }

        /// <summary>
        /// Change frequencies for templates
        /// </summary>
        public Dictionary<string, string> ChangeFrequenciesForTemplates { get; set; }

        /// <summary>
        /// Change frequencies for items
        /// </summary>
        public Dictionary<string, string> ChangeFrequenciesForItems { get; set; }

        /// <summary>
        /// Change frequency default value
        /// </summary>
        public string ChangeFrequencyDefaultValue { get; private set; }

        /// <summary>
        /// Priorities for templates
        /// </summary>
        public Dictionary<string, string> PrioritiesForTemplates { get; set; }

        /// <summary>
        /// Priorities for items
        /// </summary>
        public Dictionary<string, string> PrioritiesForItems { get; set; }

        /// <summary>
        /// Priority default value
        /// </summary>
        public string PriorityDefaultValue { get; private set; }

        /// <summary>
        /// Items processor type to load
        /// </summary>
        public string ItemsProcessorTypeToLoad { get; private set; }

        /// <summary>
        /// Items processor 
        /// </summary>
        public IItemsProcessor ItemsProcessor { get; set; }

        /// <summary>
        /// Items count
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// Sitemap
        /// </summary>
        public string SitemapUrl
        {
            get
            {
                var url = TargetHost;
                url += SitemapFilePath.Replace("//", "/");                

                return url;
            }
        }

        public string TargetHost
        {
            get
            {
                var url = Empty;

                if (Site.TargetHostName == Empty)
                {
                    Sitecore.Links.UrlOptions urlOptions = Sitecore.Links.UrlOptions.DefaultOptions;
                    urlOptions.LanguageEmbedding = LanguageEmbedding.Never;
                    urlOptions.SiteResolving = true;
                    urlOptions.Site = Site;
                    urlOptions.AlwaysIncludeServerUrl = true;

                    var startItem = _configurationItem.Database.GetItem(Site.ContentStartPath);

                    url = Sitecore.Links.LinkManager.GetItemUrl(startItem, urlOptions);
                }
                else
                {
                    url = !IsNullOrEmpty(ServerHost) ? ServerHost : Site.TargetHostName + '/';
                }

                url = DynamicSitemapHelper.EnsureHttpPrefix(url, ForceHttps);
                return url;
            }
        }

        /// <summary>
        /// Dynamic routes
        /// </summary>
        public List<Item> DynamicRoutes { get; set; }

        /// <summary>
        /// Configuration Item
        /// </summary>
        private Item _configurationItem { get; set; }

        /// <summary>
        /// Sitemap Configuration
        /// </summary>
        /// <param name="configurationItem"></param>
        public SitemapSiteConfiguration(Item configurationItem)
        {
            _configurationItem = configurationItem;

            ServerHost = configurationItem["Server Host"];

            IncludedBaseTemplates = !IsNullOrEmpty(configurationItem["Included Base Templates"])
                                    ? configurationItem["Included Base Templates"].Split('|').Select(Guid.Parse).ToList()
                                    : new List<Guid>();

            IncludedTemplates = !IsNullOrEmpty(configurationItem["Included Templates"])
                                    ? configurationItem["Included Templates"].Split('|').Select(Guid.Parse).ToList()
                                    : new List<Guid>();

            ExcludedItems = !IsNullOrEmpty(configurationItem["Excluded Items"])
                                ? configurationItem["Excluded Items"].Split('|').ToList()
                                : new List<string>();

            ExcludedPaths = !IsNullOrEmpty(configurationItem["Excluded Paths"])
                                ? configurationItem["Excluded Paths"].Split('|').ToList()
                                : new List<string>();

            SearchEngines = !IsNullOrEmpty(configurationItem["Search Engines"])
                                ? configurationItem["Search Engines"].Split('|').ToList()
                                : new List<string>();

            ExcludedItemPaths = new List<Item>();

            foreach (var excludedPath in ExcludedPaths)
            {
                ExcludedItemPaths.Add(configurationItem.Database.GetItem(excludedPath));
            }

            RootItemId = configurationItem["Root Item"];
            RootItem = configurationItem.Database.GetItem(RootItemId);

            SitemapFileName = configurationItem["Sitemap File Name"];
            ForceHttps = configurationItem["Force HTTPS"] == "1";
            ItemsProcessorTypeToLoad = configurationItem["Type to load"];

            Site = Sitecore.Configuration.Factory.GetSite(configurationItem.Name.ToLower());
            Language = configurationItem.Language;

            ReadDynamicRoutes();
            ReadChangeFrequencies();
            ReadPriorities();

            ItemsCount = 0;
        }

        /// <summary>
        /// Reads priorities configuration
        /// </summary>
        private void ReadPriorities()
        {
            PriorityDefaultValue = _configurationItem["Default Priority"];

            PrioritiesForTemplates = new Dictionary<string, string>();
            PrioritiesForItems = new Dictionary<string, string>();

            Item prioritiesFolder =
                _configurationItem.Database.GetItem(
                    _configurationItem.Paths.FullPath + "/"
                    + DynamicSitemapConfiguration.SitemapConfigurationPrioritiesFolderName);

            if (prioritiesFolder != null && prioritiesFolder.HasChildren)
            {
                foreach (var item in prioritiesFolder.Children.ToList())
                {
                    if (item.TemplateID.ToString() == TemplateIds.PriorityForTemplatesTemplateId)
                    {
                        if (item["Templates"] != Empty && item["Priority"] != Empty)
                        {
                            var elements = item["Templates"].Split('|').ToList();
                            var priority = item["Priority"];

                            if (priority != Empty)
                            {
                                foreach (var element in elements)
                                {
                                    PrioritiesForTemplates.Add(element, priority);
                                }
                            }
                        }
                    }

                    else if (item.TemplateID.ToString() == TemplateIds.PriorityForItemsTemplateId)
                    {
                        if (item["Items"] != Empty && item["Priority"] != Empty)
                        {
                            var elements = item["Items"].Split('|').ToList();
                            var priority = item["Priority"];

                            if (priority != Empty)
                            {
                                foreach (var element in elements)
                                {
                                    PrioritiesForItems.Add(element, priority);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads change frequency configuration
        /// </summary>
        private void ReadChangeFrequencies()
        {
            var defaultChangeFrequency = _configurationItem.Database.GetItem(_configurationItem["Change Frequency"]);

            ChangeFrequencyDefaultValue = defaultChangeFrequency != null && defaultChangeFrequency["Value"] != Empty
                                              ? defaultChangeFrequency["Value"]
                                              : Empty;

            ChangeFrequenciesForTemplates = new Dictionary<string, string>();
            ChangeFrequenciesForItems = new Dictionary<string, string>();

            Item changeFrequencyFolder =
                _configurationItem.Database.GetItem(
                    _configurationItem.Paths.FullPath + "/"
                    + DynamicSitemapConfiguration.SitemapConfigurationChangeFrequenciesFolderName);

            if (changeFrequencyFolder != null && changeFrequencyFolder.HasChildren)
            {
                foreach (var item in changeFrequencyFolder.Children.ToList())
                {
                    if (item.TemplateID.ToString() == TemplateIds.ChangeFrequencyForTemplatesTemplateId)
                    {
                        if (item["Templates"] != Empty && item["Change Frequency"] != Empty)
                        {
                            var elements = item["Templates"].Split('|').ToList();
                            var changeFrequency = _configurationItem.Database.GetItem(item["Change Frequency"]);

                            if (changeFrequency != null)
                            {
                                var changeFrequencyValue = changeFrequency["Value"];

                                foreach (var element in elements)
                                {
                                    ChangeFrequenciesForTemplates.Add(element, changeFrequencyValue);
                                }
                            }
                        }
                    }

                    else if (item.TemplateID.ToString() == TemplateIds.ChangeFrequencyForItemsTemplateId)
                    {
                        if (item["Items"] != Empty && item["Change Frequency"] != Empty)
                        {
                            var elements = item["Items"].Split('|').ToList();
                            var changeFrequency = _configurationItem.Database.GetItem(item["Change Frequency"]);

                            if (changeFrequency != null)
                            {
                                var changeFrequencyValue = changeFrequency["Value"];

                                foreach (var element in elements)
                                {
                                    ChangeFrequenciesForItems.Add(element, changeFrequencyValue);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads all dynamic routes
        /// </summary>
        protected void ReadDynamicRoutes()
        {
            DynamicRoutes = new List<Item>();

            Item dynamicRoutesFolder =
                _configurationItem.Database.GetItem(
                    _configurationItem.Paths.FullPath + "/"
                    + DynamicSitemapConfiguration.SitemapConfigurationRoutesFolderName);

            if (dynamicRoutesFolder != null && dynamicRoutesFolder.HasChildren)
            {
                foreach (var item in dynamicRoutesFolder.Children.ToList())
                {
                    DynamicRoutes.Add(item);
                }
            }
        }

        /// <summary>
        /// String representation of configuration
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return "Sitemap configuration for site '" + Site.Name + "' (language " + Language.Name + ") - generated as "
                   + SitemapFilePath + " (items count: " + ItemsCount + ")";
        }

        /// <summary>
        /// Gets change frequency for specified Item
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <returns>Change frequency value</returns>
        public string GetChangeFrequency(Item item)
        {
            string result = Empty;

            if (!ChangeFrequenciesForItems.TryGetValue(item.ID.ToString(), out result))
            {
                if (!ChangeFrequenciesForTemplates.TryGetValue(item.TemplateID.ToString(), out result))
                {
                    result = ChangeFrequencyDefaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets priority for specified Item
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <returns>Priority value</returns>
        public string GetPriority(Item item)
        {
            string result = Empty;

            if (!PrioritiesForItems.TryGetValue(item.ID.ToString(), out result))
            {
                if (!PrioritiesForTemplates.TryGetValue(item.TemplateID.ToString(), out result))
                {
                    result = PriorityDefaultValue;
                }
            }

            return result;
        }
    }
}