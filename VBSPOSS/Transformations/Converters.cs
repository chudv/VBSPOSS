using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace VBSPOSS.Transformations
{
    public class Converters
    {
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        /// <summary>
        /// Convert a byte array to an Object
        /// </summary>        
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }
        public static bool IsValidDate(string value, string[] dateFormats)
        {
            try
            {
                DateTime tempDate;
                bool validDate = DateTime.TryParseExact(value, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out tempDate);
                if (validDate)
                    return true;
                else
                    return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public static bool IsValidDate(string value, string dateFormats)
        {
            try
            {
                DateTime tempDate;
                bool validDate = DateTime.TryParseExact(value, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out tempDate);
                if (validDate)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// Hàm này lấy ra ngày giữa 2 ngày truyền vào
        /// </summary>
        /// <param name="fromdateString">Từ ngày: Định dạng ngày tháng năm kiểu yyyyMMdd</param>
        /// <param name="todateString">Đến ngày: Định dạng ngày tháng năm kiểu yyyyMMdd</param>
        /// <param name="dateFormats">yyyyMMdd</param>
        /// <returns></returns>
        public static int GetDayBetween(string fromdateString, string todateString, string dateFormats= "yyyyMMdd")
        {
            try
            {
                DateTime fromdate, todate;
                bool validDateFrom = DateTime.TryParseExact(fromdateString, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out fromdate);
                bool validDateTo = DateTime.TryParseExact(todateString, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out todate);
                if (!validDateFrom || !validDateTo)
                    return 0;
                else
                {
                    TimeSpan timeSpan = todate - fromdate;
                    return timeSpan.Days;
                }    
               
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
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
            else if (dateFormat.Equals("yyyy-MM-dd HH:mm:ss") && !string.IsNullOrEmpty(dateStr))
            {
                dateStr = dateStr.Replace("-", "").Replace(":", "").Replace(" ", "");
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


        public static string addZeroToLeading(string dateValue)
        {
            var dateParts = dateValue.Split('/');

            if (dateParts[0].Length < 2)
            {
                dateParts[0] = '0' + dateParts[0];
            }
            if (dateParts[1].Length < 2)
            {
                dateParts[1] = '0' + dateParts[1];
            }
            return dateParts[0] + "/" + dateParts[1] + "/" + dateParts[2];
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}
