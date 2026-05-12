namespace VBSPOSS.Data.OSS.Models
{
    public class ScriptExecutionSchedule
    {
        public long Id { get; set; }

        public long QueueId { get; set; }

        public string CronExpression { get; set; }

        public DateTime? NextRunTime { get; set; }

        public DateTime? LastRunTime { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
