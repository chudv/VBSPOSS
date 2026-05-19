namespace VBSPOSS.Models
{
    public class ScriptExecutionRequest
    {
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

        public bool IsAutoExecuted { get; set; }

        public int PriorityLevel { get; set; }

        public string CreatedBy { get; set; }

        public List<ScriptExecutionParameterRequest> Parameters { get; set; }
    }
}
