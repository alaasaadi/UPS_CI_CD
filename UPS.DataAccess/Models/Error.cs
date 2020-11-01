using System.Text.Json.Serialization;

namespace UPS.DataAccess.Models
{
    public class Error: IError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
