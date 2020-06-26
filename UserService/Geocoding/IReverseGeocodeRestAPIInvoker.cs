using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace UserService.Geocoding
{
    public interface IReverseGeocodeRestAPIInvoker
    {
        Task<JObject> RequestAndWaitForResponseAsync(double lat, double lng);
    }
}
