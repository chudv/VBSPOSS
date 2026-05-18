namespace VBSPOSS.Models
{
    public class ScriptValidationResult
    {
        public bool IsValid { get; set; }

        public bool HasWarning { get; set; }

        public string Message { get; set; }

        public string RiskLevel { get; set; }
    }
}
