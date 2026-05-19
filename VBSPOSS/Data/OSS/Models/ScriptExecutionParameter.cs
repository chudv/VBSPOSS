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

        /// <summary>
        /// INPUT / OUTPUT / INPUT_OUTPUT
        /// </summary>
        public string ParameterDirection { get; set; }

        /// <summary>
        /// Thứ tự parameter
        /// </summary>
        public int ParameterOrder { get; set; }

        /// <summary>
        /// Có mã hóa dữ liệu hay không
        /// </summary>
        public bool IsEncrypted { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
