using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using Sitecore.SharedSource.DynamicSitemap.Constants;
using Sitecore.SharedSource.DynamicSitemap.Extensions;
using Sitecore.SharedSource.DynamicSitemap.Model;

namespace Sitecore.SharedSource.DynamicSitemap.Services
{
    public class SitemapBuildingService : ISitemapBuildingService
    {
        /// <summary>
        /// Builds sitemap structure
        /// </summary>
        /// <param name="sitemapSiteConfiguration">Sitemap site configuration</param>
        /// <param name="elements"></param>
        /// <returns>Sitemap content as string</returns>
        public virtual String BuildSitemap(SitemapSiteConfiguration sitemapSiteConfiguration, List<UrlElement> elements)
        {
            var result = String.Empty;

            //var options = GetUrlOptions();

            var encoding = Encoding.UTF8;
            StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(encoding);

            // - Creating the XML Header -

            var xml = new XmlTextWriter(stringWriter);
            xml.WriteStartDocument();
            xml.WriteStartElement("urlset", DynamicSitemapConfiguration.XmlnsTpl);

            try
            {
                //options.Site = sitemapSiteConfiguration.Site;
                //options.Language = sitemapSiteConfiguration.Language;

                //List<Item> items = _itemsRepository.GetItems(sitemapSiteConfiguration.Site.RootPath, sitemapSiteConfiguration.Language);
                foreach (var urlElement in elements)
                {
                    WriteUrlElement(urlElement, sitemapSiteConfiguration, xml);
                }
            }

            catch (Exception exc)
            {
                Sitecore.Diagnostics.Log.Error(String.Format(Messages.ExceptionWhileBuilding, sitemapSiteConfiguration.Site.Name, exc.Message, exc.StackTrace), this);
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
        /// Generates url element
        /// </summary>
        /// <param name="urlElement"></param>
        /// <param name="sitemapSiteConfiguration"></param>
        /// <param name="xml"></param>
        protected virtual void WriteUrlElement(UrlElement urlElement, SitemapSiteConfiguration sitemapSiteConfiguration, XmlTextWriter xml)
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
        /// Processes all items in site under root path
        /// </summary>
        /// <param name="items">List of Items</param>
        /// <param name="sitemapSiteConfiguration">Current sitemap configuration</param>
        /// <param name="options">Url Options</param>
        /// <param name="xml">Xml Text Writer object</param>
        //public virtual void ProcessElements(List<UrlElement> items, SitemapSiteConfiguration sitemapSiteConfiguration, UrlOptions options, XmlTextWriter xml)
        //{
        //    foreach (var item in items)
        //    {
        //        if (item.Versions.Count > 0)
        //        {
        //            if (DynamicSitemapHelper.IsWildcard(item))
        //            {
        //                PrepareDynamicItems(item, sitemapSiteConfiguration, xml);
        //            }

        //            else if (IsIncluded(item, sitemapSiteConfiguration))
        //            {
        //                var url = LinkManager.GetItemUrl(item, options);
        //                url = DynamicSitemapHelper.EnsureHttpPrefix(url, sitemapSiteConfiguration.ForceHttps);

        //                if (!String.IsNullOrEmpty(sitemapSiteConfiguration.ServerHost))
        //                {
        //                    url = DynamicSitemapHelper.ReplaceHost(url, sitemapSiteConfiguration.ServerHost);
        //                }

        //                WriteUrlElement(url, item, sitemapSiteConfiguration, xml);
        //            }
        //        }
        //    }

        //    if (sitemapSiteConfiguration.ItemsProcessor != null)
        //    {
        //        var urlItems = sitemapSiteConfiguration.ItemsProcessor.ProcessItems(sitemapSiteConfiguration);

        //        foreach (var urlItem in urlItems)
        //        {
        //            WriteUrlElement(urlItem, sitemapSiteConfiguration, xml);
        //        }
        //    }
        //}

        /// <summary>
        /// Generates url element
        /// </summary>
        /// <param name="url"></param>
        /// <param name="item"></param>
        /// <param name="sitemapSiteConfiguration"></param>
        /// <param name="xml"></param>
        //protected virtual void WriteUrlElement(String url, Item item, SitemapSiteConfiguration sitemapSiteConfiguration, XmlTextWriter xml)
        //{
        //    sitemapSiteConfiguration.ItemsCount++;

        //    xml.WriteStartElement("url");
        //    xml.WriteElementString("loc", url);

        //    if (item != null)
        //    {
        //        var lastModified = item.Statistics.Updated.ToString("yyyy-MM-ddThh:mm:sszzz");

        //        xml.WriteElementString("lastmod", lastModified);

        //        String changeFrequency = sitemapSiteConfiguration.GetChangeFrequency(item);
        //        if (changeFrequency != String.Empty)
        //        {
        //            xml.WriteElementString("changefreq", changeFrequency);
        //        }

        //        String priority = sitemapSiteConfiguration.GetPriority(item);
        //        if (priority != String.Empty)
        //        {
        //            xml.WriteElementString("priority", priority);
        //        }
        //    }

        //    xml.WriteEndElement();
        //}
    }
}
