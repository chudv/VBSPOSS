namespace VBSPOSS.Utils
{
    public class CustConverter
    {
        public static DateTime StringToDate(string dateStr, string dateFormat = "yyyyMMdd")
        {
            if (dateFormat.Equals("yyyyMMdd") && !string.IsNullOrEmpty(dateStr))
            {
                string yyyy = dateStr.ToString().Substring(0, 4);
                string mm = dateStr.ToString().Substring(4, 2);
                string dd = dateStr.ToString().Substring(6, 2);
                return new DateTime(int.Parse(yyyy), int.Parse(mm), int.Parse(dd));
            }
            else if (dateFormat.Equals("dd/MM/yyyy") && !string.IsNullOrEmpty(dateStr))
            {
                string dd = dateStr.ToString().Substring(0, 2);
                string mm = dateStr.ToString().Substring(3, 2);
                string yyyy = dateStr.ToString().Substring(6, 4);
                return new DateTime(int.Parse(yyyy), int.Parse(mm), int.Parse(dd));
            }
            else if (dateFormat.Equals("dd-MM-yyyy") && !string.IsNullOrEmpty(dateStr))
            {
                string dd = dateStr.ToString().Substring(0, 2);
                string mm = dateStr.ToString().Substring(3, 2);
                string yyyy = dateStr.ToString().Substring(6, 4);
                return new DateTime(int.Parse(yyyy), int.Parse(mm), int.Parse(dd));
            }
            else if (dateFormat.Equals("yyyyMMddHHmmss") && !string.IsNullOrEmpty(dateStr))
            {
                string yyyy = dateStr.ToString().Substring(0, 4);
                string mm = dateStr.ToString().Substring(4, 2);
                string dd = dateStr.ToString().Substring(6, 2);
                string HH = dateStr.ToString().Substring(8, 2);
                string minuteMM = dateStr.ToString().Substring(10, 2);
                string ss = dateStr.ToString().Substring(12, 2);
                return new DateTime(int.Parse(yyyy), int.Parse(mm), int.Parse(dd), int.Parse(HH), int.Parse(minuteMM), int.Parse(ss));
            }
            else
            {
                return DateTime.Now;
            }
        }


        public static DateTime StringToDateTime(string sDate, string sFormat)
        {
            try
            {
                DateTime dDate = DateTime.ParseExact(sDate, sFormat, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                return dDate;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static string DateTimeToString(DateTime dDate, string sFormat)
        {
            try
            {
                string sdate = dDate.ToString(sFormat, System.Globalization.CultureInfo.InvariantCulture);
                return sdate;
            }
            catch
            {
                return "";
            }
        }


    }
}
