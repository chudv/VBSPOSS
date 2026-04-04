using System.ComponentModel.DataAnnotations.Schema;
using Kendo.Mvc.TagHelpers;

namespace VBSPOSS.Models
{
    public class UserManagementIDC
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string FunctionType { get; set; }
        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string StaffId { get; set; }
        public string StaffCode { get; set; }
        public string UserId { get; set; }
        public string NickName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string GroupName { get; set; }
        public string EntityList { get; set; }
        public string AuthType { get; set; }
        public string UserType { get; set; }
        public string MailIdFlag { get; set; }
        public string AuthsecType { get; set; }
        public string ExtraAttributeUserRole { get; set; }
        public string ExtraAttributeBranchCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Ticket { get; set; }
        public string Remark { get; set; }
        public string OrtherNotes { get; set; }
        public int Status { get; set; }
        public int StatusUpdateCore { get; set; }
        public bool? SessionValReq { get; set; }
        public int PrevStatus { get; set; }
        public string ResponseAttributes { get; set; }
        public string CallApiStatus { get; set; }
        public int CallApiReqRecordSl { get; set; }
        public string CallApiResponseCode { get; set; }
        public string CallApiResponseMsg { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ApproverBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string? IpSetCode { get; set; }
        public string? IpSetDetail { get; set; }
        public int? RestrictionFlag { get; set; }

    }


    public class UserIDCMaster
    {        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string PosCode { get; set; }
        public string PosName { get; set; }
        public string StaffId { get; set; }
        public string StaffCode { get; set; }
        public string UserId { get; set; }
        public string NickName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string GroupName { get; set; }
        public string EntityList { get; set; }
        public string AuthType { get; set; }
        public string UserType { get; set; }
        public string MailIdFlag { get; set; }
        public string AuthsecType { get; set; }
        public string ExtraAttributeUserRole { get; set; }
        public string ExtraAttributeBranchCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Remark { get; set; }
        public string OrtherNotes { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string? IpSetCode { get; set; }
        public string? IpSetDetail { get; set; }
        public int? RestrictionFlag { get; set; }
    }

    public class UserIDCApproval
    {
        public string STT { get; set; }
        public int TT_Dong { get; set; }
        public int KieuIn { get; set; }
        public string MaDonVi { get; set; }
        public int SoLgTrinh_ThemMoi { get; set; }
        public int SoLgTrinh_Matkhau { get; set; }
        public int SoLgTrinh_MoKhoa { get; set; }
        public int SoLgTrinh_Khoa { get; set; }
        public int SoLgTrinh_Edit { get; set; }
        public int SoLgTrinh_ChangePos { get; set; }
        public int SoLgTrinh_ChangeRole { get; set; }
        public string TenChiNhanh { get; set; }            //Thêm
        public string KhoangThoiGian { get; set; }
    }

    public class UserIDCRestrictionAllowedDays
    {
        public long UserMasterId { get; set; }
    
        public string UserId { get; set; }
    
        public int LoginRestrict { get; set; }
    
        public int SameTimeForAllDays { get; set; }
    
        public int RestrictionType { get; set; }
    
        public int AllowedDays { get; set; }
    
        public string StartRestriction { get; set; }
    
        public string EndRestriction { get; set; }
    
        public string Remark { get; set; }
    
        public string OrtherNotes { get; set; }
    
        public int Status { get; set; }
    
        public int? StatusUpdateCore { get; set; }
    
        public bool? SessionValReq { get; set; }
    
        public int? PrevStatus { get; set; }
    
        public string ResponseAttributes { get; set; }
    
        public string CallApiStatus { get; set; }
    
        public int CallApiReqRecordSl { get; set; }
    
        public string CallApiResponseCode { get; set; }
    
        public string CallApiResponseMsg { get; set; }
    
        public string CreatedBy { get; set; }
    
        public DateTime CreatedDate { get; set; }
    
        public string ModifiedBy { get; set; }
    
        public DateTime ModifiedDate { get; set; }
    
        public string ApproverBy { get; set; }
    
        public DateTime ApprovalDate { get; set; }
    }
}
