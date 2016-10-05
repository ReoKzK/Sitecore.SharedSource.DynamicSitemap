using Sitecore.SharedSource.DynamicSitemap.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DynamicSitemap.Modules
{
    /// <summary>
    /// Items processor assmebly loader
    /// </summary>
    public class ItemsProcessorLoader
    {
        /// <summary>
        /// Loads items processor
        /// </summary>
        /// <param name="assemblyDeclaration">Assembly nad type declaration ("Type, AssemblyName")</param>
        /// <returns></returns>
        public IItemsProcessor Load(String assemblyDeclaration)
        {
            IItemsProcessor itemsProcessor;

            try
            {
                var assmbl = assemblyDeclaration.Split(',');

                String type = assmbl[0].Trim();
                String assmebly = assmbl[1].Trim();

                // Use the file name to load the assembly into the current application domain.
                Assembly a = Assembly.Load(assmebly);

                // Get the type to use.
                Type myType = a.GetType(type);

                itemsProcessor = Activator.CreateInstance(myType) as IItemsProcessor;
            }

            catch (Exception e)
            {

                throw;
            }

            return itemsProcessor;
        }
    }
}
