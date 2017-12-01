using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Sitecore.SharedSource.DynamicSitemap.Repositories
{
    public interface IItemsRepository
    {
        List<Item> GetItems(string rootPath, Language language);

        List<Item> GetItems();
    }
}