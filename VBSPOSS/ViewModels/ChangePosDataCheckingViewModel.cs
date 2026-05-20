namespace VBSPOSS.ViewModels
{
    public class ChangePosDataCheckingViewModel
    {
        public string TYPE_CD { get; set; }

        public string POS_CD { get; set; }

        public string SCOM_CD { get; set; }

        public string CUST_ID { get; set; }

        public string CUST_NAME { get; set; }

        public string AC_NO { get; set; }

        public string PROD_CD { get; set; }

        public decimal? PRIN_AMOUNT { get; set; }

        public decimal? INT_AMOUNT { get; set; }

        public string GROUP_ID { get; set; }

        public string GROUP_NAME { get; set; }

        public string NEW_POS_CD { get; set; }

        public string NEW_SCOM_CD { get; set; }

        public string MOV_STATUS { get; set; }

        public bool ACCEPT_MOV { get; set; }
    }
}