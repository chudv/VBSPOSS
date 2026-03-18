namespace VBSPOSS.ViewModels
{
    public class UserManagementIDCViewModel
    {
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
        public string FullName { get; set; }            //Thêm
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
        public string TiOrtherNotescket { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }            //Thêm
        public int StatusUpdateCore { get; set; }
        public bool SessionValReq { get; set; }
        public string PrevStatus { get; set; }
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


    public class UserIDCMasterViewModel
    {
        public long OrderNo { get; set; }           //Thêm
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
        public string StatusText { get; set; }            //Thêm
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime ApprovalDate { get; set; }
    }
}
