namespace VBSPOSS.ViewModels
{
    public class TransferDataPosDetailViewModel
    {
        public long Id { get; set; }

        public long MasterId { get; set; }

        public string FromCommuneId { get; set; }

        public string FromCommuneName { get; set; }

        public string FromVillageId { get; set; }

        public string FromVillageName { get; set; }

        public string ToCommuneId { get; set; }

        public string ToCommuneName { get; set; }

        public string ToVillageId { get; set; }

        public string ToVillageName { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
