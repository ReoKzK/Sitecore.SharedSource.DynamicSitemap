using NUnit.Framework;

using Sitecore.SharedSource.DynamicSitemap.Helpers;

using System;

namespace Sitecore.SharedSource.DynamicSitemap.Tests
{
    [TestFixture]
    public class HelperTests
    {
        [Test]
        public void TestReplaceHost()
        {
            String url = "http://old.host.dev/page?arg=1";
            String newHost = "new.host.dev";
            String newHostInvalidFormat = "http://new.host.dev/";
            String newUrl = "http://new.host.dev/page?arg=1";

            Assert.AreEqual(DynamicSitemapHelper.ReplaceHost(url, newHost), newUrl);
            Assert.AreEqual(DynamicSitemapHelper.ReplaceHost(url, newHostInvalidFormat), newUrl);
        }

        [Test]
        public void TestEnsureHttpPrefix()
        {
            String urlWithHttp = "http://old.host.dev/page?arg=1";
            String urlWithHttps = "https://old.host.dev/page?arg=1";
            String urlWithoutHttp = "://old.host.dev/page?arg=1";

            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithHttp), urlWithHttp);
            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithHttps), urlWithHttps);

            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithoutHttp), "http" + urlWithoutHttp);
            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithoutHttp, true), "https" + urlWithoutHttp);
            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithHttp, true), "https" + urlWithoutHttp);
        }
    }
}