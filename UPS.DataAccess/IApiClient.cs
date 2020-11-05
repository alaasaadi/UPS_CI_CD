using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UPS.DataAccess.Models;

namespace UPS.DataAccess
{
    public interface IApiClient
    {
        Task<IResponse> RequestAsync(HttpMethod methodType, string subAddress, string jsonData = null, CancellationToken cancelToken = default);
    }
}