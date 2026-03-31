namespace VBSPOSS.Constants
{
    public class NotiType
    {
        /// <summary>
        /// '1' - SMS
        /// </summary>
        public const string SMS = "1";

        /// <summary>
        /// '2' - OTT
        /// </summary>
        public const string OTT = "2";

        /// <summary>
        /// '3' - Email
        /// </summary>
        public const string EMAIL = "3";
    }

    public static class NotiDataType
    {
        /// <summary>
        /// Trả nợ khoản vay
        /// </summary>
        public const string LOAN_PAYMENT = "LOAN_PAYMENT";

        /// <summary>
        /// Thông báo nợ đến hạn
        /// </summary>
        public const string LOAN_DUE_DEBT = "LOAN_DUE_DEBT";

        /// <summary>
        /// Thông báo nợ đến hạn
        /// </summary>
        public const string USER_OFFLINE = "USER_OFFLINE";
        
    }

    public static class NotiStatus
    {
        /// <summary>
        /// 0 - Tất cả
        /// </summary>
        public const string ALL = "0";

        /// <summary>
        /// 1 - Tạo lập
        /// </summary>
        public const string CREATED = "1";

        /// <summary>
        /// 2 - Gửi thành công
        /// </summary>
        public const string SUCCESS = "2";

        /// <summary>
        /// 3 - Gửi không thành công
        /// </summary>
        public const string ERROR = "3";
    }

    public static class NotiApi
    {
        /// <summary>
        /// 0 - Tất cả
        /// </summary>
        public const string ALL = "0";
    }

}