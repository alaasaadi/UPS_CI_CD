using System.Text.Json.Serialization;

namespace UPS.Core.Utilities.Pagination
{
    public class PagerWrapper
    {
        [JsonPropertyName("pagination")]
        public Pager Pager { get; set; }
    }
}
