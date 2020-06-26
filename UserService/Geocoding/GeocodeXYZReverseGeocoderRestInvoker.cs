using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;

namespace UserService.Geocoding
{
    public class GeocodeXYZReverseGeocoderRestInvoker : IReverseGeocodeRestAPIInvoker
    {
        private ILogger<GeocodeXYZReverseGeocoderRestInvoker> _logger;
        private readonly IHTTPClientFactory _hTTPClientFactory;
        private readonly NameValueCollection Query;

        private const string BaseURL = "https://geocode.xyz";

        public GeocodeXYZReverseGeocoderRestInvoker(IHTTPClientFactory HTTPClientFactory, ILogger<GeocodeXYZReverseGeocoderRestInvoker> logger)
        {
            _hTTPClientFactory = HTTPClientFactory;
            Query = HttpUtility.ParseQueryString(string.Empty);
            _logger = logger;
        }

        public async Task<JObject> RequestAndWaitForResponseAsync(double lat, double lng)
        {
            Query["json"] = "1";

            var queryString = Query.ToString();
            var latLng = $"{lat},{lng}";

            try
            {
                _logger.LogInformation($"{nameof(GeocodeXYZReverseGeocoderRestInvoker)}.{nameof(RequestAndWaitForResponseAsync)}: Making request to GeocodeXYZ");

                var response = await _hTTPClientFactory.CreateClient().GetAsync($"{BaseURL}/{latLng}?{queryString}");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{nameof(GeocodeXYZReverseGeocoderRestInvoker)}.{nameof(RequestAndWaitForResponseAsync)}: Successful response from GeocodeXYZ");

                    var content = await response.Content.ReadAsStringAsync();
                    var jResponse = JObject.Parse(content);
                    return jResponse;
                }
                else _logger.LogInformation($"{nameof(GeocodeXYZReverseGeocoderRestInvoker)}.{nameof(RequestAndWaitForResponseAsync)}: Failed response from GeocodeXYZ");
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}
