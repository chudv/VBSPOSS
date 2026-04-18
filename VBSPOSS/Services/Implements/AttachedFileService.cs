using Microsoft.EntityFrameworkCore;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
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
        public async Task<List<AttachedFileInfoView>> GetttachedFileSync(string pPosCode, string pFileType)
        {
            try
            {
                // ===== TYPE 6 : đọc folder =====
                if (pFileType == "6")
                {
                    var list = new List<AttachedFileInfoView>();
                    string folderPath = _config["AttachedFileSettings:Folder"];

                    if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                        return list;

                    var files = Directory.GetFiles(folderPath, "TXN*.*")
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

                            DownloadUrl = "/AttachedFile/DownloadFile?fileName=" + fi.Name
                        });
                    }

                    return list;
                }
                else
                {
                    // ===== TYPE KHÁC : đọc DB =====
                    var query = _dbContext.AttachedFileInfos.AsQueryable();

                    if (!string.IsNullOrEmpty(pFileType) && pFileType != "-1")
                    {
                        query = query.Where(x => x.FileType == pFileType);
                    }

                    var data = await query
                        .OrderByDescending(x => x.CreatedDate)
                        .ToListAsync();

                    int order = 1;

                    var result = data.Select(x => new AttachedFileInfoView
                    {
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

        public async Task<string> UploadFileAsync(IFormFile file, string description, string createdBy)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return "2"; // chưa chọn file

                if (file.Length > 4 * 1024 * 1024)
                    return "3"; // vượt 4MB

                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UploadFiles");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Path.GetFileName(file.FileName);
                string extension = Path.GetExtension(file.FileName);
                string newFileName = Guid.NewGuid().ToString() + extension;
                string fullPath = Path.Combine(folder, newFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachedFile = new AttachedFileInfo
                {
                    DocumentId = 0, // gán theo logic của bạn
                    FileType = file.ContentType,
                    FileName = fileName,
                    FileExtension = extension,
                    PathFile = fullPath,
                    FileNameNew = newFileName,
                    ContentDescription = description,
                    Status = 1,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now
                };

                _dbContext.AttachedFileInfos.Add(attachedFile);
                await _dbContext.SaveChangesAsync();

                return "0"; // success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload file error");
                return "1"; // lỗi
            }
        }
    }
}
