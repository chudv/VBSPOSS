namespace VBSPOSS.Data.OSS.Models
{
    public class ScriptExecutionLog
    {
        public long Id { get; set; }

        public long QueueId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int ExecutionStatus { get; set; }
        // 0 : SUCCESS
        // 1 : FAILED

        public long? OracleExecutionTimeMs { get; set; }

        public int? AffectedRows { get; set; }

        public string OracleMessage { get; set; }

        public string ExecutionLogContent { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
