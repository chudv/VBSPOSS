namespace VBSPOSS.ViewModels
{
    public class ScriptExecutionViewModel
    {
        public long Id { get; set; }

        public string ModuleCode { get; set; }

        public string ScriptName { get; set; }

        public int ExecuteType { get; set; }

        public int Status { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }


        public int PriorityLevel { get; set; }

        public string StatusDesc
        {
            get
            {
                return Status switch
                {
                    0 => "WAITING",
                    1 => "RUNNING",
                    2 => "SUCCESS",
                    3 => "FAILED",
                    4 => "CANCELLED",
                    _ => ""
                };
            }
        }

        public string PriorityLevelDesc
        {
            get
            {
                return PriorityLevel switch
                {
                    1 => "NORMAL",
                    2 => "HIGH",
                    3 => "CRITICAL",
                    _ => ""
                };
            }
        }
    }
}
