using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VBSPOSS.Data.Models
{
    public class NotiTemp
    {
        public int Id { get; set; }
        public string? NotiType { get; set; }
        [ValidateNever]
        public string? SmsTemp { get; set; }
        [ValidateNever]
        public string? OttTemp { get; set; }
        [ValidateNever]
        public string? EmailTemp { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? MailSubject { get; set; }
    }
}
