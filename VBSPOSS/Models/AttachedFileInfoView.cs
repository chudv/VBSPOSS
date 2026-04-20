namespace VBSPOSS.Models
{
    public class AttachedFileInfoView
    {


        public long FileId { get; set; }
        public long DocumentId { get; set; }

        public string FileType { get; set; }
        public string FileTypeName { get; set; }   // hiển thị text loại file

        public string FileName { get; set; }
        public string FileExtension { get; set; }

        public string DocumentNumber { get; set; }
        public string CircularRefNum { get; set; }
        public string ContentDescription { get; set; }

        public int Status { get; set; }
        public string StatusName { get; set; }     // hiển thị trạng thái

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string ApproverBy { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // UI only
        public string DownloadUrl { get; set; }

        public int Orderby { get; set; }

        public string PosCode { get; set; }
        public string PosName { get; set; }

        public decimal SizeKB { get; set; }

        public string TransactionDate { get; set; }   // dd/MM/yyyy
        public string ExportDate { get; set; }        // dd/MM/yyyy
        public string ExportTime { get; set; }        // HH:mm:ss

        public int DownloadCount { get; set; }

        public string TxnPointCode { get; set; }

        public string TxnPointName { get; set; }

        public string ProvinceCode { get; set; }

        public DateTime EffectiveDate { get; set; }

    }
}
