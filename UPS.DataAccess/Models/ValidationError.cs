using System.Text.Json.Serialization;

namespace UPS.DataAccess.Models
{
    public class ValidationError : Error
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }
    }
}
