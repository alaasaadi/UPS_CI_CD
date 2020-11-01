using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UPS.Core.Utilities.Pagination
{
    public class PagerWrapper
    {
        [JsonPropertyName("pagination")]
        public Pager Pager { get; set; }
    }
}
