namespace VBSPOSS.ViewModels
{
    public class DepositTermModel
    {
        public int Id { get; set; }
        public string TermCode { get; set; }
        public string TermDesc { get; set; }

        public string TermUnitCode { get; set; }

        public string TermUnitName { get; set; }

        public int TermValue { get; set; }

        public int MinTermValue { get; set; }

        public string InclusionFlag { get; set; }
    }
}
