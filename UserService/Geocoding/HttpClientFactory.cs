using System;
using System.Net.Http;

namespace UserService.Geocoding
{
    public class HttpClientFactory : IHTTPClientFactory
    {
        public HttpClient CreateClient()
        {
            var client = new HttpClient();
            SetupClientDefaults(client);
            return client;
        }

        private HttpClient SetupClientDefaults(HttpClient httpClient)
        {
            httpClient.Timeout = new TimeSpan(0, 0, 2);
            return httpClient;
        }
    }
}
