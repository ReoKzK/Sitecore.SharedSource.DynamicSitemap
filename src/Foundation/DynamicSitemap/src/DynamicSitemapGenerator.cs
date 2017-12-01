using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using Sitecore.SharedSource.DynamicSitemap.Constants;
using Sitecore.SharedSource.DynamicSitemap.Extensions;
using Sitecore.SharedSource.DynamicSitemap.Logic;
using Sitecore.SharedSource.DynamicSitemap.Model;
using Sitecore.SharedSource.DynamicSitemap.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Sitecore.SharedSource.DynamicSitemap.Repositories;
using Sitecore.SharedSource.DynamicSitemap.Services;

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
        /// Items repository
        /// </summary>
        protected IItemsRepository _itemsRepository;

        /// <summary>
        /// Service for building Sitemap XML structure
        /// </summary>
        protected ISitemapBuildingService _sitemapBuildingService;

        /// <summary>
        /// Service for processing items into Sitemap XML url elements
        /// </summary>
        protected IItemsProcessingService _itemsProcessingService;

        /// <summary>
        /// Dynamic Sitemap Generator
        /// </summary>
        public DynamicSitemapGenerator()
        {
            SiteConfigurations = new List<SitemapSiteConfiguration>();
            _itemsRepository = new ItemsRepository(this.Database);
            _sitemapBuildingService = new SitemapBuildingService();
            _itemsProcessingService = new ItemsProcessingService();
        }

        public DynamicSitemapGenerator(IItemsRepository itemsRepository, ISitemapBuildingService sitemapBuildingService, IItemsProcessingService itemsProcessingService)
        {
            SiteConfigurations = new List<SitemapSiteConfiguration>();
            _itemsRepository = itemsRepository;
            _sitemapBuildingService = sitemapBuildingService;
            _itemsProcessingService = itemsProcessingService;
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
        public virtual void ReadConfigurations()
        {
            ReadGlobalSitecoreConfiguration();

            SiteConfigurations = new List<SitemapSiteConfiguration>();

            if (SitecoreConfiguration.DisableSitemap)
            {
                Sitecore.Diagnostics.Log.Warn(Messages.SitemapDisabled, this);
                return;
            }

            Item[] configurationItems = Database.SelectItems(DynamicSitemapConfiguration.SitemapConfigurationItemPath + DynamicSitemapConfiguration.SitemapConfigurationSitesFolderName + "/*[@@templateid='" + TemplateIds.SiteConfigurationTemplateId + "']");

            if (!configurationItems.Any())
            {
                Sitecore.Diagnostics.Log.Warn(Messages.NoConfigurations, this);
                return;
            }
            
            if (!SitecoreConfiguration.ProcessedLanguages.Any())
            {
                Sitecore.Diagnostics.Log.Warn(Messages.NoProcessedLanguages, this);
                return;
            }
            
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
        protected virtual void ReadGlobalSitecoreConfiguration()
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

            SitecoreConfiguration.DisableSitemap = globalConfigurationItem["Disable sitemap generation"] == "1";

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
        protected virtual void GenerateSitemaps()
        {
            foreach (var sitemapSiteConfiguration in SiteConfigurations)
            {
                var items = _itemsRepository.GetItems(sitemapSiteConfiguration.Site.RootPath, sitemapSiteConfiguration.Language);
                var urlElements = _itemsProcessingService.ProcessItems(items, sitemapSiteConfiguration, GetUrlOptions()); // TODO: watch for url options issues
                
                var sitemapContent = _sitemapBuildingService.BuildSitemap(sitemapSiteConfiguration, urlElements);

                string path = Sitecore.MainUtil.MapPath(sitemapSiteConfiguration.SitemapFilePath);

                StreamWriter streamWriter = new StreamWriter(path, false);
                streamWriter.Write(sitemapContent);
                streamWriter.Close();
            }
        }
        
        /// <summary>
        /// Generates sitemaps index
        /// </summary>
        protected virtual void GenerateSitemapsIndex()
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

        
        // TODO:
        /// <summary>
        /// Prepares dynamic items - items accessed by wildcard
        /// </summary>
        /// <param name="wildcardItem">Wildcard Item</param>
        /// <param name="sitemapSiteConfiguration">Sitemap site configuration</param>
        /// <param name="xml">XmlTextWriter object</param>
        //protected virtual void PrepareDynamicItems(Item wildcardItem, SitemapSiteConfiguration sitemapSiteConfiguration, XmlTextWriter xml)
        //{
        //    var dynamicRoute = sitemapSiteConfiguration.DynamicRoutes.SingleOrDefault(x => x["Dynamic Item"] == wildcardItem.ID.ToString());

        //    if (dynamicRoute != null)
        //    {
        //        var datasource = Database.GetItem(dynamicRoute["Data Source"]);

        //        if (datasource != null && datasource.HasChildren)
        //        {
        //            UrlOptions options = GetUrlOptions();
        //            options.Site = sitemapSiteConfiguration.Site;

        //            var dynamicItemActualUrl = LinkManager.GetItemUrl(wildcardItem, options);

        //            foreach (var item in datasource.Children.ToList())
        //            {
        //                if (item.Versions.Count > 0 && IsIncluded(item, sitemapSiteConfiguration, true))
        //                {
        //                    var lastSegment = item.Name;
        //                    lastSegment = options.LowercaseUrls ? lastSegment.ToLower() : lastSegment;

        //                    var url = dynamicItemActualUrl
        //                        .Replace(",-w-,", lastSegment)
        //                        .Replace("*", lastSegment);

        //                    url = DynamicSitemapHelper.EnsureHttpPrefix(url, sitemapSiteConfiguration.ForceHttps);

        //                    if (!String.IsNullOrEmpty(sitemapSiteConfiguration.ServerHost))
        //                    {
        //                        url = DynamicSitemapHelper.ReplaceHost(url, sitemapSiteConfiguration.ServerHost);
        //                    }
                            
        //                    GenerateUrlElement(url, item, sitemapSiteConfiguration, xml);
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Gets default UrlOptions
        /// </summary>
        /// <returns></returns>
        protected virtual UrlOptions GetUrlOptions()
        {
            var urlOptions = LinkManager.GetDefaultUrlOptions();
            urlOptions.AlwaysIncludeServerUrl = true;
            urlOptions.SiteResolving = true;

            return urlOptions;
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
        public virtual void RegisterSitemapToRobotsFile()
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
