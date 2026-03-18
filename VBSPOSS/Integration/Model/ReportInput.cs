using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VBSPOSS.Integration.Model
{
    public class ReportInput
    {
        [JsonProperty("reportId", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("reportId")]
        public string ReportId { get; set; }

        [JsonProperty("fileType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("fileType")]
        public string FileType { get; set; }

        [JsonProperty("lstParameter", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("lstParameter")]
        public List<Parameter> Parameters { get; set; }
    }
}
