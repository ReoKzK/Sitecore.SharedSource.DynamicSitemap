namespace Sitecore.SharedSource.DynamicSitemap
{
    using System;
    using System.Collections.Generic;

    using Sitecore.Data;
    using Sitecore.Data.Items;

    public static class TemplateHelper
    {
        public static List<Guid> GetBaseTemplates(Item item)
        {
            List<Guid> list = new List<Guid>();
            GetBaseTemplates(item.Template, list);
            return list;
        }

        private static void GetBaseTemplates(TemplateItem template, List<Guid> list)
        {
            list.Add(template.ID.Guid);
            foreach (TemplateItem templateItem in template.BaseTemplates)
            {
                if (list.Contains(templateItem.ID.Guid))
                {
                    continue;
                }
                GetBaseTemplates(templateItem, list);
            }
        }
    }
}