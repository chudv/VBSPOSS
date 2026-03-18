using System.Collections;
using System.Data;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace VBSPOSS.Utils
{
    public static class Utilities
    {
        public static string GetLocalIpAddress()
        {
            UnicastIPAddressInformation mostSuitableIp = null;

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;

                var properties = network.GetIPProperties();

                if (properties.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (!address.IsDnsEligible)
                    {
                        if (mostSuitableIp == null)
                            mostSuitableIp = address;
                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {
                        if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            mostSuitableIp = address;
                        continue;
                    }

                    return address.Address.ToString();
                }
            }

            return mostSuitableIp != null ? mostSuitableIp.Address.ToString() : "";
        }

        public static byte[] SerializeAndCompress(this object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Compress, true))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(zs, obj);
                return ms.ToArray();
            }
        }

        public static T DecompressAndDeserialize<T>(this byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Decompress, true))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (T)bf.Deserialize(zs);
            }
        }

        /// <summary>
        /// Hàm trả về mảng Id được tách từ chuỗi phân cách nhau bởi vChar
        /// </summary>
        /// <param name="vListId">Chuỗi cần tách</param>
        /// <param name="vChar">Ký tự phân cách các phần tử trong chuỗi</param>
        /// <returns>Mảng chứa các phần tử</returns>
        public static ArrayList Splip_Lists(string vListId, string vChar)
        {
            System.Collections.ArrayList ARL_Id = new System.Collections.ArrayList();
            if (vListId != "" && vChar != "")
            {
                string Temp = vListId;
                while (Temp.EndsWith(vChar))//Bỏ dấu chấm phẩy ở cuối chuỗi đi
                    Temp = Temp.Substring(0, Temp.Length - 1);
                char vC = char.Parse(vChar);
                string[] ARL_Temp = Temp.Split(vC); //Gán chuỗi Id vào mảng
                string v = "";
                for (int i = 0; i < ARL_Temp.Length; i++)
                {
                    v = ARL_Temp[i].ToString();
                    ARL_Id.Add(v);
                }
            }
            return ARL_Id;
        }

        /// <summary>
        /// Hàm tách chuỗi phân cách nhau bằng dấu _cdelimiter ( Ví dụ dấu , hoặc ; hay | ...) thành mảng string. 
        /// Có xóa phần tử null cuối cùng
        /// </summary>
        /// <param name="_lists">Chuỗi truyền vào cần cắt các phần tử đưa ra mảng</param>
        /// <param name="_cdelimiter">Ký tự phân cách của các phần tử trong chuỗi cần cắt ra mảng</param>
        /// <returns>Mảng phần tử</returns>
        public static string[] Splip_Strings(string _lists, string _cdelimiter)
        {
            string[] find = null;
            try
            {
                find = _lists.Split(new string[] { _cdelimiter }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                find = new string[] { string.Empty };
            }
            return find;
        }

        /// <summary>
        /// Hàm tách chuỗi chỉ số phân cách nhau bằng dấu(, hoặc ;) thành mảng string. Có xóa phần tử null cuối cùng
        /// </summary>
        /// <param name="_lists">Chuỗi truyền vào</param>
        /// <returns>Mảng phần tử</returns>
        public static string[] Splip_Lists(string _lists)
        {
            string[] find;
            try
            {
                find = _lists.Split(new string[] { ", ", ",", "; ", ";", "|", "| " }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                find = new string[] { string.Empty };
            }
            return find;
        }

        static string IntListToDelimitedString(List<string> pArlLists, string pDelimiter)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < pArlLists.Count; i++)
            {
                builder.Append(pArlLists[i].ToString());
                if (i != pArlLists.Count - 1)
                    builder.Append(pDelimiter);
            }
            return builder.ToString();
        }
        public static string JoinItemInArrayList(List<string> pArlLists, string pSplitChar)
        {
            return string.Join(pSplitChar, pArlLists.Select(n => n.ToString()).ToArray());
        }


        public static string Find_Replace(string strInput)
        {
            string cstr = "";
            if (!string.IsNullOrEmpty(strInput))
            {
                if (strInput.IndexOf("'") >= 0)
                    cstr = strInput.Replace("'", "''");
                else if (strInput.IndexOf("\\") >= 0)
                    cstr = strInput.Replace("\\", "\\\\");
                else if (strInput.IndexOf("~") >= 0)
                    cstr = strInput.Replace("~", "#");
                else if (strInput.IndexOf("&") >= 0)
                    cstr = strInput.Replace("&", "-");
                else if (strInput == "NULL")
                    cstr = "NULL ";
                else
                    cstr = strInput;
                while (cstr.IndexOf("  ") > 0)
                    cstr = cstr.Replace("  ", " ");
            }
            return cstr;
        }

        /// <summary>
        /// Hàm thực hiện xóa ký tự ở đầu, ở cuối và xóa hai ký tự sát nhau theo tham số ký tự cần xóa truyền vào 
        /// </summary>
        /// <param name="pInput">Chuỗi cần chuẩn hóa xóa ký tự thừa ở đầu, cuối và hai ký tự liền kề</param>
        /// <param name="pDelimiter">Ký tự cần xóa ở đầu và cuối chuỗi và hai ký tự sát nhau</param>
        /// <returns>Chuỗi giá trị đã được chuẩn hóa</returns>
        public static string DeleteChar_FirstAndLast(string pInput, string pDelimiter)
        {
            string _StandardizedRet = "";
            if (!string.IsNullOrEmpty(pInput))
            {
                _StandardizedRet = pInput;
                _StandardizedRet = _StandardizedRet.Replace(pDelimiter + pDelimiter, pDelimiter);
                if (!string.IsNullOrEmpty(_StandardizedRet))
                {
                    while (_StandardizedRet.EndsWith(pDelimiter))       //Bỏ dấu chấm phẩy ở cuối chuỗi đi
                        _StandardizedRet = _StandardizedRet.Substring(0, _StandardizedRet.Length - 1);
                }
                if (!string.IsNullOrEmpty(_StandardizedRet))
                {
                    while (!string.IsNullOrEmpty(_StandardizedRet) && _StandardizedRet.Trim().Substring(0, 1) == pDelimiter)
                        _StandardizedRet = _StandardizedRet.Trim().Substring(1);
                }
            }
            return _StandardizedRet;
        }

        //public static string GetTelcoName(string phoneNo)
        //{
        //    string _telcoName = "";
        //    if (string.IsNullOrEmpty(phoneNo) || phoneNo.Length < 9 || phoneNo.Length > 11)
        //    {
        //        return "";
        //    }
        //    if (phoneNo.Length == 10 && phoneNo.StartsWith("0"))
        //    {
        //        phoneNo = phoneNo.Substring(1, 9);
        //    }
        //    else if (phoneNo.Length == 11 && phoneNo.StartsWith("84"))
        //    {
        //        phoneNo = phoneNo.Substring(2, 9);
        //    }

        //    //Viettel : 86, 96, 97, 98, 32, 33, 34, 35, 36, 37, 38, 39
        //    if (phoneNo.StartsWith("86") || phoneNo.StartsWith("96") || phoneNo.StartsWith("97") || phoneNo.StartsWith("98")
        //        || phoneNo.StartsWith("32") || phoneNo.StartsWith("33") || phoneNo.StartsWith("34") || phoneNo.StartsWith("35")
        //        || phoneNo.StartsWith("36") || phoneNo.StartsWith("37") || phoneNo.StartsWith("38") || phoneNo.StartsWith("39"))
        //        return BankGateway.Infrastructure.Constants.OperatorName.Viettel;

        //    //Mobifone: 89, 90, 93, 70, 79, 77, 76, 78
        //    if (phoneNo.StartsWith("89") || phoneNo.StartsWith("90") || phoneNo.StartsWith("93")
        //        || phoneNo.StartsWith("70") || phoneNo.StartsWith("79") || phoneNo.StartsWith("77") || phoneNo.StartsWith("76") || phoneNo.StartsWith("78"))
        //        return BankGateway.Infrastructure.Constants.OperatorName.Mobifone;

        //    //Vinaphone: 88, 91, 94, 83, 84, 85, 81, 82
        //    if (phoneNo.StartsWith("88") || phoneNo.StartsWith("91") || phoneNo.StartsWith("94")
        //        || phoneNo.StartsWith("83") || phoneNo.StartsWith("84") || phoneNo.StartsWith("85") || phoneNo.StartsWith("81") || phoneNo.StartsWith("82"))
        //        return BankGateway.Infrastructure.Constants.OperatorName.Vinaphone;

        //    //Vietnamobile: 92, 56, 58
        //    if (phoneNo.StartsWith("92") || phoneNo.StartsWith("56") || phoneNo.StartsWith("58"))
        //        return BankGateway.Infrastructure.Constants.OperatorName.Vietnamobile;

        //    //Gmobile:      99, 59
        //    if (phoneNo.StartsWith("99") || phoneNo.StartsWith("59"))
        //        return BankGateway.Infrastructure.Constants.OperatorName.Gmobile;

        //    return _telcoName;
        //}

        /// <summary>
        /// Hàm kiểm tra xem chuỗi truyền vào có phải là số không - Function to test whether the string is valid number or not
        /// </summary>
        /// <param name="strNumber">Chuỗi cần kiểm tra</param>
        /// <returns>True: Nếu là số. False: Không phải số</returns>
        public static bool IsNumber(String strNumber)
        {
            try
            {
                System.Text.RegularExpressions.Regex objNotNumberPattern = new System.Text.RegularExpressions.Regex("[^0-9.-]");
                System.Text.RegularExpressions.Regex objTwoDotPattern = new System.Text.RegularExpressions.Regex("[0-9]*[.][0-9]*[.][0-9]*");
                System.Text.RegularExpressions.Regex objTwoMinusPattern = new System.Text.RegularExpressions.Regex("[0-9]*[-][0-9]*[-][0-9]*");
                String strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
                String strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
                System.Text.RegularExpressions.Regex objNumberPattern = new System.Text.RegularExpressions.Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");
                return !objNotNumberPattern.IsMatch(strNumber) && !objTwoDotPattern.IsMatch(strNumber) && !objTwoMinusPattern.IsMatch(strNumber) && objNumberPattern.IsMatch(strNumber);
            }
            catch
            {
                return false;
            }
        }

        public static DateTime GetLongDateEndOfMonth(int Month, int Year)
        {
            return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month), 23, 59, 59, 999);
        }

        public static DateTime GetShortDateEndOfMonth(int Month, int Year)
        {
            return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month), 00, 00, 00, 000);
        }



        /// <summary>
        /// Hàm kiểm tra xem trong mảng có chứa giá trị truyền vào không ?
        /// </summary>
        /// <param name="_Lists">Mảng string truyền vào</param>
        /// <param name="val">Giá trị cần kiểm tra</param>
        /// <returns>True: Có. False: Không</returns>
        public static bool IsInArray(string[] _Lists, string val)
        {
            if (val == null || _Lists == null)
                return false;
            foreach (string member in _Lists)
            {
                if (member.Equals(val))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Hàm kiểm tra tính xác thực của địa chỉ e-mail. Return true if it is in valid email format.
        /// </summary>
        /// <param name="strEmail">Chuỗi địa chỉ e-mail truyền vào</param>
        /// <returns>True: Thỏa mãn. False không thỏa mãn</returns>
        public static bool IsValidEmail(string strEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            System.Text.RegularExpressions.Regex _rex = new System.Text.RegularExpressions.Regex(strRegex);
            if (_rex.IsMatch(strEmail))
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Return true if it is in valid datetime format "dd/MM/yyyy Or dd-MM-yyyy Or dd.MM.yyyy". Hàm này chỉ kiểm tra về định dạng
        /// Còn với ví dụ tháng 02 thường không có ngày 31 thì hàm này không kiểm tra được.
        /// </summary>
        /// <param name="strDate">Ngày tháng năm truyền vào</param>
        /// <returns>True: Nếu hợp lệ</returns>
        public static bool IsDateTime(string strDate)
        {
            string strRegex = @"(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)[0-9]{2}";
            System.Text.RegularExpressions.Regex _rex = new System.Text.RegularExpressions.Regex(strRegex);
            if (_rex.IsMatch(strDate))
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Return true if it is in valid address ip format "192.168.0.2".
        /// </summary>
        /// <param name="AddressIP">Địa chỉ IP cần kiểm tra xem đúng định dạng chưa</param>
        /// <returns>True: Nếu hợp lệ</returns>
        public static bool IsAddressIP(string AddressIP)
        {
            string _Regex = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
            System.Text.RegularExpressions.Regex _rex = new System.Text.RegularExpressions.Regex(_Regex);
            if (_rex.IsMatch(AddressIP))
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Return true if it is in valid address ip format "192.168.0.2".
        /// </summary>
        /// <param name="AddressIP">Địa chỉ IP cần kiểm tra xem đúng định dạng chưa. Kiểm tra riêng dải địa chi ví dụ: 10.25.0.XXX</param>
        /// <returns>True: Nếu hợp lệ</returns>
        public static bool IsAddressIPXXX(string AddressIP)
        {
            string _Regex = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.";
            System.Text.RegularExpressions.Regex _rex = new System.Text.RegularExpressions.Regex(_Regex);
            if (_rex.IsMatch(AddressIP))
                return (true);
            else
                return (false);
        }

        /// <summary>
        /// Hàm trả về true nếu chuỗi truyền vào có số ký tự thuộc xác định khoảng. 
        /// Return true if it is in valid string format 
        /// "Your string must contain between _minChar and _maxChar characters!".
        /// </summary>
        /// <param name="_Password">Password is checked</param>
        /// <returns>True: If is in valid password format</returns>
        public static bool IsLengChar(string _string, int _minChar, int _maxChar)
        {
            System.Text.RegularExpressions.Regex _rex = new System.Text.RegularExpressions.Regex(@"^\w{" + _minChar + "," + _maxChar + "}$");
            return _rex.IsMatch(_string);
        }

        /// <summary>
        /// Return true if it is in valid string format. "Your password must contain at least one number and letter!"
        /// Hàm trả về true khi mật khẩu chứa tối thiểu một chữ số và ký tự. Và chuỗi không chứa khoảng trống Non-space
        /// </summary>
        /// <param name="_Password"></param>
        /// <returns></returns>
        public static bool IsPassword(string _Password)
        {
            string _Regex = @"^\S[a-zA-Z]+\w*\d+\w*";
            System.Text.RegularExpressions.Regex _rex = new System.Text.RegularExpressions.Regex(_Regex);
            return _rex.IsMatch(_Password);
        }

        /// <summary>
        /// Mật khẩu phải ít nhất _minChar ký tự và ko vượt quá maxChar ký tự 
        /// Và ít nhất 1 ký tự viết hoa, 1 ký tự viết thường và 1 chữ số.
        /// </summary>
        /// <param name="_Password"></param>
        /// <param name="_minChar"></param>
        /// <param name="maxChar"></param>
        /// <returns></returns>
        public static bool IsPassword(string _Password, int _minChar, int _maxChar)
        {
            string _Regex = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{" + _minChar + "," + _maxChar + "}$";
            System.Text.RegularExpressions.Regex _rex = new System.Text.RegularExpressions.Regex(_Regex);
            return _rex.IsMatch(_Password);
        }

        /// <summary>
        /// Function to test for Positive Integers. Hàm kiểm tra xem chuỗi truyền vào có phải là số tự nhiên không ?
        /// </summary>
        /// <param name="strNumber"></param>
        /// <returns>True: Nến hợp lệ</returns>
        public static bool IsNaturalNumber(String strNumber)
        {
            System.Text.RegularExpressions.Regex objNotNaturalPattern = new System.Text.RegularExpressions.Regex("[^0-9]");
            System.Text.RegularExpressions.Regex objNaturalPattern = new System.Text.RegularExpressions.Regex("0*[1-9][0-9]*");
            return !objNotNaturalPattern.IsMatch(strNumber) &&
            objNaturalPattern.IsMatch(strNumber);
        }

        /// <summary>
        /// Hàm thực hiện lấy các ký tự đầu tiên của chuỗi truyền vào - Get the first character from a number of words
        /// </summary>
        /// <param name="pWords">Chuỗi cần lấy các ký tự đầu tiên của các từ</param>
        /// <param name="pCaseUL">Chỉ số xác định chuỗi trả về. Quy ước: 1 - Chuỗi chữ hoa; 2 - Thường; 3 - Mặc định</param>
        /// <param name="pIsSign">Giá trị xác định có chuyển ký tự có dấu thành không dấu không? Với: True - Trả về giá trị không có dấu tiếng việt; False - Mặc định</param>
        /// <returns>Chuỗi ký tự trả về</returns>
        public static string GetFirst_CharWords(string pWords, byte pCaseUL, bool pIsSign)
        {
            string _RetStr = "";
            if (pWords.Trim().Length > 0)
            {
                string[] ARL_Char = pWords.Split(' ');
                if (ARL_Char.Length > 0)
                {
                    foreach (string sChar in ARL_Char)
                    {
                        _RetStr = _RetStr + sChar[0].ToString();
                    }
                    if (pCaseUL == 1)
                        _RetStr = _RetStr.ToUpper();
                    else if (pCaseUL == 2)
                        _RetStr = _RetStr.ToLower();
                    if (pIsSign)
                        _RetStr = UnicodeToUnSign(_RetStr);
                }
            }
            return _RetStr;
        }


        /// <summary>
        /// Hàm thực hiện xóa khoảng trắng thừa trong chuỗi truyền vào
        /// </summary>
        /// <param name="sInput">Chuỗi cần xóa những khoảng trắng thừa</param>
        /// <returns>Chuỗi đã được chuẩn hóa dấu cách thừa</returns>
        public static string Delete_Spaces(string sInput)
        {
            string sret = "";
            //Xóa các dấu cách thừa ở đầu và ở cuối chuỗi
            sInput = sInput.Trim();
            //Thực hiện xóa các dấu cách thừa ở bên trong chuỗi
            int n = sInput.Length;
            int i = 0;
            while (i < n)
            {
                while ((i < n) && (sInput[i] != (char)32))
                {
                    sret += sInput[i];
                    i++;
                }
                if (i < n)
                {
                    sret += sInput[i];
                    while ((i < n) && (sInput[i] == (char)32))
                    {
                        i++;
                    }
                }
            }
            return sret;
        }

        /// <summary>
        /// Hàm thực hiện xóa khoảng trắng thừa trong chuỗi truyền vào
        /// </summary>
        /// <param name="sInput">Chuỗi cần xóa những khoảng trắng thừa</param>
        /// <returns>Chuỗi đã được chuẩn hóa dấu cách thừa</returns>
        public static string Collapse_Spaces(string sInput)
        {
            string sret = "";
            //Thay thế những ký tự trắng (từ 2 ký tự trắng trở lên) thành 1 khoảng trắng
            sret = System.Text.RegularExpressions.Regex.Replace(sInput, @"\s+", " ");      //Với \s =>Ký tự khoảng trắng tương đương [ \f\n\r\t\v]
            return sret;
        }

        /// <summary>
        /// Hàm thực hiện viết hoa ký từ đầu mỗi từ của chuỗi truyền vào
        /// </summary>
        /// <param name="sInput">Chuỗi cần chuẩn hóa lại ký tự đầu mỗi từ</param>
        /// <returns>Chuỗi đã được viết hoa các từ đầu tiên của chuỗi</returns>
        public static string ProperUnicode(string sInput)
        {
            string sret = "";
            sInput = sInput.Trim();
            sInput = Collapse_Spaces(sInput);
            string[] arrWord = sInput.Split((char)32);
            string sFirstChar = "";
            for (int i = 0; i < arrWord.Length; i++)
            {
                sFirstChar = arrWord[i] != "" ? arrWord[i].Substring(0, 1) : "";
                sret += arrWord[i] != "" ? sFirstChar.ToUpper() + arrWord[i].Remove(0, 1) + (char)32 : "";
            }
            return sret;
        }

        /// <summary>
        /// Hàm chuyển đổi chuỗi ký tự có dấu thành không dấu. Hàm này dựa hoàn toàn vào mã ACSII của các ký tự đặc biệt
        /// (trừ ký tự cách trắng – space có mã ACSII là 32), sau đó remove các ký tự đó khỏi chuỗi
        /// Đối với các ký tự cách trắng thì replace thành dấu "-". Tiếp theo là chuyển các ký tự có dấu thành không dấu.
        /// </summary>
        /// <param name="sText">Chuỗi cần chuyển thành chuỗi không dấu</param>
        /// <returns>Chuỗi không dấu</returns>
        public static string ConvertToUnSign(string sInput)
        {
            System.Text.RegularExpressions.Regex v_reg_regex = new System.Text.RegularExpressions.Regex("\\p{IsCombiningDiacriticalMarks}+");
            string v_str_FormD = sInput.Normalize(NormalizationForm.FormD);
            return v_reg_regex.Replace(v_str_FormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        /// <summary>
        /// Hàm thực hiện chuyển chuỗi Unicode có dấu thành không dấu
        /// </summary>
        /// <param name="sText">Chuỗi cần convert không dấu</param>
        /// <returns>Chuỗi không dấu trả về</returns>
        public static string UnicodeToUnSign(string sInput)
        {
            //Khai báo hằng số tương ứng
            const string sUnicode = "0224 0225 7843 0227 7841 0259 7857 7855 7859 7861 7863 0226 7847 7845 7849 7851 7853 0192 0193 7842 0195 7840 0258 7856 7854 7858 7860 7862 0194 7846 7844 7848 7850 7852 0242 0243 7887 0245 7885 0244 7891 7889 7893 7895 7897 0417 7901 7899 7903 7905 7907 0210 0210 0211 7886 0213 7884 0212 7890 7888 7892 7894 7896 0416 7900 7898 7902 7904 7906 0232 0233 7867 7869 7865 0234 7873 7871 7875 7877 7879 0200 0201 7866 7868 7864 0202 7872 7870 7874 7876 7878 0249 0250 7911 0361 7909 0432 7915 7913 7917 7919 7921 0217 0218 7910 0360 7908 0431 7914 7912 7916 7918 7920 0236 0237 7881 0297 7883 0204 0205 7880 0296 7882 7923 0253 7927 7929 7925 7922 0221 7926 7928 7924 0273 0272";
            const string sAnsii = "0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0097 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0065 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0111 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0079 0101 0101 0101 0101 0101 0101 0101 0101 0101 0101 0101 0069 0069 0069 0069 0069 0069 0069 0069 0069 0069 0069 0117 0117 0117 0117 0117 0117 0117 0117 0117 0117 0117 0085 0085 0085 0085 0085 0085 0085 0085 0085 0085 0085 0105 0105 0105 0105 0105 0073 0073 0073 0073 0073 0121 0121 0121 0121 0121 0089 0089 0089 0089 0089 0100 0068";
            string sRet = "", sTemp = "";
            int i = 0, iPos = -1;
            sInput = sInput.Trim();
            int n = sInput.Length;
            while (i < n)
            {
                sTemp = sInput.Substring(i, 1);
                //AscW: Chuyển ký tự thành mã Ascii có hỗ trợ Unicode ==> C#.NET: System.Convert.ToInt32('A') ToString();
                string _Ascii = Convert.ToInt32(Convert.ToChar(sTemp)).ToString();
                while (_Ascii.Length < 4)
                {
                    _Ascii = "0" + _Ascii;
                }
                iPos = sUnicode.IndexOf(_Ascii);  //Cách lấy mã Ascii của 1 kí tự
                if (iPos >= 0)
                {
                    //ChrW: Chuyển mã Ascii thành ký tự có hỗ trợ Unicode ==>C#.NET: System.Convert.ToChar(65) ToString();
                    sRet += System.Convert.ToChar(System.Convert.ToInt32(sAnsii.Substring(iPos, 4))).ToString();
                }
                else
                {
                    int iAscii = System.Convert.ToInt32(Convert.ToChar(sTemp));
                    if (iAscii == 272 || iAscii == 208)
                        sTemp = System.Convert.ToChar(68).ToString();
                    sRet += sTemp;
                }
                i++;
            }
            return sRet;
        }

        /// <summary>
        /// Hàm thực hiện đếm số lượng từ truyền vào từ chuỗi.
        /// </summary>
        /// <param name="sInput">Chuỗi cần đếm số lượng từ</param>
        /// <returns>Số từ trong chuỗi</returns>
        public static int CountWords(string sInput)
        {
            int iCountWords = 0;
            string groupOfWords = @"It's a text for counting of words, with different word " +
                           "boundaries and hyphenated word like the all-clear.Is it OK? ";
            var matchesByListedChars = Regex.Matches(groupOfWords, @"[^\s.?,]+", RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            iCountWords = matchesByListedChars.Count;

            //var matchesByAlphaNumeric = Regex.Matches(groupOfWords,  @"[\w]+", RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return iCountWords;
        }
        public static int CountWords2(string sInput)
        {
            int iCountWords = 0;
            for (int i = 1; i < sInput.Length; i++)
            {
                if (char.IsWhiteSpace(sInput[i - 1]) == true)
                {
                    if (char.IsLetterOrDigit(sInput[i]) == true || char.IsPunctuation(sInput[i]))
                    {
                        iCountWords++;
                    }
                }
            }
            if (sInput.Length > 2)
            {
                iCountWords++;
            }
            return iCountWords;
        }

        public static List<string> GetPropertiesName(Object obj)
        {
            List<string> result = new List<string>();
            PropertyInfo[] destinFieldList = obj.GetType().GetProperties();
            foreach (PropertyInfo dField in destinFieldList)
            {
                result.Add(dField.Name);
            }
            return result;
        }

        public static object GetPropertyValue(Object obj, string fieldName)
        {
            PropertyInfo[] destinFieldList = obj.GetType().GetProperties();
            foreach (PropertyInfo dField in destinFieldList)
            {
                if (dField.Name.Equals(fieldName))
                {
                    return dField.GetValue(obj);
                }
            }
            return null;
        }

        public static bool IsNumeric(DataColumn col)
        {
            if (col == null)
                return false;
            // Make this const
            var numericTypes = new[] { typeof(Byte), typeof(Decimal), typeof(Double), typeof(Int16), typeof(Int32), typeof(Int64), typeof(SByte), typeof(Single), typeof(UInt16), typeof(UInt32), typeof(UInt64) };
            return numericTypes.Contains(col.DataType);
        }

        private static string[] ChuSo = new string[10] { " không", " một", " hai", " ba", " bốn", " năm", " sáu", " bẩy", " tám", " chín" };
        private static string[] Tien = new string[6] { "", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ" };
        // Hàm đọc số thành chữ
        public static string ReadMoney(long SoTien, string strTail)
        {
            int lan, i;
            long so;
            string KetQua = "", tmp = "";
            int[] ViTri = new int[6];
            if (SoTien < 0) return "Số tiền âm !";
            if (SoTien == 0) return "Không đồng !";
            if (SoTien > 0)
            {
                so = SoTien;
            }
            else
            {
                so = -SoTien;
            }
            //Kiểm tra số quá lớn
            if (SoTien > 8999999999999999)
            {
                SoTien = 0;
                return "";
            }
            ViTri[5] = (int)(so / 1000000000000000);
            so = so - long.Parse(ViTri[5].ToString()) * 1000000000000000;
            ViTri[4] = (int)(so / 1000000000000);
            so = so - long.Parse(ViTri[4].ToString()) * +1000000000000;
            ViTri[3] = (int)(so / 1000000000);
            so = so - long.Parse(ViTri[3].ToString()) * 1000000000;
            ViTri[2] = (int)(so / 1000000);
            ViTri[1] = (int)((so % 1000000) / 1000);
            ViTri[0] = (int)(so % 1000);
            if (ViTri[5] > 0)
            {
                lan = 5;
            }
            else if (ViTri[4] > 0)
            {
                lan = 4;
            }
            else if (ViTri[3] > 0)
            {
                lan = 3;
            }
            else if (ViTri[2] > 0)
            {
                lan = 2;
            }
            else if (ViTri[1] > 0)
            {
                lan = 1;
            }
            else
            {
                lan = 0;
            }
            for (i = lan; i >= 0; i--)
            {
                tmp = ReadMoney3Number(ViTri[i]);
                KetQua += tmp;
                if (ViTri[i] != 0) KetQua += Tien[i];
                if ((i > 0) && (!string.IsNullOrEmpty(tmp))) KetQua += " ";//&& (!string.IsNullOrEmpty(tmp))
            }
            if (KetQua.Substring(KetQua.Length - 1, 1) == " ") KetQua = KetQua.Substring(0, KetQua.Length - 1);
            KetQua = KetQua.Trim() + strTail + " đồng.";
            return KetQua.Substring(0, 1).ToUpper() + KetQua.Substring(1);
        }
        // Hàm đọc số có 3 chữ số
        private static string ReadMoney3Number(int baso)
        {
            int tram, chuc, donvi;
            string KetQua = "";
            tram = (int)(baso / 100);
            chuc = (int)((baso % 100) / 10);
            donvi = baso % 10;
            if ((tram == 0) && (chuc == 0) && (donvi == 0)) return "";
            if (tram != 0)
            {
                KetQua += ChuSo[tram] + " trăm";
                if ((chuc == 0) && (donvi != 0)) KetQua += " linh";
            }
            if ((chuc != 0) && (chuc != 1))
            {
                KetQua += ChuSo[chuc] + " mươi";
                if ((chuc == 0) && (donvi != 0)) KetQua = KetQua + " linh";
            }
            if (chuc == 1) KetQua += " mười";
            switch (donvi)
            {
                case 1:
                    if ((chuc != 0) && (chuc != 1))
                    {
                        KetQua += " mốt";
                    }
                    else
                    {
                        KetQua += ChuSo[donvi];
                    }
                    break;
                case 5:
                    if (chuc == 0)
                    {
                        KetQua += ChuSo[donvi];
                    }
                    else
                    {
                        KetQua += " lăm";
                    }
                    break;
                default:
                    if (donvi != 0)
                    {
                        KetQua += ChuSo[donvi];
                    }
                    break;
            }
            return KetQua;
        }

        /// <summary>
        /// Hàm chuyển chuỗi định dạng HTML thành chuỗi bình thường 
        /// </summary>
        /// <param name="source">Chuỗi HTML cần chuyển đổi thành PlainText</param>
        /// <returns>Chuỗi PlainText</returns>
        public static string StripHTML(string source)
        {
            try
            {
                string result;

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result, @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result, @"<( )*style([^>])*>", "<style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, @"(<( )*(/)( )*style( )*>)", "</style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result, "(<style>).*(</style>)", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", "\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result, @" ", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                result = result.Replace("\n", "\r");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r)", "\t\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)( )+(\t)", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+(\r)", "\r\r",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r)(\t)+", "\r\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\r\r";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\r");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r";
                    tabs = tabs + "\t";
                }

                return result;
            }
            catch
            {
                return source;
            }
        }
    }
}
