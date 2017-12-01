using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Sitecore.SharedSource.DynamicSitemap.Repositories
{
    public class ItemsRepository : IItemsRepository
    {
        protected readonly Database _database;

        public ItemsRepository(Database database)
        {
            _database = database;
        }

        public virtual List<Item> GetItems(String rootPath, Language language)
        {
            var items = new List<Item>();

            using (new LanguageSwitcher(language.Name))
            {
                // - Add root Item -
                items.Add(_database.SelectSingleItem(rootPath));

                items.AddRange(
                    _database.SelectItems("fast:" + rootPath + "//*")
                        .ToList()
                );
            }

            return items;
        }

        public List<Item> GetItems()
        {
            return GetItems("/sitecore/content", Sitecore.Context.Language);
        }
    }
}
