using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Helpers;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class AttachedFileService : IAttachedFileService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<InterestRateConfigureService> _logger;
        ILogger<InterestRateConfigureService> logger;
        private readonly IMapper _mapper;


        public AttachedFileService(IMapper mapper, ApplicationDbContext context, IConfiguration config)
        {
            _dbContext = context;
            _config = config;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<List<AttachedFileInfoView>> GetttachedFileSync(string pPosCode, string pFileType,
            string pFromTranDateFind, string pToTranDateFind, string pFileName)
        {
            try
            {
                // ===== TYPE 6 : đọc folder =====
                if (pFileType == "6")
                {
                    var list = new List<AttachedFileInfoView>();
                    string folderPath = _config["AttachedFileSettings:FolderTXN"];

                    if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                        return list;

                    //var files = Directory.GetFiles(folderPath, "TXN*.*")
                    //                     .OrderByDescending(f => new FileInfo(f).LastWriteTime);

                    var files = Directory.GetFiles(
                        folderPath,
                        "TXN*.*",
                        SearchOption.AllDirectories 
                    )
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime);

                    int stt = 1;

                    foreach (var file in files)
                    {
                        var fi = new FileInfo(file);

                        string baseName = fi.Name.Split('.')[0];
                        string txnPoint = baseName.Substring(0, baseName.Length - 8);
                        var point = await _dbContext.ListOfTransPoints
                           .FirstOrDefaultAsync(x => x.TxnPointCode == txnPoint);
                        string transDate = "";
                        if (baseName.Length >= 8)
                        {
                            string datePart = baseName.Substring(baseName.Length - 8);
                            

                            if (DateTime.TryParseExact(
                                datePart,
                                "yyyyMMdd",
                                null,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime d))
                            {
                                transDate = d.ToString("dd/MM/yyyy");
                            }

                             
                        }
                       


                        list.Add(new AttachedFileInfoView
                        {
                            FileId =0,
                            Orderby = stt++,
                            PosCode = point?.PosCode,
                            PosName = point?.PosName,

                            FileName = fi.Name,
                            SizeKB = Math.Round(fi.Length / 1024m / 1024m, 2),

                            TransactionDate = transDate,
                            ExportDate = fi.LastWriteTime.ToString("dd/MM/yyyy"),
                            ExportTime = fi.LastWriteTime.ToString("HH:mm:ss"),

                            DownloadCount = 1,
                            CreatedDate = fi.CreationTime,
                            TxnPointCode = point.TxnPointCode,
                            TxnPointName = point.TxnPointName,
                            ProvinceCode = point.ProvinceCode,
                            EffectiveDate = point.EffectiveDate,
                            DownloadUrl = "/AttachedFile/DownloadFile?fileName=" + fi.Name
                        });
                    }
                    DateTime fromDate = DateTime.ParseExact(
                    pFromTranDateFind,
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);

                    DateTime toDate = DateTime.ParseExact(
                        pToTranDateFind,
                        "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture);


                    if (pPosCode == "000100")
                    {
                        var filteredAll = list
                            .Where(x =>
                            {
                                DateTime tranDate;

                                return DateTime.TryParseExact(
                                           x.TransactionDate,
                                           "dd/MM/yyyy",
                                           System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.None,
                                           out tranDate)

                                       && tranDate >= fromDate
                                       && tranDate <= toDate

                                       && (string.IsNullOrEmpty(pFileName)
                                           || (!string.IsNullOrEmpty(x.FileName) &&
                                               x.FileName.ToUpper().Contains(pFileName.ToUpper())));
                            })
                            .ToList();
                        return filteredAll;
                    }
                    //var posNew = _dbContext.ListOfPoss.FirstOrDefaultAsync(x => x.Code == pPosCode);
                    //string provinceCode = posNew.Result.Code.Substring(2, 2); // "05"
                    var filtered = list
                    .Where(x =>
                    {
                        DateTime tranDate;

                        return x.PosCode == pPosCode

                            && DateTime.TryParseExact(
                                x.TransactionDate,
                                "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out tranDate)

                            && tranDate >= fromDate
                            && tranDate <= toDate

                            && (string.IsNullOrEmpty(pFileName)
                                || (!string.IsNullOrEmpty(x.FileName) &&
                                    x.FileName.ToUpper().Contains(pFileName.ToUpper())));
                    })
                    .ToList();
                    return filtered;
                }
                else
                {
                    // ===== TYPE KHÁC : đọc DB =====
                    var query = _dbContext.AttachedFileInfos.AsQueryable();

                    if (!string.IsNullOrEmpty(pFileType) && pFileType != "-1")
                    {
                        query = query.Where(x => x.FileType == pFileType);
                    }

                    if (!string.IsNullOrEmpty(pFileName))
                    {
                        query = query.Where(x =>
                            x.FileName != null &&
                            x.FileName.ToLower().Contains(pFileName.ToLower()));
                    }

                    var data = await query
                        .OrderByDescending(x => x.CreatedDate)
                        .ToListAsync();

                    int order = 1;

                    var result = data.Select(x => new AttachedFileInfoView
                    {
                        FileId = x.FileId,
                        Orderby = order++,
                        FileName = x.FileName,
                        SizeKB = 0,
                        ContentDescription = x.ContentDescription,
                        CreatedDate = x.CreatedDate,
                        DocumentNumber = x.DocumentNumber,
                        DownloadCount = 1,
                        DownloadUrl = "/AttachedFile/DownloadFile?fileName=" + x.FileNameNew
                    }).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetttachedFileSync error");
                return new List<AttachedFileInfoView>();
            }
        }

        public async Task<string> UploadFileAsync(
    IFormFile file,
    string description,
    string createdBy,
    string valueFileType,
    string DocumentNumber)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return "2";

                if (file.Length > 50 * 1024 * 1024)
                    return "3";

                // =========================
                // ROOT FOLDER
                // =========================
                string uploadFolder = _config["AttachedFileSettings:FolderUpload"];

                string rootFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    uploadFolder);

                // =========================
                // SUB FOLDER theo FileType
                // =========================
                string subFolder = "";

                switch (valueFileType)
                {
                    case "5":
                        subFolder = "ExeFile";
                        break;

                    case "7":
                        subFolder = "OtherFile";
                        break;
                }

                // folder cuối cùng
                string finalFolder = string.IsNullOrEmpty(subFolder)
                    ? rootFolder
                    : Path.Combine(rootFolder, subFolder);

                if (!Directory.Exists(finalFolder))
                    Directory.CreateDirectory(finalFolder);

                // =========================
                // FILE INFO
                // =========================
                string originalFileName = Path.GetFileName(file.FileName);
                string extension = Path.GetExtension(file.FileName);

                // tạo record trước để lấy FileId
                var entity = new AttachedFileInfo
                {
                    DocumentId = 0,
                    FileType = valueFileType,
                    FileName = originalFileName,
                    FileExtension = extension,
                    PathFile = "",
                    FileNameNew = "",
                    ContentDescription = description,
                    Status = 1,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now,
                    DocumentNumber = DocumentNumber
                };

                _dbContext.AttachedFileInfos.Add(entity);
                await _dbContext.SaveChangesAsync();

                // =========================
                // BUILD FILE NAME
                // =========================
                string vv = valueFileType == "1" ? "IntRate" : "User";

                string year = DateTime.Now.Year.ToString();

                string seq = entity.FileId.ToString().PadLeft(10, '0');

                string fileNameNew = $"{vv}_{valueFileType}_{year}_{seq}{extension}";

                // =========================
                // SAVE FILE
                // =========================
                string fullPath = Path.Combine(finalFolder, fileNameNew);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // =========================
                // SAVE PATH (relative)
                // =========================
                string relativePath = string.IsNullOrEmpty(subFolder)
                    ? uploadFolder
                    : Path.Combine(uploadFolder, subFolder);

                // update lại record
                entity.FileNameNew = fileNameNew;
                entity.PathFile = relativePath;
                entity.ModifiedDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return "0";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload file error");
                return "1";
            }
        }

        public DownloadFileResult DownloadFile(long fileId, string fileName)
        {
            try
            {
                string fullPath = string.Empty;
                string downloadName = fileName;

                // =========================
                // FILE từ DB (upload)
                // =========================
                if (fileId > 0)
                {
                    var file = _dbContext.AttachedFileInfos
                        .FirstOrDefault(x => x.FileId == fileId);

                    if (file == null)
                        return null;

                    string rootFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot");

                    // 🔥 QUAN TRỌNG: dùng PathFile (có thể là UploadFiles\OtherFile)
                    fullPath = Path.Combine(
                        rootFolder,
                        file.PathFile ?? "",
                        file.FileNameNew ?? "");

                    downloadName = file.FileName;
                }
                else
                {
                    // =========================
                    // FILE TXN (fileType = 6)
                    // =========================
                    string folderPath = _config["AttachedFileSettings:FolderTXN"];

                    if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                        return null;

                    //  tìm file trong toàn bộ subfolder
                    var filePath = Directory.GetFiles(
                            folderPath,
                            fileName,
                            SearchOption.AllDirectories)
                        .FirstOrDefault();

                    if (string.IsNullOrEmpty(filePath))
                        return null;

                    fullPath = filePath;
                    downloadName = Path.GetFileName(filePath);
                }

                // =========================
                // CHECK FILE
                // =========================
                if (!System.IO.File.Exists(fullPath))
                    return null;

                var stream = new FileStream(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

                return new DownloadFileResult
                {
                    Stream = stream,
                    FileName = downloadName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "DownloadFile error: fileId={fileId}, fileName={fileName}",
                    fileId, fileName);

                return null;
            }
        }

        /// <summary>
        /// Hàm sinh ra tên file mới khi upload lên máy chủ
        /// </summary>
        /// <param name="pFileId">Chỉ số Id của bản ghi trong bảng AttachedFileInfo</param>
        /// <param name="pFileType">Loại file FileType.<>.Value</param>
        /// <param name="pProductGroupCode">Loại cấu hình lãi suất (Không bắt buộc) - Áp dụng cho file cấu hình lãi suất</param>
        /// <param name="pAttachDate">Ngày hiệu lực/Upload</param>
        /// <param name="pTypeChild">Mã phụ nếu muốn đưa vào tên file</param>
        /// <returns>Tên file mới tự sinh theo quy ước</returns>
        private string GetFileNameNewUpload(long pFileId, string pFileType, string pProductGroupCode, DateTime pAttachDate, string pTypeChild)
        {
            string sFileNameNew = "";
            long iFileIdTemp = 0;
            if (pFileId == 0)
            {
                var listAttachedFileTemp = _dbContext.AttachedFileInfos.OrderByDescending(o => o.FileId).ToList();
                if (listAttachedFileTemp != null && listAttachedFileTemp.Count != 0)
                    iFileIdTemp = listAttachedFileTemp.FirstOrDefault().FileId;
                iFileIdTemp++;
            }
            else iFileIdTemp = pFileId;
            //1 - File cấu hình lãi suất Tide / Casa / DepositPenal;
            if (pFileType == FileType.FileType_ConfigIntRate.Value.ToString())
            {
                if (pProductGroupCode == ProductGroupCode.TIDE.Code)
                    sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Tide_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
                else if (pProductGroupCode == ProductGroupCode.CASA.Code)
                    sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Casa_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
                else if (pProductGroupCode == ProductGroupCode.DEPOSITPENAL.Code)
                    sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Penal_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
                else sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Unknown_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
            }
            else if (pFileType == FileType.FileType_User_IDC.Value.ToString())
            {
                if (string.IsNullOrEmpty(pTypeChild))
                    sFileNameNew = $"{FileType.FileType_User_IDC.Code}_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
                else
                    sFileNameNew = $"{FileType.FileType_User_IDC.Code}_{pTypeChild}_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
            }
            return sFileNameNew;
        }

        /// <summary>
        /// Hàm cập nhật thông tin bảng dữ liệu AttachedFileInfo (Tệp tin đính kèm văn bản/tài liệu/quyết định cấu hình lãi suất)
        /// </summary>
        /// <param name="pDocumentId">Chỉ số xác định băn bản/tài liệu/quyết định có file đính kèm</param>
        /// <param name="pListAttachedFiles">Danh sách file đính kèm</param>
        /// <param name="pFileType">Phân loại:  1 - File cấu hình lãi suất Tide/Casa/DepositPenal;
        ///                                     2 - File đính kèm của người dùng iDC;</param>
        /// <param name="pProductGroupCode">Phân nhóm sản phẩm Tide hay Casa. Giá trị: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pUserNameUpd">Người thực hiện</param>
        /// <param name="pTypeChild">Mã phụ nếu muốn đưa vào tên file/param>
        /// <returns>Chuỗi FileId được thêm mới hoặc cập nhật chỉnh sửa</returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<long>> SaveAttachedFileInfo(long pDocumentId, List<AttachedFileInfo> pListAttachedFiles, string pFileType, string pProductGroupCode, 
                    string pUserNameUpd, string pTypeChild)
        {
            int iCountDocumentId = 0;
            long iDocumentIdTemp = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            if (pListAttachedFiles == null || !pListAttachedFiles.Any())
                return null;
            if (pListAttachedFiles == null || pListAttachedFiles.Count <= 0)
                return null;
            try
            {
                List<long> listIdResult = new List<long>();
                List<AttachedFileInfo> listAttachedFilesAddNew = new List<AttachedFileInfo>();
                List<AttachedFileInfo> listAttachedFilesUpdate = new List<AttachedFileInfo>();
                var listAttachedFileTemp = await _dbContext.AttachedFileInfos.OrderByDescending(o => o.FileId).ToListAsync();
                if (listAttachedFileTemp != null && listAttachedFileTemp.Count != 0)
                {
                    iDocumentIdTemp = listAttachedFileTemp.FirstOrDefault().FileId;
                    var listAttachedFileTemp01 = listAttachedFileTemp.Where(w => w.DocumentId == iDocumentIdTemp).OrderByDescending(o => o.DocumentId).ToList();
                    if (listAttachedFileTemp01 != null && listAttachedFileTemp01.Count != 0)
                    {
                        iDocumentIdTemp = listAttachedFileTemp01.FirstOrDefault().DocumentId;
                        iDocumentIdTemp++;
                    }
                }
                else iDocumentIdTemp++;
                iCountDocumentId = pListAttachedFiles.Where(w => w.DocumentId != 0).Count();
                var listFileOld = await _dbContext.AttachedFileInfos.Where(x => pListAttachedFiles.Select(s => s.FileId).Contains(x.FileId)).ToListAsync();
                foreach (var itemAttachedFile in pListAttachedFiles)
                {
                    if (itemAttachedFile.FileId != 0)
                    {
                        var objAttachedFileUpd = await _dbContext.AttachedFileInfos.FindAsync(itemAttachedFile.FileId);
                        if (objAttachedFileUpd != null && objAttachedFileUpd.FileId > 0)
                        {
                            _mapper.Map(itemAttachedFile, objAttachedFileUpd);
                            if (iCountDocumentId <= 0)
                                objAttachedFileUpd.DocumentId = iDocumentIdTemp;
                            objAttachedFileUpd.DocumentId = (itemAttachedFile.DocumentId <= 0) ? iDocumentIdTemp : itemAttachedFile.DocumentId;
                            objAttachedFileUpd.FileType = itemAttachedFile.FileType;
                            objAttachedFileUpd.FileName = itemAttachedFile.FileName;
                            objAttachedFileUpd.FileExtension = itemAttachedFile.FileExtension;
                            objAttachedFileUpd.PathFile = itemAttachedFile.PathFile;
                            objAttachedFileUpd.FileNameNew = GetFileNameNewUpload(itemAttachedFile.FileId, itemAttachedFile.FileType, pProductGroupCode, dCurrentDateTmp, pTypeChild) + $"{itemAttachedFile.FileExtension}";
                            objAttachedFileUpd.DocumentNumber = itemAttachedFile.DocumentNumber;
                            objAttachedFileUpd.CircularRefNum = itemAttachedFile.CircularRefNum;
                            objAttachedFileUpd.Status = StatusTrans.Status_Modified.Value;
                            objAttachedFileUpd.ModifiedBy = pUserNameUpd ?? "UnknownUser";
                            objAttachedFileUpd.ModifiedDate = dCurrentDateTmp;
                            listAttachedFilesUpdate.Add(objAttachedFileUpd);
                        }
                    }
                    else if (itemAttachedFile.FileId == 0)
                    {
                        if (iCountDocumentId <= 0)
                            itemAttachedFile.DocumentId = iDocumentIdTemp;
                        itemAttachedFile.Status = StatusTrans.Status_Created.Value;
                        itemAttachedFile.CreatedBy = pUserNameUpd;
                        itemAttachedFile.CreatedDate = dCurrentDateTmp;
                        itemAttachedFile.ModifiedBy = pUserNameUpd;
                        itemAttachedFile.ModifiedDate = dCurrentDateTmp;
                        itemAttachedFile.ApproverBy = pUserNameUpd;
                        itemAttachedFile.ApprovalDate = dCurrentDateTmp;
                        listAttachedFilesAddNew.Add(itemAttachedFile);
                    }
                }
                if (listAttachedFilesUpdate != null && listAttachedFilesUpdate.Count != 0)
                {
                    _dbContext.AttachedFileInfos.UpdateRange(listAttachedFilesUpdate);
                    int iSaveChanges = await _dbContext.SaveChangesAsync();
                    if (iSaveChanges > 0)
                        listIdResult.AddRange(listAttachedFilesUpdate.Select(s => s.FileId).ToList());
                }
                if (listAttachedFilesAddNew != null && listAttachedFilesAddNew.Count != 0)
                {
                    _dbContext.AttachedFileInfos.AddRange(listAttachedFilesAddNew);
                    int iSaveChanges = await _dbContext.SaveChangesAsync();
                    if (iSaveChanges > 0)
                        listIdResult.AddRange(listAttachedFilesAddNew.Select(s => s.FileId).ToList());
                }

                //Cập nhật lại tên file mới
                if (listIdResult != null && listIdResult.Count != 0)
                {
                    List<AttachedFileInfo> listAttachFileUpdFileName = new List<AttachedFileInfo>();
                    foreach (var itemUpd in listIdResult)
                    {
                        var objAttachedFileUpdFileName = _dbContext.AttachedFileInfos.Where(w => w.FileId == itemUpd).FirstOrDefault();
                        if (objAttachedFileUpdFileName != null && objAttachedFileUpdFileName.FileId != 0)
                        {
                            objAttachedFileUpdFileName.FileNameNew = GetFileNameNewUpload(objAttachedFileUpdFileName.FileId, "1", pProductGroupCode, dCurrentDateTmp, pTypeChild);
                            if (!objAttachedFileUpdFileName.FileNameNew.Contains(objAttachedFileUpdFileName.FileExtension))
                            {
                                objAttachedFileUpdFileName.FileNameNew = $"{objAttachedFileUpdFileName.FileNameNew}{objAttachedFileUpdFileName.FileExtension}";
                            }
                            listAttachFileUpdFileName.Add(objAttachedFileUpdFileName);
                        }
                    }
                    if (listAttachFileUpdFileName != null && listAttachFileUpdFileName.Count != 0)
                    {
                        _dbContext.AttachedFileInfos.UpdateRange(listAttachFileUpdFileName);
                        int iSaveChanges = await _dbContext.SaveChangesAsync();
                    }
                }
                //Xóa file dữ liệu đã upload trước đó nếu có trên server
                int iCountFileDelete = 0;
                if (listFileOld != null && listFileOld.Count != 0)
                {
                    foreach (var itemOld in listFileOld)
                    {
                        bool bIsDeleteFile = false;
                        if (itemOld.PathFile.Contains(itemOld.FileExtension))
                            bIsDeleteFile = FilesUtils.Delete_File("", itemOld.PathFile);
                        else
                            bIsDeleteFile = FilesUtils.Delete_File(itemOld.FileNameNew, itemOld.PathFile);

                        if (bIsDeleteFile)
                            iCountFileDelete++;
                    }
                }
                return listIdResult;
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? "Không có inner exception";
                Console.WriteLine($"SaveAttachedFileInfo({pDocumentId.ToString()},'{pListAttachedFiles.FirstOrDefault().CircularRefNum}', '{pFileType}', '{pProductGroupCode}', '{pUserNameUpd}') => Error: {innerException}\n{ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? "Không có inner exception";
                Console.WriteLine($"SaveAttachedFileInfo({pDocumentId.ToString()},'{pListAttachedFiles.FirstOrDefault().CircularRefNum}', '{pFileType}', '{pProductGroupCode}', '{pUserNameUpd}') => Error: {innerException}\n{ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật file đính kèm " +
                            $"SaveAttachedFileInfo({pDocumentId.ToString()},'{pListAttachedFiles.FirstOrDefault().CircularRefNum}', '{pFileType}', '{pProductGroupCode}', '{pUserNameUpd}') => Error: {ex.Message}", ex);
                throw;
            }
        }

    }
}
