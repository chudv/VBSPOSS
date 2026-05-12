namespace VBSPOSS.Data.OSS.Models
{
    public class ScriptExecutionQueue
    {
        public long Id { get; set; }

        public string ModuleCode { get; set; }

        public long? BusinessId { get; set; }

        public string ScriptName { get; set; }

        public string DbType { get; set; }

        public int ExecuteType { get; set; }
        // 0 : MANUAL
        // 1 : AUTO

        public int ExecuteMode { get; set; }
        // 0 : SQL
        // 1 : PROCEDURE
        // 2 : PACKAGE

        public string ScriptContent { get; set; }

        public string RollbackScript { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? ScheduleTime { get; set; }

        public DateTime? ExecuteTime { get; set; }

        public int Status { get; set; }
        // 0 : WAITING
        // 1 : RUNNING
        // 2 : SUCCESS
        // 3 : FAILED
        // 4 : CANCELLED

        public int RetryCount { get; set; }

        public bool IsAutoExecuted { get; set; }

        public int PriorityLevel { get; set; }
        // 1 : NORMAL
        // 2 : HIGH
        // 3 : CRITICAL

        public string OracleSessionId { get; set; }

        public string ErrorMessage { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string ExecutedBy { get; set; }

        public DateTime? ExecutedDate { get; set; }
    }
}
