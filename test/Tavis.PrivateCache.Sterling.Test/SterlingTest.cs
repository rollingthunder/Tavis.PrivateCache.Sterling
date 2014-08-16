﻿namespace Tavis.PrivateCache.Sterling.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Cache;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Tavis.PrivateCache.Sterling;
    using Wintellect.Sterling.Core;
    using Xunit;

    public class SterlingTest
    {
        private readonly ISterlingDatabaseInstance Database;

        public SterlingTest()
        {
            Database = TestHelper.CreateDatabase();  
        }

        [Fact]
        public void Sterling_can_initialize()
        {
            // Arrange

            // Act

            // Assert
            Assert.NotNull(Database);
        }

        [Fact]
        public async Task Does_not_fail_on_nonexistent_key()
        {
            // Arrange
            var key = new PrimaryCacheKey("https://localhost/test", "POST").ToString();

            // Act
            var result = await Database.LoadAsync<SterlingCacheEntry>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Can_serialize_a_CacheEntry()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:443/test");
            var response = new HttpResponseMessage(HttpStatusCode.OK){RequestMessage = request};
            var key = new PrimaryCacheKey(request.RequestUri, request.Method);
            var entry = new SterlingCacheEntry(key, Enumerable.Empty<string>());

            // Act
            var objKey = await Database.SaveAsAsync(entry);
            var restored = await Database.LoadAsync<SterlingCacheEntry>(objKey);

            // Assert
            Assert.NotNull(objKey);
            Assert.NotNull(restored);
            Assert.Equal(restored.Key, entry.Key);
            Assert.Empty(restored.VaryHeaders);
        }

        [Fact]
        public async Task Can_save_a_CacheContent()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:443/test");
            var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request, Content = new StringContent("42") };
            var key = new PrimaryCacheKey(request.RequestUri, request.Method);
            var entry = new SterlingCacheEntry(key, Enumerable.Empty<string>());

            var content = new CacheContent()
            {
                CacheControl = new CacheControlHeaderValue() { MaxAge = TimeSpan.FromDays(2) },
                CacheEntry = entry,
                Expires = DateTimeOffset.Now.AddYears(1),
                Key = "testkey",
                Response = response
            };

            // Act
            var objKey = await Database.SaveAsAsync(content);
            var restored = await Database.LoadAsync<CacheContent>(objKey);

            // Assert
            Assert.NotNull(objKey);
            Assert.NotNull(restored);
            Assert.Equal(content.Key, restored.Key);
            Assert.Equal(content.CacheControl, restored.CacheControl);
            Assert.Equal(content.Response, restored.Response);
        }
    }
}
