using System.Text.Json.Serialization;

namespace UPS.Core.Utilities.Pagination
{
    public class Pager: IPager
    {
        [JsonPropertyName("total")]
        public int TotalRecords { get; set; }
        [JsonPropertyName("pages")]
        public int TotalPages { get; set; }
        [JsonPropertyName("page")]
        public int CurrentPage { get; set; }
        [JsonPropertyName("limit")]
        public int PageSize { get; set; }
    }
}
