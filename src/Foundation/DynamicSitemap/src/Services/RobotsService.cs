using Sitecore.SharedSource.DynamicSitemap.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sitecore.SharedSource.DynamicSitemap.Services
{
    public class RobotsService : IRobotsService
    {
        /// <summary>
        /// Registers sitemaps to robots.txt
        /// </summary>
        public virtual void RegisterSitemapToRobotsFile(List<string> sitemapUrls)
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
