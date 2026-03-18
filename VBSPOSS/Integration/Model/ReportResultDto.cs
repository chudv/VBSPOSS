using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VBSPOSS.Integration.Model
{
    public class ReportResultDto
    {
        [JsonProperty("reportId", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("reportId")]
        public string ReportId { get; set; }

        [JsonProperty("lstParameter", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("lstParameter")]
        public List<Parameter> Parameters { get; set; }

        [JsonProperty("fileName", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonProperty("fileType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("fileType")]
        public string FileType { get; set; }

        [JsonProperty("downloadUrl", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonProperty("createdDate", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("createdBy", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }
    }


    public class Parameter
    {
        [JsonProperty("paraName", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("paraName")]
        public string ParaName { get; set; }

        [JsonProperty("paraValue", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("paraValue")]
        public string ParaValue { get; set; }

        [JsonProperty("paraType", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("paraType")]
        public string ParaType { get; set; }
    }
}
