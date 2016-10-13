using Sitecore.Diagnostics;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DynamicSitemap
{
    /// <summary>
    /// Dynamic Sitemap Manager Form
    /// </summary>
    public class DynamicSitemapManagerForm : BaseForm
    {
        /// <summary>
        /// Refresh Button
        /// </summary>
        protected Button RefreshButton;

        /// <summary>
        /// Message
        /// </summary>
        protected Literal Message;

        /// <summary>
        /// OnLoad
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Context.ClientPage.IsEvent)
            {
                this.RefreshButton.Click = "RefreshButtonClick";
            }
        }

        /// <summary>
        /// Refresh button method
        /// </summary>
        public void RefreshButtonClick()
        {
            DynamicSitemapGenerator dynamicSitemapGenerator = new DynamicSitemapGenerator();
            dynamicSitemapGenerator.RegenerateSitemap(this, new EventArgs());

            StringBuilder stringBuilder = new StringBuilder();

            String message = String.Empty;

            if (dynamicSitemapGenerator.SiteConfigurations == null || !dynamicSitemapGenerator.SiteConfigurations.Any())
            {
                this.Message.Text = "No sitemap configurations found under " + DynamicSitemapConfiguration.SitemapConfigurationItemPath + ". Please create one or more configuration nodes and try refreshing again.";
                DynamicSitemapManagerForm.RefreshPanel("MainPanel");
            }

            else
            {
                message += "Sitemaps for this sites has been generated: <br/><ul>";

                foreach (String configuration in dynamicSitemapGenerator.SiteConfigurations.Select(x => x.ToString()))
                {
                    message += String.Format("<li> &bull; {0}</li>", configuration);
                }
                
                message += "</ul>";

                this.Message.Text = message;
                DynamicSitemapManagerForm.RefreshPanel("MainPanel");
            }
        }

        /// <summary>
        /// Refreshes panel
        /// </summary>
        /// <param name="panelName"></param>
        private static void RefreshPanel(String panelName)
        {
            Panel panel = Context.ClientPage.FindControl(panelName) as Panel;
            Assert.IsNotNull(panel, "can't find panel");
            Context.ClientPage.ClientResponse.Refresh(panel);
        }
    }
}
