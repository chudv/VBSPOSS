namespace VBSPOSS.ViewModels
{
    public class TransferDataPosMasterViewModel
    {
        public int OrderNo { get; set; }                        //Thêm

        public string OrderNoText { get; set; }                 //Thêm

        public string EventCode { get; set; }

        public string EventName { get; set; }                   //Thêm

        public long Id { get; set; }

        public string FromPosCode { get; set; }

        public string FromPosName { get; set; }

        public string ToPosCode { get; set; }

        public string ToPosName { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public string Remark { get; set; }

        public int Status { get; set; }

        public string StatusName { get; set; }

        public int TotalVillage { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string ApproverBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public string RejectReason { get; set; }

        public bool IsDeleted { get; set; }
    }
}
