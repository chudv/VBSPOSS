namespace VBSPOSS.Data.OSS.Models
{
    public class ScriptExecutionAudit
    {
        public long Id { get; set; }

        public long QueueId { get; set; }

        public int ActionType { get; set; }
        // 0 : CREATE
        // 1 : EXECUTE
        // 2 : RETRY
        // 3 : ROLLBACK
        // 4 : CANCEL

        public string ActionContent { get; set; }

        public string ClientIp { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
