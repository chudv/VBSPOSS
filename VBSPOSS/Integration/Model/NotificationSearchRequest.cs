using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
using VBSPOSS.Constants;

namespace VBSPOSS.Integration.Model
{
    public class NotificationSearchRequest
    {
        public string NotiType { get; set; } = string.Empty;

        public Dictionary<string, string> Conditions { get; set; } = new();
    }
}