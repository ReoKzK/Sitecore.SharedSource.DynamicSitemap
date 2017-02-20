using NUnit.Framework;
using Sitecore.SharedSource.DynamicSitemap.Extensions;
using System;

namespace Sitecore.SharedSource.DynamicSitemap.Tests
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void TestReplaceFirst()
        {
            String url = "http://old.host.dev/page?arg=1";
            String expectedResult = "https://old.host.dev/page?arg=1";
            
            String urlWithTwoOccurences = "http://old.host.http.dev/page?arg=1";
            String expectedResultWithTwoOccurences = "https://old.host.http.dev/page?arg=1";

            Assert.AreEqual(url.ReplaceFirst("http", "https"), expectedResult);
            Assert.AreEqual(urlWithTwoOccurences.ReplaceFirst("http", "https"), expectedResultWithTwoOccurences);
        }
    }
}
