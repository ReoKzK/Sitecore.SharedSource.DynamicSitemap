using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SharedSource.DynamicSitemap.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.SharedSource.DynamicSitemap.Repositories
{
    public class ItemsIndexRepository : IItemsRepository
    {
        public List<Item> GetItems(string rootPath, Language language)
        {
            List<SearchResultItem> searchResultItems;

            var index = ContentSearchManager.GetIndex(DynamicSitemapConfiguration.SitecoreIndex);

            using (var context = index.CreateSearchContext())
            {
                var searchQuery = context.GetQueryable<SearchResultItem>()
                    .Where(x => x.Language.Equals(language.Name))
                    .Where(x => x.Path.StartsWith(rootPath));

                searchResultItems = searchQuery.ToList();
            }

            return searchResultItems.Select(x => x.GetItem()).ToList();
        }

        public List<Item> GetItems()
        {
            return GetItems("/sitecore/content", Sitecore.Context.Language);
        }
    }
}
