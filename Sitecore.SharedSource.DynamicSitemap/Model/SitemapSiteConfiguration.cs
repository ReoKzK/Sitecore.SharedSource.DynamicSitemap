using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using Sitecore.SharedSource.DynamicSitemap.Constants;
using Sitecore.SharedSource.DynamicSitemap.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.SharedSource.DynamicSitemap.Model
{
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
        public List<String> ExcludedPaths { get; private set; }

        /// <summary>
        /// Included templates
        /// </summary>
        public List<string> IncludedTemplates { get; private set; }

        /// <summary>
        /// Excluded Item paths
        /// </summary>
        public List<Item> ExcludedItemPaths { get; private set; }

        /// <summary>
        /// Search engines
        /// </summary>
        public List<String> SearchEngines { get; private set; }

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
        public String LanguageName
        {
            get
            {
                return Language != null ? Language.Name : String.Empty;
            }
        }

        /// <summary>
        /// Sitemap file name
        /// </summary>
        public String SitemapFileName { get; set; }

        /// <summary>
        /// Sitemap file path
        /// </summary>
        public String SitemapFilePath { get; set; }

        /// <summary>
        /// Forces generating https urls
        /// </summary>
        public Boolean ForceHttps { get; set; }

        /// <summary>
        /// Change frequencies for templates
        /// </summary>
        public Dictionary<String, String> ChangeFrequenciesForTemplates { get; set; }

        /// <summary>
        /// Change frequencies for items
        /// </summary>
        public Dictionary<String, String> ChangeFrequenciesForItems { get; set; }

        /// <summary>
        /// Change frequency default value
        /// </summary>
        public String ChangeFrequencyDefaultValue { get; private set; }

        /// <summary>
        /// Priorities for templates
        /// </summary>
        public Dictionary<String, String> PrioritiesForTemplates { get; set; }

        /// <summary>
        /// Priorities for items
        /// </summary>
        public Dictionary<String, String> PrioritiesForItems { get; set; }

        /// <summary>
        /// Priority default value
        /// </summary>
        public String PriorityDefaultValue { get; private set; }

        /// <summary>
        /// Items processor type to load
        /// </summary>
        public String ItemsProcessorTypeToLoad { get; private set; }

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
        public String SitemapUrl
        {
            get
            {
                var url = String.Empty;

                if (Site.HostName == String.Empty)
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
                    url = !String.IsNullOrEmpty(ServerHost)
                        ? ServerHost
                        : Site.HostName;
                }

                url = url.Replace("//", "/");
                url = !url.StartsWith("http://") 
                    ? "http://" + url 
                    : url;
                
                url += SitemapFilePath.Replace("//", "/");

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

            IncludedTemplates = !String.IsNullOrEmpty(configurationItem["Included Templates"])
                ? configurationItem["Included Templates"].Split('|').ToList()
                : new List<String>();

            ExcludedItems = !String.IsNullOrEmpty(configurationItem["Excluded Items"])
                ? configurationItem["Excluded Items"].Split('|').ToList()
                : new List<String>();

            ExcludedPaths = !String.IsNullOrEmpty(configurationItem["Excluded Paths"])
                ? configurationItem["Excluded Paths"].Split('|').ToList()
                : new List<String>();

            SearchEngines = !String.IsNullOrEmpty(configurationItem["Search Engines"])
                ? configurationItem["Search Engines"].Split('|').ToList()
                : new List<String>();

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
            
            PrioritiesForTemplates = new Dictionary<String, String>();
            PrioritiesForItems = new Dictionary<String, String>();

            Item prioritiesFolder = _configurationItem.Database.GetItem(_configurationItem.Paths.FullPath + "/" + DynamicSitemapConfiguration.SitemapConfigurationPrioritiesFolderName);

            if (prioritiesFolder != null && prioritiesFolder.HasChildren)
            {
                foreach (var item in prioritiesFolder.Children.ToList())
                {
                    if (item.TemplateID.ToString() == TemplateIds.PriorityForTemplatesTemplateId)
                    {
                        if (item["Templates"] != String.Empty && item["Priority"] != String.Empty)
                        {
                            var elements = item["Templates"].Split('|').ToList();
                            var priority = item["Priority"];

                            if (priority != String.Empty)
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
                        if (item["Items"] != String.Empty && item["Priority"] != String.Empty)
                        {
                            var elements = item["Items"].Split('|').ToList();
                            var priority = item["Priority"];

                            if (priority != String.Empty)
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

            ChangeFrequencyDefaultValue = defaultChangeFrequency != null && defaultChangeFrequency["Value"] != String.Empty
                ? defaultChangeFrequency["Value"]
                : String.Empty;

            ChangeFrequenciesForTemplates = new Dictionary<String, String>();
            ChangeFrequenciesForItems = new Dictionary<String, String>();

            Item changeFrequencyFolder = _configurationItem.Database.GetItem(_configurationItem.Paths.FullPath + "/" + DynamicSitemapConfiguration.SitemapConfigurationChangeFrequenciesFolderName);

            if (changeFrequencyFolder != null && changeFrequencyFolder.HasChildren)
            {
                foreach (var item in changeFrequencyFolder.Children.ToList())
                {
                    if (item.TemplateID.ToString() == TemplateIds.ChangeFrequencyForTemplatesTemplateId)
                    {
                        if (item["Templates"] != String.Empty && item["Change Frequency"] != String.Empty)
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
                        if (item["Items"] != String.Empty && item["Change Frequency"] != String.Empty)
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
            
            Item dynamicRoutesFolder = _configurationItem.Database.GetItem(_configurationItem.Paths.FullPath + "/" + DynamicSitemapConfiguration.SitemapConfigurationRoutesFolderName);

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
            return "Sitemap configuration for site '" + Site.Name + "' (language " + Language.Name  + ") - generated as " + SitemapFilePath + " (items count: " + ItemsCount + ")";
        }

        /// <summary>
        /// Gets change frequency for specified Item
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <returns>Change frequency value</returns>
        public String GetChangeFrequency(Item item)
        {
            String result = String.Empty;

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
        public String GetPriority(Item item)
        {
            String result = String.Empty;

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
