namespace VBSPOSS.Data.Models
{
    public class InterestRatePosApply
    {
        public long Id { get; set; }
        public long IntRateConfigId { get; set; }
        public string PosCode { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int Status { get; set; } 
    }
}
