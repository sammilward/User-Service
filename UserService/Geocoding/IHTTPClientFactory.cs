using System.Net.Http;

namespace UserService.Geocoding
{
    public interface IHTTPClientFactory
    {
        HttpClient CreateClient();
    }
}
