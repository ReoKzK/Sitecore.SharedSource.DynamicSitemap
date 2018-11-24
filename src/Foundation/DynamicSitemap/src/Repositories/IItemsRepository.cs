using Sitecore.Data.Items;
using Sitecore.Globalization;
using System.Collections.Generic;

namespace Sitecore.SharedSource.DynamicSitemap.Repositories
{
    public interface IItemsRepository
    {
        List<Item> GetItems(string rootPath, Language language);

        List<Item> GetItems();
    }
}