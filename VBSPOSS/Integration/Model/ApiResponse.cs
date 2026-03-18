namespace VBSPOSS.Models // 


{ 
    public class ApiResponse
{
    public string txnStatus { get; set; }
    public List<Record> record { get; set; }
    public string responseCode { get; set; }
    public string responseMsg { get; set; }
}

public class Record
{
    public string interestRate { get; set; }
    public string prodCode { get; set; }
    public string debitCreditFlag { get; set; }
    public string accountType { get; set; }
    public string penalRate { get; set; }
    public string posRateExpiryDate { get; set; }
    public string posCode { get; set; }
    public string subType { get; set; }
    public string currency { get; set; }
    public string circularRef { get; set; }
    public string effectiveDate { get; set; }
    public string circularDate { get; set; }
}
}