namespace VBSPOSS.Integration.Model
{
    public class NotificationDataResponse
    {
        public long? id { get; set; }
        public string? notiType { get; set; }
        public string? sourceId { get; set; }
        public DateTime? businessDate { get; set; }
        public string? posCode { get; set; }
        public string? posName { get; set; }
        public string? customerId { get; set; }
        public string? customerName { get; set; }
        public string? mobileNo { get; set; }
        public string? email { get; set; }

        public string? d1 { get; set; }
        public string? d2 { get; set; }
        public string? d3 { get; set; }
        public string? d4 { get; set; }
        public string? d5 { get; set; }
        public string? d6 { get; set; }
        public string? d7 { get; set; }
        public string? d8 { get; set; }
        public string? d9 { get; set; }
        public string? d10 { get; set; }
        public string? d11 { get; set; }
        public string? d12 { get; set; }
        public string? d13 { get; set; }
        public string? d14 { get; set; }
        public string? d15 { get; set; }
        public string? d16 { get; set; }
        public string? d17 { get; set; }
        public string? d18 { get; set; }
        public string? d19 { get; set; }
        public string? d20 { get; set; }

        public string? status { get; set; }
        public string? errorCode { get; set; }
        public string? errorMessage { get; set; }

        public DateTime? createdTime { get; set; }
        public string? createdBy { get; set; }
        public DateTime? updatedTime { get; set; }
        public string? updatedBy { get; set; }
        public DateTime? sendTime { get; set; }
        public string? sendBy { get; set; }
        public string? messageId { get; set; }
        public string? sendType { get; set; }

        public string? smsContent { get; set; }
        public string? ottContent { get; set; }
        public string? emailContent { get; set; }
        public string? messageContent { get; set; }
    }
}
