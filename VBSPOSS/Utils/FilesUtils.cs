using System.Text;

namespace VBSPOSS.Utils
{
    public class FilesUtils
    {
        /// <summary>
        /// Cấu trúc của file
        /// </summary>
        public struct RowFile
        {
            public int File_Id;
            public string File_Path;
            public string File_Name_Old;
            public string File_Name_New;
            public string File_Size;
            public string File_Ext;
            public RowFile(int _File_Id, string _File_Path, string _File_Name_Old, string _File_Name_New, string _File_Size, string _File_Ext)
            {
                File_Id = _File_Id;
                File_Path = _File_Path;
                File_Name_Old = _File_Name_Old;
                File_Name_New = _File_Name_New;
                File_Size = _File_Size;
                File_Ext = _File_Ext;
            }
        }

        /// <summary>
        /// Hàm kiểm tra tồn tại thư mục hay không. True nếu tồn tại và ngược lại
        /// </summary>
        /// <param name="sDirectory">Đường dẫn thư mục cần kiểm tra</param>
        /// <returns>T - Tồn tại thư mục. F - Không tồn tại thư mục</returns>
        public static bool IsDirectory(string sDirectory)
        {
            return System.IO.Directory.Exists(sDirectory);
        }

        /// <summary>
        /// Hàm kiểm tra xem file có tồn tại hay không. Hàm trả về giá trị True nếu tồn tại
        /// </summary>
        /// <param name="sFile">File cần kiểm tra</param>
        /// <returns>True - Nếu file tồn tại. False - File không tồn tại</returns>
        public static bool IsFile(string sFile)
        {
            return System.IO.File.Exists(sFile);
        }

        /// <summary>
        /// Hàm thực hiện trả về tên file từ đường dẫn của file - Get file name for path file
        /// </summary>
        /// <param name="sFilePath">Đường dẫn của file truyền vào</param>
        /// <param name="bFlagExt">False: Tên có kèm theo đuôi file. True: Tên không có đuôi file</param>
        /// <returns>Tên file cần lấy</returns>
        public static string GetFileName(string sFilePath, bool bFlagExt)
        {
            string sRet = "";
            sRet = sFilePath;
            int n = sRet.LastIndexOf("\\");
            sRet = sRet.Substring(n + 1);
            if (bFlagExt == true)
            {
                sRet = sRet.Substring(0, sRet.LastIndexOf("."));
            }
            return sRet;
        }

        /// <summary>
        /// Hàm thực hiện trả về chuỗi chứa đuôi của file truyền vào - Get format file
        /// </summary>
        /// <param name="sFilePath">Đường dẫn của file</param>
        /// <returns>Đuôi của file</returns>
        public static string GetExtention(string sFilePath)
        {
            string sExtFile = "";
            if (System.IO.File.Exists(sFilePath))
            {
                System.IO.FileInfo _fileinfo = new System.IO.FileInfo(sFilePath);
                sExtFile = _fileinfo.Extension.ToString();
            }
            return sExtFile;
        }

        /// <summary>
        /// Hàm thực hiện trả về chuỗi chứa đuôi của file truyền vào - Get format file
        /// </summary>
        /// <param name="sFilePath">Đường dẫn của file</param>
        /// <returns>Đuôi của file</returns>
        public static DateTime GetDateFile(string sFilePath)
        {
            DateTime _dateFile = DateTime.Now;
            if (System.IO.File.Exists(sFilePath))
            {
                System.IO.FileInfo _fileinfo = new System.IO.FileInfo(sFilePath);
                _dateFile = _fileinfo.CreationTime;
            }
            return _dateFile;
        }

        /// <summary>
        /// Hàm lấy tổng dung lượng của đường dẫn truyền vào
        /// </summary>
        /// <param name="sPath">Đường dẫn của file hoặc thư mục cần tính tổng Size</param>
        /// <param name="_IncludeSubdirectories">True: Nếu có thư mục con. False: Nếu không có thư mục con</param>
        /// <returns>Tổng dung lượng - Get total size</returns>
        public static double GetDirSize(string sPath, bool _IncludeSubdirectories)
        {
            double dTotalSize = 0;          //Tổng dung lượng
            System.IO.DirectoryInfo dirInfors = new System.IO.DirectoryInfo(sPath);
            System.IO.FileInfo[] files = dirInfors.GetFiles();
            foreach (System.IO.FileInfo _file in files)
            {
                dTotalSize += _file.Length;
            }
            if (_IncludeSubdirectories)
            {
                System.IO.DirectoryInfo[] dirs = dirInfors.GetDirectories();
                foreach (System.IO.DirectoryInfo dir in dirs)
                {
                    dTotalSize += GetDirSize(dir.FullName, true);
                }
            }
            return dTotalSize;
        }


        /// <summary>
        /// Hàm thực hiện trả về dung lượng file tính theo bytes từ đường dẫn file truyền vào
        /// Ex: GetSizeFile(@"C:\Data\Huong dan su dung.doc") ==> Result: 1022992
        /// </summary>
        /// <param name="sPathFile">Đường dẫn file truyền vào</param>
        /// <returns>Dung lượng của file tính theo bytes (Ex: Result = 1022992)</returns>
        public static long GetSizeFile(string sPathFile)
        {
            try
            {
                System.IO.FileInfo fileInfor = new System.IO.FileInfo(sPathFile);
                long dSize = fileInfor.Length;
                return dSize;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Hàm tính tổng dung lượng của đường dẫn truyền vào: Bao gồm cả file và sub directory
        /// </summary>
        /// <param name="vDirPath"></param>
        /// <returns></returns>
        public static double GetDirSize(string sPath)
        {
            double dsize = 0;
            System.IO.DirectoryInfo vDirInfo = new System.IO.DirectoryInfo(sPath);
            foreach (System.IO.FileInfo vFile in vDirInfo.GetFiles())
            {
                dsize += vFile.Length;
            }
            foreach (System.IO.DirectoryInfo vSubDir in vDirInfo.GetDirectories())
            {
                dsize += GetDirSize(vSubDir.FullName);
            }
            return dsize;
        }

        /// <summary>
        /// Hàm thực hiện trả về Format của dung lượng file từ dung lượng kiểu số tính theo byte truyền vào
        /// Ex: GetFileSize(30983) ==> Result: 30.26 KB
        /// </summary>
        /// <param name="lgBytes">Số bytes của file</param>
        /// <returns>Chuỗi dung lượng file đã được format (Ex: Result = "10.05 MB")</returns>
        public static string GetFileSize(long lgBytes)
        {
            string sRet = "";
            long kilobyte = 1024;
            long megabyte = 1024 * kilobyte;
            long gigabyte = 1024 * megabyte;
            long terabyte = 1024 * gigabyte;
            if (lgBytes > terabyte)
            {
                Decimal size = Decimal.Divide(lgBytes, terabyte);
                sRet = String.Format("{0:0.00} TB", size);
            }
            else if (lgBytes >= gigabyte)
            {
                Decimal size = Decimal.Divide(lgBytes, gigabyte);
                sRet = String.Format("{0:0.00} GB", size);
            }
            else if (lgBytes >= megabyte)
            {
                Decimal size = Decimal.Divide(lgBytes, megabyte);
                sRet = String.Format("{0:0.00} MB", size);
            }
            else if (lgBytes >= kilobyte)
            {
                Decimal size = Decimal.Divide(lgBytes, kilobyte);
                sRet = String.Format("{0:0.00} KB", size);
            }
            else if (lgBytes > 0 && lgBytes < kilobyte)
            {
                Decimal size = lgBytes;
                sRet = String.Format("{0:0.00} Bytes", size);
            }
            else
            {
                sRet = "0 Bytes";
            }
            return sRet;
        }

        /// <summary>
        /// Hàm thực hiện trả về dung lượng file theo định dạng Fomat từ đường dẫn file truyền vào
        /// Ex: GetFileSize(@"C:\Data\Huong dan su dung.doc") ==> Result: 10.05 MB
        /// </summary>
        /// <param name="sPathFile">Đường dẫn file truyền vào</param>
        /// <returns>Chuỗi dung lượng file đã được format (Ex: Result = "10.05 MB")</returns>
        public static string GetFileSize(string sPathFile)
        {
            try
            {
                string sRet = "";
                System.IO.FileInfo fileInfor = new System.IO.FileInfo(sPathFile);
                long dSize = fileInfor.Length;
                sRet = GetFileSize(dSize);
                return sRet;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hàm thực hiện đổi dung lượng file đang được format ở dạng chuỗi về dung lượng file theo bytes
        ///     Ex: GetBytesFile("10 MB") ==> Result: 10485760
        /// </summary>
        /// <returns>Dung lượng file tính theo bytes</returns>
        public static double GetBytesFile(string sSize)
        {
            double dSizeByte = 0;
            if (sSize.EndsWith("KB"))
            {
                dSizeByte = Convert.ToDouble(sSize.Substring(0, sSize.LastIndexOf(" "))) * 1024;
            }
            else if (sSize.EndsWith("MB"))
            {
                dSizeByte = Convert.ToDouble(sSize.Substring(0, sSize.LastIndexOf(" "))) * 1024 * 1024;
            }
            else if (sSize.EndsWith("GB"))
            {
                dSizeByte = Convert.ToDouble(sSize.Substring(0, sSize.LastIndexOf(" "))) * 1024 * 1024 * 1024;
            }
            else if (sSize.EndsWith("TB"))
            {
                dSizeByte = Convert.ToDouble(sSize.Substring(0, sSize.LastIndexOf(" "))) * 1024 * 1024 * 1024 * 1024;
            }
            else dSizeByte = Convert.ToDouble(sSize.Substring(0, sSize.LastIndexOf(" ")));
            return dSizeByte;
        }

        /// <summary>
        /// Hàm thực hiện copy một file - Copy file
        /// </summary>
        /// <param name="srcFile">Đường dẫn chứa file nguồn</param>
        /// <param name="dstFile">Đường dẫn đích</param>
        /// <returns>True: Copy file thành công</returns>
        public static bool Copy_File(string srcFile, string dstFile)
        {
            try
            {
                System.IO.File.Copy(srcFile, dstFile, true);
                return true;
            }
            catch
            {
                Console.WriteLine("Thuc hien copy file khong thanh cong :\n" + GetFileName(srcFile, false));
                return false;
            }
        }

        /// <summary>
        /// Hàm thực hiện xóa file truyền vào. Delete file from path file input
        /// </summary>
        /// <param name="sFile">Path file Cần xóa</param>
        /// <returns>True: Xóa thành công</returns>
        public static bool Delete_File(string sFile)
        {
            try
            {
                if (System.IO.File.Exists(sFile))
                {
                    System.IO.File.Delete(sFile);
                    return true;
                }
                else return false;
            }
            catch
            {
                Console.WriteLine("Delete file: Thao tac khong hop le. Co the file nay dang duoc su dung!");
                return false;
            }
        }

        /// <summary>
        /// Hàm thực hiện chuẩn hóa lại tên file. Cắt bỏ các ký tự đặc biệt không hợp lệ
        /// </summary>
        /// <param name="fileName">Tên file cần chuẩn hóa lại</param>
        /// <returns>Tên file đã được chuẩn hóa</returns>
        /// 
        public static string Standard_FileName(string sFileName)
        {
            string sret = "";
            if (sFileName != "")
            {
                if (sFileName.IndexOf("|") >= 0)
                    sFileName = sFileName.Replace("|", "");
                if (sFileName.IndexOf("''") >= 0)
                    sFileName = sFileName.Replace("''", "");
                if (sFileName.IndexOf("*") >= 0)
                    sFileName = sFileName.Replace("*", "");
                if (sFileName.IndexOf("?") >= 0)
                    sFileName = sFileName.Replace("?", "");
                if (sFileName.IndexOf(":") >= 0)
                    sFileName = sFileName.Replace(":", "");
                if (sFileName.IndexOf(">") >= 0)
                    sFileName = sFileName.Replace(">", "");
                if (sFileName.IndexOf("<") >= 0)
                    sFileName = sFileName.Replace("<", "");
                if (sFileName.IndexOf("/") >= 0)
                    sFileName = sFileName.Replace("/", "");
                if (sFileName.IndexOf(@"\") >= 0)
                    sFileName = sFileName.Replace(@"\", "");
                sret = sFileName;
            }
            return sret;
        }

        /// <summary>
        /// Hàm thực hiện ghi dữ liệu vào file text theo nội dung trong chuỗi truyền vào
        /// </summary>
        /// <param name="sPath">Đường dẫn file text</param>
        /// <param name="sContent">Nội dung cần ghi</param>
        /// <returns>True - Ghi dữ liệu thành công; Flase - Không thành công</returns>
        public static bool Write_Text(string sPath, string sContent)
        {
            System.IO.FileStream fs = null;
            try
            {
                if (!System.IO.File.Exists(sPath))
                    fs = new System.IO.FileStream(sPath, System.IO.FileMode.Create);            //Thực hiện tạo một file mới
                else
                    //Mở tệp tin văn bản, di chuyển con trỏ về cuối file để thực hiện ghi dữ liệu.
                    fs = new System.IO.FileStream(sPath, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                //Tạo một writer và chỉ định kiểu mã hóa. Kiểu mặc định (UTF-8) hỗ trợ ký tự Unicode, Nhưng mã hóa ký tự chuẩn giống như Ascii
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, Encoding.UTF8))
                {
                    //Thực hiện ghi nội dung một chuỗi vào file text (Write text strings to the stream)
                    sw.Write(sContent);
                    // Bảo đảm tất cả dữ liệu được ghi từ buffer.
                    sw.Flush();
                    // Đóng file.
                    sw.Close();
                }
                return true;
            }
            catch (Exception _ex)
            {
                Console.WriteLine("Ghi du lieu ra file text: " + _ex.Message);
                return false;
            }
            finally
            {
                fs.Close();     // Close the stream
            }
        }

        /// <summary>
        /// Hàm thực hiện đọc dữ liệu trong file và trả về một bảng gồm các phần tử là nội dung từng dòng trong file đó
        /// </summary>
        /// <param name="sPathText">Đường dẫn file cần lấy thông tin bind vào mảng</param>
        /// <returns>
        /// Mảng chứa các phần tử là nội dung từng dòng trong file
        /// Ex: System.Collections.Generic.List<string> lines = GetListOfLinesFromFile(@"C:\MDS\DATA\TEXTFILE\MIG_CHECK_SUM_0000.TXT");
        /// </returns>
        public static System.Collections.Generic.List<string> GetListOfLinesFromFile(string sPathText)
        {
            try
            {
                System.Collections.Generic.List<string> lines = new System.Collections.Generic.List<string>();
                System.IO.StreamReader reader = new System.IO.StreamReader(sPathText);
                string currentLine = string.Empty;
                while ((currentLine = reader.ReadLine()) != null)
                {
                    lines.Add(currentLine);
                }
                reader.Close();
                return lines;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hàm đọc chuyển file từ đường dẫn đầy đủ thành dạng bytes
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)
            FileStream fs = null;
            try
            {
                fs = System.IO.File.OpenRead(fullFilePath);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        public static string[] GetLists_Files(string pPathFile, string pMasking)
        {
            string[] listFiles = null;
            try
            {
                listFiles = Directory.GetFiles(pPathFile, pMasking);
            }
            catch
            {
                listFiles = new string[] { string.Empty };
            }
            return listFiles;
        }

    }
}
