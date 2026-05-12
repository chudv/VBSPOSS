namespace VBSPOSS.Data.OSS.Models
{
    public class ScriptExecutionParameter
    {
        public long Id { get; set; }

        public long QueueId { get; set; }

        public string ParamKey { get; set; }

        public string ParamValue { get; set; }

        public string OracleDataType { get; set; }
        // VARCHAR2
        // NUMBER
        // DATE

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
