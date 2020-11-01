using System.Text.Json;
using System.Text.Json.Serialization;

namespace UPS.DataAccess.Models
{
    public class Response : IResponse
    {
        [JsonPropertyName("code")]
        public ResponseCode Status { get; set; }

        [JsonPropertyName("meta")]
        public JsonElement Meta { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}
