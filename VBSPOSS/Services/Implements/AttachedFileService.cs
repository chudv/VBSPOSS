using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Helpers;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class AttachedFileService : IAttachedFileService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<InterestRateConfigureService> _logger;
        ILogger<InterestRateConfigureService> logger;

        public AttachedFileService(ApplicationDbContext context, IConfiguration config)
        {
            _dbContext = context;
            _config = config;
            _logger = logger;
        }
        public async Task<List<AttachedFileInfoView>> GetttachedFileSync(string pPosCode, string pFileType, string pTranDate_Find, string pFileName)
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
                    if(pPosCode == "000100")
                    {
                        var filteredAll = list
                            .Where(x =>
                            x.TransactionDate == pTranDate_Find
                            && (string.IsNullOrEmpty(pFileName)
                                || (!string.IsNullOrEmpty(x.FileName) &&
                                    x.FileName.ToUpper().Contains(pFileName.ToUpper())))
                        )
                        .ToList();
                        return filteredAll;
                    }    

                    string provinceCode = pPosCode.Substring(2, 2); // "05"
                    DateTime tranDate;
                    var filtered = list
                    .Where(x =>
                        x.ProvinceCode == provinceCode
                        && x.TransactionDate == pTranDate_Find
                        && (string.IsNullOrEmpty(pFileName)
                            || (!string.IsNullOrEmpty(x.FileName) &&
                                x.FileName.ToUpper().Contains(pFileName.ToUpper())))
                    )
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



        
    }
}
