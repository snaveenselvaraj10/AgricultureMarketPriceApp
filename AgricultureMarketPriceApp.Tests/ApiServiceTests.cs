using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using AgricultureMarketPriceApp.Services;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace AgricultureMarketPriceApp.Tests
{
    public class ApiServiceTests
    {
        [Fact]
        public async Task GetDailyPricesAsync_Constructs_Url_With_Filters()
        {
            // Arrange
            var handler = new TestMessageHandler((req) =>
            {
                // Assert parts of the URL
                Assert.Contains("format=json", req.RequestUri.Query);
                Assert.Contains("filters[State]=Tamil%20Nadu", req.RequestUri.Query);
                Assert.Contains("filters[District]=Erode", req.RequestUri.Query);
                Assert.Contains("filters[Commodity]=Coconut", req.RequestUri.Query);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"records\": [] }")
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return Task.FromResult(response);
            });

            var http = new HttpClient(handler);
            var svc = new ApiService(http);

            // Act
            var results = await svc.GetDailyPricesAsync(apiKey: "test", state: "Tamil Nadu", commodity: "Coconut", district: "Erode", limit: 10);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetDailyPricesAsync_Parses_Json_To_PriceRecord()
        {
            var json = @"{ 'records': [ { 'State': 'Tamil Nadu', 'District': 'Erode', 'Commodity': 'Coconut', 'Arrival_Date': '04/04/2026', 'Modal_Price': '1234' } ] }".Replace('\'', '"');

            var handler = new TestMessageHandler((req) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return Task.FromResult(response);
            });

            var http = new HttpClient(handler);
            var svc = new ApiService(http);
            var results = await svc.GetDailyPricesAsync(apiKey: "test", limit: 10);

            Assert.NotNull(results);
            Assert.Single(results);
            var r = results[0];
            Assert.Equal("Tamil Nadu", r.State);
            Assert.Equal("Erode", r.District);
            Assert.Equal("Coconut", r.Commodity);
            Assert.Equal("04/04/2026", r.Arrival_Date);
            Assert.Equal(1234, r.Modal_price);
        }
    }

    // simple test http handler
    public class TestMessageHandler : HttpMessageHandler
    {
        private readonly System.Func<HttpRequestMessage, Task<HttpResponseMessage>> _responder;
        public TestMessageHandler(System.Func<HttpRequestMessage, Task<HttpResponseMessage>> responder)
        {
            _responder = responder;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return _responder(request);
        }
    }
}
