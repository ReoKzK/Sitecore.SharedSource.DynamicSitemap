using System;

namespace Sitecore.SharedSource.DynamicSitemap.Constants
{
    /// <summary>
    /// Messages
    /// </summary>
    public static class Messages
    {
        public static readonly String Label = "DynamicSitemapGenerator: ";

        public static readonly String ExecutionInterrupted = Label + "There are no sitemap configurations, execution interrupted.";
        public static readonly String NoConfigurations = Label + "There are no sitemap configurations.";

        public static readonly String ExceptionWhileBuilding = Label + "Exception while building sitemap for {0} - {1}\n\n{2}";

        public static readonly String SitemapBuidSuccess = Label + "Sitemap generated - {0}";

        public static readonly String SitemapSubmitterCannotSubmit = Label + "Cannot submit sitemap to \"{0}\" - {1}";
        public static readonly String SitemapSubmitterExceptionWhileSubmit = Label + "Search engine submission \"{0}\" returns an error - {1} \n\n{2}";
    }
}
