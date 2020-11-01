using System.Text.Json;

namespace UPS.DataAccess.Models
{
    public interface IResponse
    {
        ResponseCode Status { get; set; }
        JsonElement Meta { get; set; }
        JsonElement Data { get; set; }
    }
}
