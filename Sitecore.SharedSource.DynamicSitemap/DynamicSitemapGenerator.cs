using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using Sitecore.SharedSource.DynamicSitemap.Constants;
using Sitecore.SharedSource.DynamicSitemap.Extensions;
using Sitecore.SharedSource.DynamicSitemap.Helpers;
using Sitecore.SharedSource.DynamicSitemap.Interfaces;
using Sitecore.SharedSource.DynamicSitemap.Logic;
using Sitecore.SharedSource.DynamicSitemap.Model;
using Sitecore.SharedSource.DynamicSitemap.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Sitecore.SharedSource.DynamicSitemap
{
    /// <summary>
    /// Dynamic Sitemap Generator
    /// </summary>
    public class DynamicSitemapGenerator
    {
        /// <summary>
        /// Dynamic Sitemap configuration in Sitecore
        /// </summary>
        protected DynamicSitemapSitecoreConfiguration SitecoreConfiguration { get; set; }

        /// <summary>
        /// Sitemap Configurations List
        /// </summary>
        public List<SitemapSiteConfiguration> SiteConfigurations { get; private set; }

        /// <summary>
        /// Sitemap index
        /// </summary>
        protected SitemapIndexConfiguration SitemapIndex { get; set; }
        
        /// <summary>
        /// Sitemap file name format
        /// </summary>
        protected String _sitemapFileNameFormat = "sitemap-{0}-{1}.xml";

        /// <summary>
        /// Sitemap index file name
        /// </summary>
        protected String _sitemapIndexFileName = "sitemap.xml";

        /// <summary>
        /// Database
        /// </summary>
        public Database Database
        {
            get
            {
                return Sitecore.Configuration.Factory.GetDatabase(DynamicSitemapConfiguration.WorkingDatabase);
            }
        }
        
        /// <summary>
        /// Dynamic Sitemap Generator
        /// </summary>
        public DynamicSitemapGenerator()
        {
            SiteConfigurations = new List<SitemapSiteConfiguration>();
        }

        /// <summary>
        /// Regenerates sitemap for all configured sites
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void RegenerateSitemap(object sender, System.EventArgs args)
        {
            if (args == null)
            {
                return;
            }

            EnsureSitemapsDirectoryExists();

            ReadConfigurations();

            if (SiteConfigurations.Count == 0)
            {
                Sitecore.Diagnostics.Log.Warn(Messages.ExecutionInterrupted, this);
                return;
            }

            GenerateSitemaps();
            GenerateSitemapsIndex();

            RegisterSitemapToRobotsFile();

            if (DynamicSitemapConfiguration.IsProductionEnvironment)
            {
                var submitter = new SitemapSubmitter(SitecoreConfiguration, SiteConfigurations, Database);
                submitter.SubmitSitemapsToSearchEngines();
            }
        }

        /// <summary>
        /// Reads configurations from Sitecore
        /// </summary>
        public void ReadConfigurations()
        {
            ReadGlobalSitecoreConfiguration();

            Item[] configurationItems = Database.SelectItems(DynamicSitemapConfiguration.SitemapConfigurationItemPath + DynamicSitemapConfiguration.SitemapConfigurationSitesFolderName + "/*[@@templateid='" + TemplateIds.SiteConfigurationTemplateId + "']");

            if (configurationItems.Count() == 0)
            {
                Sitecore.Diagnostics.Log.Warn(Messages.NoConfigurations, this);
                return;
            }

            SiteConfigurations = new List<SitemapSiteConfiguration>();

            foreach (var configurationItem in configurationItems)
            {
                var languageItems = configurationItem.Languages.Where(x => SitecoreConfiguration.ProcessedLanguages.Contains(x.Name)).ToList();

                foreach (var languageItem in languageItems)
                {
                    var item = configurationItem.Database.GetItem(configurationItem.ID, languageItem);

                    if (item.Versions.Count > 0)
                    {
                        var site = configurationItem.Name.ToLower();

                        var sitemapSiteConfiguration = new SitemapSiteConfiguration(item);

                        sitemapSiteConfiguration.SitemapFileName = sitemapSiteConfiguration.SitemapFileName != String.Empty
                            ? sitemapSiteConfiguration.SitemapFileName
                            : String.Format(_sitemapFileNameFormat, site, languageItem.Name.ToLower());

                        sitemapSiteConfiguration.SitemapFilePath = DynamicSitemapConfiguration.SitemapConfigurationOutputFolder + "/" + sitemapSiteConfiguration.SitemapFileName;

                        // - Load ItemsProcessor -

                        if (!String.IsNullOrWhiteSpace(sitemapSiteConfiguration.ItemsProcessorTypeToLoad))
                        {
                            var loader = new ItemsProcessorLoader();
                            var itemsProcessor = loader.Load(sitemapSiteConfiguration.ItemsProcessorTypeToLoad);

                            if (itemsProcessor != null)
                            {
                                sitemapSiteConfiguration.ItemsProcessor = itemsProcessor;
                            }

                            else
                            {
                                Sitecore.Diagnostics.Log.Warn(String.Format(Messages.CannotLoadItemsProcessor, sitemapSiteConfiguration.ItemsProcessorTypeToLoad), this);
                            }
                        }
                        
                        SiteConfigurations.Add(sitemapSiteConfiguration);
                    }
                }
            }

            SitecoreConfiguration.MainSiteConfiguration = SiteConfigurations.FirstOrDefault(x => x.Site.Name.ToLower() == SitecoreConfiguration.MainSiteConfigurationItem.Name.ToLower());

            SitemapIndex = new SitemapIndexConfiguration();
            SitemapIndex.ServerHost = SitecoreConfiguration.MainSiteConfiguration != null
                ? SitecoreConfiguration.MainSiteConfiguration.ServerHost
                : SiteConfigurations.FirstOrDefault().ServerHost;
            SitemapIndex.FileName = _sitemapIndexFileName;
        }

        /// <summary>
        /// Read global configuration from Sitecore
        /// </summary>
        protected void ReadGlobalSitecoreConfiguration()
        {
            Item globalConfigurationItem = Database.GetItem(DynamicSitemapConfiguration.SitemapConfigurationItemPath + "/Configuration");

            if (globalConfigurationItem == null)
            {
                Sitecore.Diagnostics.Log.Error(Messages.NoGlobalScConfiguration, this);
                return;
            }

            Item mainSiteConfiguration = null;

            if (globalConfigurationItem["Main Site Configuration"] != String.Empty)
            {
                mainSiteConfiguration = Database.GetItem(globalConfigurationItem["Main Site Configuration"]);
            }

            SitecoreConfiguration = new DynamicSitemapSitecoreConfiguration();

            if (mainSiteConfiguration != null)
            {
                SitecoreConfiguration.MainSiteConfigurationItem = mainSiteConfiguration;
            }

            SitecoreConfiguration.SearchEngines = !String.IsNullOrEmpty(globalConfigurationItem["Search Engines"])
                ? globalConfigurationItem["Search Engines"].Split('|').ToList()
                : new List<String>();

            SitecoreConfiguration.ProcessedLanguages = new List<String>();

            if (!String.IsNullOrEmpty(globalConfigurationItem["Processed languages"]))
            {
                var itemIds = globalConfigurationItem["Processed languages"].Split('|').ToList();

                foreach (var itemId in itemIds)
                {
                    var item = Database.GetItem(itemId);

                    if (item != null)
                    {
                        SitecoreConfiguration.ProcessedLanguages.Add(item.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Generate sitemaps into the file system
        /// </summary>
        protected void GenerateSitemaps()
        {
            foreach (var sitemapSiteConfiguration in SiteConfigurations)
            {
                var sitemapContent = BuildSitemap(sitemapSiteConfiguration);

                string path = Sitecore.MainUtil.MapPath(sitemapSiteConfiguration.SitemapFilePath);

                StreamWriter streamWriter = new StreamWriter(path, false);
                streamWriter.Write(sitemapContent);
                streamWriter.Close();
            }
        }

        /// <summary>
        /// Builds sitemap structure
        /// </summary>
        /// <param name="sitemapSiteConfiguration">Sitemap site configuration</param>
        /// <returns>Sitemap content</returns>
        public String BuildSitemap(SitemapSiteConfiguration sitemapSiteConfiguration)
        {
            var result = String.Empty;

            var options = GetUrlOptions();

            var encoding = Encoding.UTF8;
            StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(encoding);

            // - Creating the XML Header -
           
            var xml = new XmlTextWriter(stringWriter);
            xml.WriteStartDocument();
            xml.WriteStartElement("urlset", DynamicSitemapConfiguration.XmlnsTpl);

            try
            {
                options.Site = sitemapSiteConfiguration.Site;
                options.Language = sitemapSiteConfiguration.Language;

                List<Item> items = GetItems(sitemapSiteConfiguration.Site.RootPath, sitemapSiteConfiguration.Language);
                
                ProcessItems(items, sitemapSiteConfiguration, options, xml);
            }

            catch (Exception exc)
            {
                Sitecore.Diagnostics.Log.Error(String.Format(Messages.ExceptionWhileBuilding, sitemapSiteConfiguration.Site.Name, exc.Message, exc.StackTrace) , this);
            }

            finally
            {
                xml.WriteEndElement();
                xml.WriteEndDocument();
                xml.Flush();

                result = stringWriter.ToString();

                Sitecore.Diagnostics.Log.Info(String.Format(Messages.SitemapBuildSuccess, sitemapSiteConfiguration), this);
            }

            return result;
        }

        /// <summary>
        /// Generates sitemaps index
        /// </summary>
        protected void GenerateSitemapsIndex()
        {
            if (!DynamicSitemapConfiguration.UseSitemapsIndexFile)
            {
                return;
            }

            var encoding = Encoding.UTF8;
            StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(encoding);

            // - Creating the XML Header -

            var xml = new XmlTextWriter(stringWriter);
            xml.WriteStartDocument();
            xml.WriteStartElement("sitemapindex", DynamicSitemapConfiguration.XmlnsTpl);

            int sitemapsCount = 0;

            try
            {
                foreach (var sitemap in SiteConfigurations)
                {
                    xml.WriteStartElement("sitemap");
                    xml.WriteElementString("loc", sitemap.SitemapUrl);

                    var lastModified = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:sszzz");

                    xml.WriteElementString("lastmod", lastModified);
                    xml.WriteEndElement();

                    sitemapsCount++;
                }
            }

            catch (Exception exc)
            {
                Sitecore.Diagnostics.Log.Error("DynamicSitemapGenerator: " + exc.Message + "\n\n" + exc.StackTrace, this);
            }

            finally
            {
                xml.WriteEndElement();
                xml.WriteEndDocument();
                xml.Flush();

                String result = stringWriter.ToString();
                
                string path = Sitecore.MainUtil.MapPath("/" + SitemapIndex.FileName);

                StreamWriter streamWriter = new StreamWriter(path, false);
                streamWriter.Write(result);
                streamWriter.Close();
                
                Sitecore.Diagnostics.Log.Info("DynamicSitemapGenerator: Sitemap index generated - in path " + path + ", " + sitemapsCount + " sitemaps attached", this);
            }
        }

        /// <summary>
        /// Gets all items in path
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        protected List<Item> GetItems(String rootPath, Language language)
        {
            var items = new List<Item>();

            using (new LanguageSwitcher(language.Name))
            {
                // - Add root Item -
                items.Add(Database.SelectSingleItem(rootPath));

                items.AddRange(
                    Database.SelectItems("fast:" + rootPath + "//*")
                        .ToList()
                );
            }
            
            return items;
        }

        /// <summary>
        /// Processes all items in site under root path
        /// </summary>
        /// <param name="items">List of Items</param>
        /// <param name="sitemapSiteConfiguration">Current sitemap configuration</param>
        /// <param name="options">Url Options</param>
        /// <param name="xml">Xml Text Writer object</param>
        public void ProcessItems(List<Item> items, SitemapSiteConfiguration sitemapSiteConfiguration, UrlOptions options, XmlTextWriter xml)
        {
            foreach (var item in items)
            {
                if (item.Versions.Count > 0)
                {
                    if (DynamicSitemapHelper.IsWildcard(item))
                    {
                        PrepareDynamicItems(item, sitemapSiteConfiguration, xml);
                    }

                    else if (IsIncluded(item, sitemapSiteConfiguration))
                    {
                        var url = LinkManager.GetItemUrl(item, options);
                        url = DynamicSitemapHelper.EnsureHttpPrefix(url, sitemapSiteConfiguration.ForceHttps);

                        if (!String.IsNullOrEmpty(sitemapSiteConfiguration.ServerHost))
                        {
                            url = DynamicSitemapHelper.ReplaceHost(url, sitemapSiteConfiguration.ServerHost);
                        }

                        GenerateUrlElement(url, item, sitemapSiteConfiguration, xml);
                    }
                }
            }

            if (sitemapSiteConfiguration.ItemsProcessor != null)
            {
                var urlItems = sitemapSiteConfiguration.ItemsProcessor.ProcessItems(sitemapSiteConfiguration);

                foreach (var urlItem in urlItems)
                {
                    GenerateUrlElement(urlItem, sitemapSiteConfiguration, xml);
                }
            }
        }

        /// <summary>
        /// Prepares dynamic items - items accessed by wildcard
        /// </summary>
        /// <param name="wildcardItem">Wildcard Item</param>
        /// <param name="sitemapSiteConfiguration">Sitemap site configuration</param>
        /// <param name="xml">XmlTextWriter object</param>
        protected void PrepareDynamicItems(Item wildcardItem, SitemapSiteConfiguration sitemapSiteConfiguration, XmlTextWriter xml)
        {
            var dynamicRoute = sitemapSiteConfiguration.DynamicRoutes.SingleOrDefault(x => x["Dynamic Item"] == wildcardItem.ID.ToString());

            if (dynamicRoute != null)
            {
                var datasource = Database.GetItem(dynamicRoute["Data Source"]);

                if (datasource != null && datasource.HasChildren)
                {
                    UrlOptions options = GetUrlOptions();
                    options.Site = sitemapSiteConfiguration.Site;

                    var dynamicItemActualUrl = LinkManager.GetItemUrl(wildcardItem, options);

                    foreach (var item in datasource.Children.ToList())
                    {
                        if (item.Versions.Count > 0 && IsIncluded(item, sitemapSiteConfiguration, true))
                        {
                            var lastSegment = item.Name;
                            lastSegment = options.LowercaseUrls ? lastSegment.ToLower() : lastSegment;

                            var url = dynamicItemActualUrl
                                .Replace(",-w-,", lastSegment)
                                .Replace("*", lastSegment);

                            url = DynamicSitemapHelper.EnsureHttpPrefix(url, sitemapSiteConfiguration.ForceHttps);

                            if (!String.IsNullOrEmpty(sitemapSiteConfiguration.ServerHost))
                            {
                                url = DynamicSitemapHelper.ReplaceHost(url, sitemapSiteConfiguration.ServerHost);
                            }
                            
                            GenerateUrlElement(url, item, sitemapSiteConfiguration, xml);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets default UrlOptions
        /// </summary>
        /// <returns></returns>
        protected UrlOptions GetUrlOptions()
        {
            var urlOptions = LinkManager.GetDefaultUrlOptions();
            urlOptions.AlwaysIncludeServerUrl = true;
            urlOptions.SiteResolving = true;

            return urlOptions;
        }

        /// <summary>
        /// Generates url element
        /// </summary>
        /// <param name="url"></param>
        /// <param name="item"></param>
        /// <param name="sitemapSiteConfiguration"></param>
        /// <param name="xml"></param>
        protected void GenerateUrlElement(String url, Item item, SitemapSiteConfiguration sitemapSiteConfiguration, XmlTextWriter xml)
        {
            sitemapSiteConfiguration.ItemsCount++;

            xml.WriteStartElement("url");
            xml.WriteElementString("loc", url);

            if (item != null)
            {
                var lastModified = item.Statistics.Updated.ToString("yyyy-MM-ddThh:mm:sszzz");

                xml.WriteElementString("lastmod", lastModified);

                String changeFrequency = sitemapSiteConfiguration.GetChangeFrequency(item);
                if (changeFrequency != String.Empty)
                {
                    xml.WriteElementString("changefreq", changeFrequency);
                }

                String priority = sitemapSiteConfiguration.GetPriority(item);
                if (priority != String.Empty)
                {
                    xml.WriteElementString("priority", priority);
                }
            }
            
            xml.WriteEndElement();
        }

        protected void GenerateUrlElement(UrlElement urlElement, SitemapSiteConfiguration sitemapSiteConfiguration, XmlTextWriter xml)
        {
            sitemapSiteConfiguration.ItemsCount++;

            xml.WriteStartElement("url");
            xml.WriteElementString("loc", urlElement.Location);
            
            var lastModified = urlElement.LastModification.ToString("yyyy-MM-ddThh:mm:sszzz");

            xml.WriteElementString("lastmod", lastModified);
            
            if (urlElement.ChangeFrequency != String.Empty)
            {
                xml.WriteElementString("changefreq", urlElement.ChangeFrequency);
            }
            
            if (urlElement.Priority != String.Empty)
            {
                xml.WriteElementString("priority", urlElement.Priority);
            }

            xml.WriteEndElement();
        }

        /// <summary>
        /// Checks if Item can be included in sitemap
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="isDataSourceItem">Is item used only in wildcard</param>
        /// <returns>true if included</returns>
        protected bool IsIncluded(Item item, SitemapSiteConfiguration sitemapSiteConfiguration, bool isDataSourceItem = false)
        {
            var result = false;

            if (!sitemapSiteConfiguration.ExcludedItems.Any(x => x == item.ID.ToString())
                && sitemapSiteConfiguration.IncludedTemplates.Contains(item.TemplateID.ToString())
                && !sitemapSiteConfiguration.ExcludedItemPaths.Any(x => item.Paths.FullPath.ToLower().StartsWith(x.Paths.FullPath.ToLower()) || item.Paths.FullPath.ToLower().Equals(x.Paths.FullPath.ToLower()))
                && (item.Paths.FullPath.StartsWith(sitemapSiteConfiguration.RootItem.Paths.FullPath)
                    || item.Paths.FullPath.Equals(sitemapSiteConfiguration.RootItem.Paths.FullPath)
                    || isDataSourceItem)) // - datasource items can be out of root item
            {
                result = true;
            }

            return result;
        }
        
        /// <summary>
        /// Ensures that sitemaps directory exists
        /// </summary>
        protected static void EnsureSitemapsDirectoryExists()
        {
            String dirPath = Sitecore.MainUtil.MapPath(DynamicSitemapConfiguration.SitemapConfigurationOutputFolder + "/");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
        
        /// <summary>
        /// Registers sitemaps to robots.txt
        /// </summary>
        public void RegisterSitemapToRobotsFile()
        {
            if (DynamicSitemapConfiguration.RefreshRobotsFile)
            {
                String robotsPath = MainUtil.MapPath("/" + "robots.txt");

                StringBuilder stringBuilder = new StringBuilder(String.Empty);

                if (File.Exists(robotsPath))
                {
                    StreamReader streamReader = new StreamReader(robotsPath);
                    stringBuilder.Append(streamReader.ReadToEnd());
                    streamReader.Close();
                }

                StreamWriter streamWriter = new StreamWriter(robotsPath, false);

                List<String> sitemapUrls = new List<String>();

                if (DynamicSitemapConfiguration.UseSitemapsIndexFile)
                {
                    sitemapUrls.Add(SitemapIndex.Url);
                }

                else
                {
                    sitemapUrls.AddRange(SiteConfigurations.Select(x => x.SitemapUrl));
                }

                foreach (var url in sitemapUrls)
                {
                    String value = "Sitemap: " + url;

                    if (!stringBuilder.ToString().Contains(value))
                    {
                        if (!stringBuilder.ToString().EndsWith(Environment.NewLine) && stringBuilder.ToString().Trim() != String.Empty)
                        {
                            stringBuilder.AppendLine();
                        }

                        stringBuilder.AppendLine(value);
                    }
                }

                streamWriter.Write(stringBuilder.ToString());
                streamWriter.Close();
            }
        }
    }
}
