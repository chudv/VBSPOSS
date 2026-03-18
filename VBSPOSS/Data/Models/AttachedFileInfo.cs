namespace VBSPOSS.Data.Models
{
    public class AttachedFileInfo
    {
        public long FileId { get; set; }
        public long DocumentId { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string PathFile { get; set; }
        public string FileNameNew { get; set; }
        public string DocumentNumber { get; set; }
        public string ContentDescription { get; set; }
        public int Status { get; set; }
        public string CircularRefNum { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ApproverBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}
