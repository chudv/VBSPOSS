using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VBSPOSS.Helpers
{
    public enum Quarter
    {
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4
    }

    public enum Month
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
    public class DateTimeUtils
    {
        public static string[] VN_DAY_NAMES = { "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy", "Chủ nhật" };

        public static string[] VN_DAY_NAMES_VT = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };

        public DateTimeUtils()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region -----> Days - DateTime <-----
        public static DateTime MinSqlDateTime()
        {
            return new DateTime(1900, 1, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Hàm thực hiện trả về tên Thứ trong tuần từ ngày truyền vào hàm. Ví dụ ngày 3/2/2015 --> Trả về chuỗi "Thứ ba"
        /// </summary>
        /// <param name="dDateTime">Ngày cần lấy tên thứ truyền vào</param>
        /// <param name="bFlagName">Trạng thái xác định kiểu Tên thứ trả về. Với quy ước: 0 - Tên đầy đủ (Ví dụ: Thứ hai); 1 - Tên viết tắt (Ví dụ: T2)</param>
        /// <returns>Chuỗi giá trị Tên thứ</returns>
        public static string GetDayVnName(DateTime dDateTime, byte bFlagName)
        {
            switch (dDateTime.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return bFlagName == 0 ? VN_DAY_NAMES[0] : VN_DAY_NAMES_VT[0];
                case DayOfWeek.Tuesday:
                    return bFlagName == 0 ? VN_DAY_NAMES[1] : VN_DAY_NAMES_VT[1];
                case DayOfWeek.Wednesday:
                    return bFlagName == 0 ? VN_DAY_NAMES[2] : VN_DAY_NAMES_VT[2];
                case DayOfWeek.Thursday:
                    return bFlagName == 0 ? VN_DAY_NAMES[3] : VN_DAY_NAMES_VT[3];
                case DayOfWeek.Friday:
                    return bFlagName == 0 ? VN_DAY_NAMES[4] : VN_DAY_NAMES_VT[4];
                case DayOfWeek.Saturday:
                    return bFlagName == 0 ? VN_DAY_NAMES[5] : VN_DAY_NAMES_VT[5];
                case DayOfWeek.Sunday:
                    return bFlagName == 0 ? VN_DAY_NAMES[6] : VN_DAY_NAMES_VT[6];
                default:
                    return "";
            }
        }

        public static DateTime GetBeginOfToDay()
        {
            return GetBeginOfDate(DateTime.Now);
        }

        public static DateTime GetEndOfToDay()
        {
            return GetEndOfDate(DateTime.Now);
        }

        /// <summary>
        /// Get the begin of this date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetBeginOfDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        /// <summary>
        /// Get the end of this date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetEndOfDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 99);
        }

        public static DateTime GetStartOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfDay(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static bool EqualsDate(DateTime baseDate, DateTime compareDate)
        {
            return ((baseDate.Day == compareDate.Day) && (baseDate.Month == compareDate.Month) && (baseDate.Year == compareDate.Year));
        }

        public static int GetDaysBetweenDates(DateTime firstDate, DateTime secondDate)
        {
            return secondDate.Subtract(firstDate).Days;
        }
        #endregion

        #region -----> Week - Hàm liên quan đến Tuần <-----
        /// <summary>
        /// Hàm trả về ngày đầu tiên của tuần trước tuần hiện thời
        /// </summary>
        /// <returns>Ngày đầu tiên của tuần trước tuần hiện thời</returns>
        public static DateTime GetStartOfLastWeek()
        {
            int DaysToSubtract = (int)DateTime.Now.DayOfWeek + 7;                   //DayOfWeek: Là ngày thứ mấy trong tuần hiện thời. Ví dụ 3/2/2015 là ngày thứ 2 trong tuần
            DateTime dt_tmp = DateTime.Now.Subtract(System.TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt_tmp.Year, dt_tmp.Month, dt_tmp.Day, 0, 0, 0, 0);
        }

        /// <summary>
        /// Hàm trả về ngày Cuối cùng của tuần trước tuần hiện thời
        /// </summary>
        /// <returns>Ngày Cuối cùng của tuần trước tuần hiện thời</returns>
        public static DateTime GetEndOfLastWeek()
        {
            DateTime dt = GetStartOfLastWeek().AddDays(6);
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, 999);
        }

        /// <summary>
        /// Hàm trả về ngày đầu tiên của tuần hiện thời - Tính là ngày Chủ nhật (Chủ nhật là ngày đầu tiên của tuần)
        /// </summary>
        /// <returns>Ngày đầu tiên của tuần hiện thời</returns>
        public static DateTime GetStartOfCurrentWeek()
        {
            int DaysToSubtract = (int)DateTime.Now.DayOfWeek;
            DateTime dt_tmp = DateTime.Now.Subtract(System.TimeSpan.FromDays(DaysToSubtract));
            return new DateTime(dt_tmp.Year, dt_tmp.Month, dt_tmp.Day, 0, 0, 0, 0);
        }

        /// <summary>
        /// Hàm trả về ngày cuối cùng của tuần hiện thời - Tính là ngày Thứ 7 (Thứ 7 là ngày cuối cùng của tuần)
        /// </summary>
        /// <returns>Ngày cuối cùng của tuần hiện thời</returns>
        public static DateTime GetEndOfCurrentWeek()
        {
            DateTime dt_tmp = GetStartOfCurrentWeek().AddDays(6);
            return new DateTime(dt_tmp.Year, dt_tmp.Month, dt_tmp.Day, 23, 59, 59, 999);
        }

        /// <summary>
        /// Hàm trả về ngày đầu tiên của Tuần - Tính ngày đầu tiên là ngày Thứ 2
        /// </summary>
        /// <param name="date">Ngày truyền vào để lấy ngày đầu tiên của tuần</param>
        /// <returns>Ngày đầu tiên của tuần</returns>
        public static DateTime GetBeginWeekOfDate(DateTime date)
        {
            DateTime beginOfWeek = date;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    beginOfWeek = GetBeginOfDate(date);
                    break;
                case DayOfWeek.Tuesday:
                    beginOfWeek = GetBeginOfDate(date.AddDays(-1));
                    break;
                case DayOfWeek.Wednesday:
                    beginOfWeek = GetBeginOfDate(date.AddDays(-2));
                    break;
                case DayOfWeek.Thursday:
                    beginOfWeek = GetBeginOfDate(date.AddDays(-3));
                    break;
                case DayOfWeek.Friday:
                    beginOfWeek = GetBeginOfDate(date.AddDays(-4));
                    break;
                case DayOfWeek.Saturday:
                    beginOfWeek = GetBeginOfDate(date.AddDays(-5));
                    break;
                case DayOfWeek.Sunday:
                    beginOfWeek = GetBeginOfDate(date.AddDays(-6));
                    break;
            }
            return beginOfWeek;
        }

        /// <summary>
        /// Hàm trả về ngày cuối cùng của Tuần - Tính ngày kết thúc là ngày Chủ nhật
        /// </summary>
        /// <param name="date">Ngày truyền vào để lấy ngày cuối cùng của tuần</param>
        /// <returns>Ngày cuối cùng của tuần - Chủ nhật</returns>
        public static DateTime GetEndWeekOfDate(DateTime date)
        {
            DateTime endOfWeek = date;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    endOfWeek = GetEndOfDate(date.AddDays(6));
                    break;
                case DayOfWeek.Tuesday:
                    endOfWeek = GetEndOfDate(date.AddDays(5));
                    break;
                case DayOfWeek.Wednesday:
                    endOfWeek = GetEndOfDate(date.AddDays(4));
                    break;
                case DayOfWeek.Thursday:
                    endOfWeek = GetEndOfDate(date.AddDays(3));
                    break;
                case DayOfWeek.Friday:
                    endOfWeek = GetEndOfDate(date.AddDays(2));
                    break;
                case DayOfWeek.Saturday:
                    endOfWeek = GetEndOfDate(date.AddDays(1));
                    break;
                case DayOfWeek.Sunday:
                    endOfWeek = GetEndOfDate(date);
                    break;
            }
            return endOfWeek;
        }

        /// <summary>
        /// Hàm trả về Ngày đầu tiên của Tuần - Từ ngày hiện thời
        /// </summary>
        /// <returns>Ngày đầu tiên của tuần tính theo NOW</returns>
        public static DateTime GetBeginOfThisWeek()
        {
            return GetBeginWeekOfDate(DateTime.Now);
        }

        /// <summary>
        /// Hàm trả về Ngày cuối cùng của Tuần - Từ ngày hiện thời
        /// </summary>
        /// <returns>Ngày cuối cùng của tuần tính theo NOW</returns>
        public static DateTime GetEndOfThisWeek()
        {
            return GetEndWeekOfDate(DateTime.Now);
        }

        /// <summary>
        /// Hàm thực hiện trả về Tuần thứ mấy trong năm - Trả về kiểu nguyên số thứ tự của tuần qua ngày truyền vào là thứ mấy
        /// </summary>
        /// <param name="date">Ngày cần lấy thứ tự tuần trong năm</param>
        /// <returns>Kiểu số - Thứ tự Tuần trong năm</returns>
        public static int GetWeekNumber(DateTime date)
        {
            const int JAN = 1;
            const int DEC = 12;
            const int LASTDAYOFDEC = 31;
            const int FIRSTDAYOFJAN = 1;
            const int THURSDAY = 4;
            bool ThursdayFlag = false;

            // Get the day number since the beginning of the year
            int DayOfYear = date.DayOfYear;

            // Get the numeric weekday of the first day of the 
            // year (using sunday as FirstDay)
            int StartWeekDayOfYear =
                (int)(new DateTime(date.Year, JAN, FIRSTDAYOFJAN)).DayOfWeek;
            int EndWeekDayOfYear =
                (int)(new DateTime(date.Year, DEC, LASTDAYOFDEC)).DayOfWeek;

            // Compensate for the fact that we are using monday
            // as the first day of the week
            if (StartWeekDayOfYear == 0)
                StartWeekDayOfYear = 7;
            if (EndWeekDayOfYear == 0)
                EndWeekDayOfYear = 7;

            // Calculate the number of days in the first and last week
            int DaysInFirstWeek = 8 - (StartWeekDayOfYear);
            int DaysInLastWeek = 8 - (EndWeekDayOfYear);

            // If the year either starts or ends on a thursday it will have a 53rd week
            if (StartWeekDayOfYear == THURSDAY || EndWeekDayOfYear == THURSDAY)
                ThursdayFlag = true;

            // We begin by calculating the number of FULL weeks between the start of the year and
            // our date. The number is rounded up, so the smallest possible value is 0.
            int FullWeeks = (int)Math.Ceiling((DayOfYear - (DaysInFirstWeek)) / 7.0);

            int WeekNumber = FullWeeks;

            // If the first week of the year has at least four days, then the actual week number for our date
            // can be incremented by one.
            if (DaysInFirstWeek >= THURSDAY)
                WeekNumber = WeekNumber + 1;

            // If week number is larger than week 52 (and the year doesn't either start or end on a thursday)
            // then the correct week number is 1.
            if (WeekNumber > 52 && !ThursdayFlag)
                WeekNumber = 1;

            // If week number is still 0, it means that we are trying to evaluate the week number for a
            // week that belongs in the previous year (since that week has 3 days or less in our date's year).
            // We therefore make a recursive call using the last day of the previous year.
            if (WeekNumber == 0)
                WeekNumber = GetWeekNumber(
                    new DateTime(date.Year - 1, DEC, LASTDAYOFDEC));
            return WeekNumber;
        }

        /// <summary>
        /// Hàm thực hiện trả về Tuần thứ mấy trong năm - Trả về kiểu nguyên số thứ tự của tuần qua ngày truyền vào là thứ mấy
        /// </summary>
        /// <param name="date">Ngày cần lấy thứ tự tuần trong năm</param>
        /// <returns>Kiểu số - Thứ tự Tuần trong năm</returns>
        public static int GetWeekNo(DateTime date)
        {
            DateTime currentMonday = GetBeginWeekOfDate(date);
            DateTime firstMonday = GetBeginWeekOfDate(new DateTime(date.Year, 1, 1, 0, 0, 0));
            int days = 0;
            if (firstMonday.Year < currentMonday.Year) days = currentMonday.DayOfYear + ((new DateTime(firstMonday.Year, 12, 31, 0, 0, 0)).DayOfYear - firstMonday.DayOfYear);
            else days = currentMonday.DayOfYear - firstMonday.DayOfYear;
            if (days > 0) return days / 7 + 1;
            else return 1;
        }

        /// <summary>
        /// Hàm trả về ngày đầu tiên của Tuần từ Năm và Số thứ tự tuần trong năm truyền vào
        /// </summary>
        /// <param name="year">Năm truyền vào</param>
        /// <param name="week">Thứ tự tuần trong năm</param>
        /// <returns>Ngày đầu tiên của Tuần</returns>
        public static DateTime GetBeginDateOfWeek(int year, int week)
        {
            DateTime beginYear = new DateTime(year, 1, 1, 0, 0, 0);
            DateTime firstMonday = GetBeginWeekOfDate(beginYear);
            if (week > 0)
            {
                firstMonday = firstMonday.AddDays((week - 1) * 7);
            }
            return firstMonday;
        }
        #endregion

        #region -----> Quarters - Hàm liên quan đến Quý <-----
        /// <summary>
        /// Hàm trả về Ngày đầu tiên của Quý và Năm truyền vào
        /// </summary>
        /// <param name="pYear">Năm truyền vào - Kiểu nguyên</param>
        /// <param name="pQuarter">Quý truyền vào - Kiểu nguyên</param>
        /// <returns>Ngày đầu của Quý và Năm Truyền vào</returns>
        public static DateTime GetStartOfQuarter(int pYear, int pQuarter)
        {
            int iMonth = pQuarter * 3 - 2;
            return new DateTime(pYear, iMonth, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Hàm trả về Ngày đầu tiên của Quý và Năm truyền vào
        /// </summary>
        /// <param name="pYear">Năm truyền vào - Kiểu nguyên</param>
        /// <param name="pQuarter">Quý truyền vào - Kiểu string. Ví dụ: I, II, III, IV</param>
        /// <returns>Ngày đầu của Quý và Năm Truyền vào</returns>
        public static DateTime GetStartOfQuarter(int pYear, string pQuarter)
        {
            int iQuarter = pQuarter.Trim().ToUpper() == "I" ? 1 : pQuarter.Trim().ToUpper() == "II" ? 2 : pQuarter.Trim().ToUpper() == "III" ? 3 : 4;
            int iMonth = iQuarter * 3 - 2;
            return new DateTime(pYear, iMonth, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Hàm trả về Ngày đầu tiên của Quý và Năm truyền vào
        /// </summary>
        /// <param name="Year">Năm truyền vào</param>
        /// <param name="pQuarter">Quý truyền vào</param>
        /// <returns>Ngày đầu tiên của Quý và năm truyền vào</returns>
        public static DateTime GetStartOfQuarter(int Year, Quarter pQuarter)
        {
            if (pQuarter == Quarter.First)                      // 1st Quarter = January 1 to March 31
                return new DateTime(Year, 1, 1, 0, 0, 0, 0);
            else if (pQuarter == Quarter.Second)                // 2nd Quarter = April 1 to June 30
                return new DateTime(Year, 4, 1, 0, 0, 0, 0);
            else if (pQuarter == Quarter.Third)                 // 3rd Quarter = July 1 to September 30
                return new DateTime(Year, 7, 1, 0, 0, 0, 0);
            else                                                // 4th Quarter = October 1 to December 31
                return new DateTime(Year, 10, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Hàm trả về Ngày cuối cùng của Năm và Quý truyền vào
        /// </summary>
        /// <param name="Year">Năm truyền vào - Kiểu nguyên</param>
        /// <param name="pQuarter">Quý truyền vào - Kiểu Nguyên</param>
        /// <returns>Ngày cuối cùng của Quý</returns>
        public static DateTime GetEndOfQuarter(int Year, int pQuarter)
        {
            int iMonth = pQuarter * 3;
            return new DateTime(Year, iMonth, DateTime.DaysInMonth(Year, iMonth), 23, 59, 59, 999);
        }

        /// <summary>
        /// Hàm trả về Ngày cuối cùng của Năm và Quý truyền vào
        /// </summary>
        /// <param name="Year">Năm truyền vào - Kiểu nguyên</param>
        /// <param name="pQuarter">Quý truyền vào - Kiểu string. Ví dụ: I, I, III, IV</param>
        /// <returns>Ngày cuối cùng của Quý</returns>
        public static DateTime GetEndOfQuarter(int Year, string pQuarter)
        {
            int iQuarter = pQuarter.Trim().ToUpper() == "I" ? 1 : pQuarter.Trim().ToUpper() == "II" ? 2 : pQuarter.Trim().ToUpper() == "III" ? 3 : 4;
            int iMonth = iQuarter * 3;
            return new DateTime(Year, iMonth, DateTime.DaysInMonth(Year, iMonth), 23, 59, 59, 999);
        }

        /// <summary>
        /// Hàm trả về Ngày cuối cùng của Năm và Quý truyền vào
        /// </summary>
        /// <param name="Year">Năm truyền vào - Kiểu nguyên</param>
        /// <param name="pQuarter">Quý truyền vào - Kiểu đối tượng</param>
        /// <returns>Ngày cuối cùng của Quý</returns>
        public static DateTime GetEndOfQuarter(int Year, Quarter pQuarter)
        {
            if (pQuarter == Quarter.First)    // 1st Quarter = January 1 to March 31
                return new DateTime(Year, 3, DateTime.DaysInMonth(Year, 3), 23, 59, 59, 999);
            else if (pQuarter == Quarter.Second) // 2nd Quarter = April 1 to June 30
                return new DateTime(Year, 6, DateTime.DaysInMonth(Year, 6), 23, 59, 59, 999);
            else if (pQuarter == Quarter.Third) // 3rd Quarter = July 1 to September 30
                return new DateTime(Year, 9, DateTime.DaysInMonth(Year, 9), 23, 59, 59, 999);
            else // 4th Quarter = October 1 to December 31
                return new DateTime(Year, 12, DateTime.DaysInMonth(Year, 12), 23, 59, 59, 999);
        }

        /// <summary>
        /// Hàm trả về Quý hiện theo tháng truyền vào
        /// </summary>
        /// <param name="Month">Tháng truyền vào</param>
        /// <returns>Quý hiện trả về</returns>
        public static Quarter GetQuarter(Month Month)
        {
            if (Month <= Month.March)
                // 1st Quarter = January 1 to March 31
                return Quarter.First;
            else if ((Month >= Month.April) && (Month <= Month.June))
                // 2nd Quarter = April 1 to June 30
                return Quarter.Second;
            else if ((Month >= Month.July) && (Month <= Month.September))
                // 3rd Quarter = July 1 to September 30
                return Quarter.Third;
            else // 4th Quarter = October 1 to December 31
                return Quarter.Fourth;
        }
        /// <summary>
        /// Hàm thực hiện trả về Quý hiện thời từ tháng truyền vào
        /// </summary>
        /// <param name="pMonth">Tháng truyền vào</param>
        /// <returns></returns>
        public static int GetQuarter(int pMonth)
        {
            int Quarter = 0;
            if (pMonth % 3 == 0)
                Quarter = pMonth / 3;
            else
                Quarter = pMonth / 3 + 1;
            return Quarter;
        }

        /// <summary>
        /// Hàm thực hiện trả về Ngày Cuối cùng của Quý trước quý hiện thời (NOW)
        /// Ex: Ví dụ hôm ngay ngày 03/02/2015 thì hàm sẽ trả về ngày 31/12/2014
        /// </summary>
        /// <returns>Ngày cuối cùng của Quý trước quý hiện thời (NOW)</returns>
        public static DateTime GetEndOfLastQuarter()
        {
            if ((Month)DateTime.Now.Month <= Month.March)
                //go to last quarter of previous year
                return GetEndOfQuarter(DateTime.Now.Year - 1, Quarter.Fourth);
            else //return last quarter of current year
                return GetEndOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month));
        }

        /// <summary>
        /// Hàm thực hiện trả về Ngày đầu tiên của Quý trước quý hiện thời (NOW)
        /// Ex: Ví dụ hôm ngay ngày 03/02/2015 thì hàm sẽ trả về ngày 01/10/2014
        /// </summary>
        /// <returns>Ngày đầu tiên của Quý trước quý hiện thời (NOW)</returns>
        public static DateTime GetStartOfLastQuarter()
        {
            if ((Month)DateTime.Now.Month <= Month.March)
                //go to last quarter of previous year
                return GetStartOfQuarter(DateTime.Now.Year - 1, Quarter.Fourth);
            else //return last quarter of current year
                return GetStartOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month));
        }

        /// <summary>
        /// Hàm trả về ngày đầu tiên của Quý hiện thời (Hiện thời theo ngày giờ hệ thống)
        /// </summary>
        /// <returns>Ngày Đầu tiên của Quý hiện thời (NOW)</returns>
        public static DateTime GetStartOfCurrentQuarter()
        {
            return GetStartOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month));
        }

        /// <summary>
        /// Hàm trả về ngày Cuối cùng của Quý hiện thời (Hiện thời theo ngày giờ hệ thống)
        /// </summary>
        /// <returns>Ngày Cuối cùng của Quý hiện thời (NOW)</returns>
        public static DateTime GetEndOfCurrentQuarter()
        {
            return GetEndOfQuarter(DateTime.Now.Year, GetQuarter((Month)DateTime.Now.Month));
        }
        #endregion

        #region -----> Months <-----
        /// <summary>
        /// Hàm trả về ngày đầu tiên của tháng - Từ năm và tháng truyền vào
        /// </summary>
        /// <param name="Month">Tháng truyền vào - Kiểu nguyên</param>
        /// <param name="Year">Năm truyền vào - Kiểu nguyên</param>
        /// <returns>Ngày đầu tiên của tháng</returns>
        public static DateTime GetStartOfMonth(int Month, int Year)
        {
            return new DateTime(Year, Month, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Hàm trả về ngày cuối cùng của tháng - Từ năm và tháng truyền vào
        /// </summary>
        /// <param name="Month">Tháng truyền vào - Kiểu nguyên</param>
        /// <param name="Year">Năm truyền vào - Kiểu nguyên</param>
        /// <returns>Ngày cuối cùng của tháng</returns>
        public static DateTime GetEndOfMonth(int Month, int Year)
        {
            return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month), 23, 59, 59, 999);
        }

        /// <summary>
        /// Hàm trả về ngày đầu tiên của tháng trước tháng hiện thời - Từ ngày hiện thời hệ thống (NOW)
        /// </summary>
        /// <returns>Ngày đầu tiên của tháng trước tháng hiện thời</returns>
        public static DateTime GetStartOfLastMonth()
        {
            if (DateTime.Now.Month == 1)
                return GetStartOfMonth(12, DateTime.Now.Year - 1);
            else
                return GetStartOfMonth(DateTime.Now.Month - 1, DateTime.Now.Year);
        }

        /// <summary>
        /// Hàm trả về ngày cuối cùng của tháng trước tháng hiện thời - Từ ngày hiện thời hệ thống (NOW)
        /// </summary>
        /// <returns>Ngày cuối cùng của tháng trước tháng hiện thời</returns>
        public static DateTime GetEndOfLastMonth()
        {
            if (DateTime.Now.Month == 1)
                return GetEndOfMonth(12, DateTime.Now.Year - 1);
            else
                return GetEndOfMonth(DateTime.Now.Month - 1, DateTime.Now.Year);
        }

        /// <summary>
        /// Hàm trả về ngày đầu tiên của của tháng hiện thời
        /// </summary>
        /// <returns>Ngày đầu tiên của của tháng hiện thời</returns>
        public static DateTime GetStartOfCurrentMonth()
        {
            return GetStartOfMonth(DateTime.Now.Month, DateTime.Now.Year);
        }

        /// <summary>
        /// Hàm trả về ngày cuối cùng của của tháng hiện thời
        /// </summary>
        /// <returns>Ngày cuối cùng của của tháng hiện thời</returns>
        public static DateTime GetEndOfCurrentMonth()
        {
            return GetEndOfMonth(DateTime.Now.Month, DateTime.Now.Year);
        }

        /// <summary>
        /// Hàm trả về ngày đầu tiên của Tháng - Từ ngày truyền vào
        /// </summary>
        /// <param name="date">Ngày truyền vào</param>
        /// <returns>Ngày đầu tiên của tháng</returns>
        public static DateTime GetBeginDateOfMonthOfDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Ngày cuối cùng của tháng từ ngày truyền vào
        /// </summary>
        /// <param name="date">Ngày truyền vào</param>
        /// <returns>Ngày cuối cùng của tháng</returns>
        public static DateTime GetEndDateOfMonthOfDate(DateTime dateTime)
        {
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
        }

        #endregion

        #region -----> Years <-----
        public static DateTime GetStartOfYear(int Year)
        {
            return new DateTime(Year, 1, 1, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfYear(int Year)
        {
            return new DateTime(Year, 12, DateTime.DaysInMonth(Year, 12), 23, 59, 59, 999);
        }

        public static DateTime GetStartOfLastYear()
        {
            return GetStartOfYear(DateTime.Now.Year - 1);
        }

        public static DateTime GetEndOfLastYear()
        {
            return GetEndOfYear(DateTime.Now.Year - 1);
        }

        public static DateTime GetStartOfCurrentYear()
        {
            return GetStartOfYear(DateTime.Now.Year);
        }

        public static DateTime GetEndOfCurrentYear()
        {
            return GetEndOfYear(DateTime.Now.Year);
        }
        #endregion

        public static DateTime StringToDateTime(string sDate, string sFormat)
        {
            try
            {
                if ((sFormat.Equals("yyyyMMddHHmmss") || sFormat.Equals("yyyy-MM-dd HH:mm:ss") || sFormat.Equals("yyyyMMdd HHmmss")
                    || sFormat.Equals("yyyy.MM.dd HH:mm:ss") || sFormat.Equals("yyyyMMdd")) && !string.IsNullOrEmpty(sDate))
                {
                    sDate = sDate.Replace("-", "").Replace(":", "").Replace(".", "").Replace(" ", "");
                    string yyyy = sDate.ToString().Substring(0, 4);
                    string mm = sDate.ToString().Substring(4, 2);
                    string dd = sDate.ToString().Substring(6, 2);

                    string HH = sFormat.Equals("yyyyMMdd") ? "00" : sDate.ToString().Substring(8, 2);
                    string minuteMM = sFormat.Equals("yyyyMMdd") ? "00" : sDate.ToString().Substring(10, 2);
                    string ss = sFormat.Equals("yyyyMMdd") ? "00" : sDate.ToString().Substring(12, 2);

                    return new DateTime(int.Parse(yyyy), int.Parse(mm), int.Parse(dd), int.Parse(HH), int.Parse(minuteMM), int.Parse(ss));
                }
                else
                {
                    DateTime dDate = DateTime.ParseExact(sDate, sFormat, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None);
                    return dDate;
                }
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

        public static bool IsDateTime(string text)
        {
            DateTime dateTime;
            bool isDateTime = false;

            // Check for empty string.
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            isDateTime = DateTime.TryParse(text, out dateTime);

            return isDateTime;
        }
        public static bool IsDate(string tempDate)
        {
            try
            {
                if (string.IsNullOrEmpty(tempDate))
                {
                    return false;
                }
                DateTime fromDateValue;
                var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd" };
                if (DateTime.TryParseExact(tempDate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fromDateValue))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}