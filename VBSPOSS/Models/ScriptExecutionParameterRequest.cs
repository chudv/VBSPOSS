namespace VBSPOSS.Models
{
    public class ScriptExecutionParameterRequest
    {
        public string ParamKey { get; set; }

        public string ParamValue { get; set; }

        public string OracleDataType { get; set; }

        public string ParameterDirection { get; set; }

        public bool IsEncrypted { get; set; }
    }
}
