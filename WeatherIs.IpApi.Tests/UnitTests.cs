using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace WeatherIs.IpApi.Tests
{
    public class Tests
    {
        private IpApiEndpoint _client;

        [OneTimeSetUp]
        public void Setup()
        {
            _client = new IpApiEndpoint();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        [Test]
        [TestCase("1.1.1.1")]
        [TestCase("8.8.8.8")]
        [TestCase("aeris.dev")]
        [TestCase("delightedcat.net")]
        [TestCase("ip-api.com")]
        public async Task GetIpGeolocation(string ip)
        {
            var geolocation = await _client.GetIpGeolocationAsync(ip);

            Assert.IsNotNull(geolocation);

            Assert.AreEqual(geolocation.Status, "success");
        }

        [Test]
        [TestCase("127.0.0.1")]
        [TestCase("dummy text")]
        [TestCase("foo.bar")]
        public void GetIpGeolocationErrors(string ip)
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _client.GetIpGeolocationAsync(ip));
        }
    }
}