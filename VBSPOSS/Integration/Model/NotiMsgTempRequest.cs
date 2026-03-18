using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
using VBSPOSS.Constants;

namespace VBSPOSS.Integration.Model
{
    public class NotiMsgTempRequest
    {
        [JsonPropertyName("notiType")]
        public string? NotiType { get; set; }

        [JsonPropertyName("smsTemp")]
        public string? SmsTemp { get; set; }

        [JsonPropertyName("ottTemp")]
        public string? OttTemp { get; set; }

        [JsonPropertyName("emailTemp")]
        public string? EmailTemp { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("createdDate")]
        public string? CreatedDate { get; set; }

        [JsonPropertyName("modifiedDate")]
        public string? ModifiedDate { get; set; }

        [JsonPropertyName("mailSubject")]
        public string? MailSubject { get; set; }
    }
}