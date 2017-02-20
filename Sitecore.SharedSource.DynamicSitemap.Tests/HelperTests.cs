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
            String urlWithoutProtocol = "://old.host.dev/page?arg=1";

            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithHttp), urlWithHttp);
            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithHttps), urlWithHttps);

            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithoutProtocol), "http" + urlWithoutProtocol);
            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithoutProtocol, true), "https" + urlWithoutProtocol);
            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithHttp, true), "https" + urlWithoutProtocol);
            Assert.AreEqual(DynamicSitemapHelper.EnsureHttpPrefix(urlWithHttps, true), "https" + urlWithoutProtocol);
        }
    }
}
