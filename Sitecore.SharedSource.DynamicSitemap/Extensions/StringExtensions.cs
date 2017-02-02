using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.DynamicSitemap.Extensions
{
    /// <summary>
    /// Strin extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces first occurence of given string
        /// </summary>
        /// <param name="text">String to process</param>
        /// <param name="search">String to search</param>
        /// <param name="replace">String to replace</param>
        /// <returns>Processed string</returns>
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}