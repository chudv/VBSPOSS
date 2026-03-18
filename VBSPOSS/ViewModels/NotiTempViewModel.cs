using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VBSPOSS.ViewModels
{
    public class NotiTempViewModel
    {
        public int Id { get; set; }
        public string? NotiType { get; set; }
        public string? SmsTemp { get; set; }
        public string? OttTemp { get; set; }
        public string? EmailTemp { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? NotiSend { get; set; }
        public string? Detail { get; set; }
        [ValidateNever]
        public string? Description { get; set; }
        public string? NotiLink { get; set; }
        public string? MailSubject { get; set; }
        public string? StatusHT { get; set; }
        public string? SmsTempHT { get; set; }
        public string? OttTempHT { get; set; }
        public string? EmailTempHT { get; set; }
    }
}
