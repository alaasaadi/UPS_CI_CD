using System.Net.Http;
using System.Threading.Tasks;
using UPS.DataAccess.Models;

namespace UPS.DataAccess
{
    public interface IApiClient
    {
        Task<IResponse> Request(HttpMethod methodType, string subAddress, string jsonData = null);
    }
}