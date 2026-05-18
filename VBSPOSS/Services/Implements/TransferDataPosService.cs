using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class TransferDataPosService : ITransferDataPosService
    {
        /// <summary>
        /// Defines the _dbContext.
        /// </summary>
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Kết nối CSDL Oracle của iDC
        /// </summary>
        private readonly IntellectIDCDbContext _dbContextIDC;

        /// <summary>
        /// Defines the _mapper.
        /// </summary>
        private readonly IMapper _mapper;

        private readonly ILogger<ListOfTransPointService> _logger;

        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfValueService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        public TransferDataPosService(ApplicationDbContext dbContext, IConfiguration config, IMapper mapper, IntellectIDCDbContext dbContextIDC, ILogger<ListOfTransPointService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _dbContextIDC = dbContextIDC;
            _logger = logger;
            _config = config;
        }



        /// <summary>
        /// Hàm lấy danh sách điểm giao dịch để cho Lựa chọn thay đổi theo yêu cầu nghiệp vụ
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc). Nếu lấy cả chi nhánh thì truyền vào 4 ký tự đầu của POS Chi nhánh</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pTxnPointName">Tên điểm gioa dịch</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào -1</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pEventCode">Tìm kiếm theo bản ghi có yêu cầu nghiệp vụ với điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách điểm giao dịch theo Model ListOfTransPointWorkViewModel</returns>
        public List<TransferDataPosMasterViewModel> GetListOfTranferDataPosSearch(string pProvinceCode, string pPosCode, string pTxnPointCode, string pTxnPointName,
                                int pStatus, string pTxnLocation, string pEventCode, string pUserGrade)
        {
            var transferDataPosMasterViews = new List<TransferDataPosMasterViewModel>();

            try
            {
                int orderNo = 0;
                var query = _dbContext.TransferDataPosMasters.AsQueryable();

                if (pUserGrade == "3")
                {
                    query = query.Where(w =>
                        w.IsDeleted == false
                        &&
                        (
                            w.Status == 2 || w.Status == 3 || w.Status == 4 || w.Status == 1
                        )
                        && w.MainPos.StartsWith(pPosCode));
                }
                else if (pUserGrade == "2")
                {
                    query = query.Where(w =>
                        w.IsDeleted == false
                        && w.MainPos.StartsWith(pPosCode));
                }
                else
                {
                    query = query.Where(w =>
                        w.IsDeleted == false);
                }

                var listOfTransferDataPosMaster =
                    query
                        .OrderByDescending(o => o.CreatedDate)
                        .ToList();

                transferDataPosMasterViews = listOfTransferDataPosMaster
                    .Select(x => new TransferDataPosMasterViewModel
                    {
                        OrderNo = ++orderNo,
                        Id = x.Id,
                        FromPosCode = x.FromPosCode,
                        FromPosName = GetPosName(x.FromPosCode),
                        ToPosCode = x.ToPosCode,
                        ToPosName = GetPosName(x.ToPosCode),
                        EffectiveDate = x.EffectiveDate,
                        Remark = x.Remark,
                        Status = x.Status,
                        TotalVillage = x.TotalVillage,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        ModifiedBy = x.ModifiedBy,
                        ModifiedDate = x.ModifiedDate,
                        ApproverBy = x.ApproverBy,
                        ApprovalDate = x.ApprovalDate,
                        RejectReason = x.RejectReason,
                        IsDeleted = x.IsDeleted
                    })
                    .ToList();

                return transferDataPosMasterViews;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetPosName(string posCode)
        {
            try
            {
                if (string.IsNullOrEmpty(posCode))
                {
                    return string.Empty;
                }

                var answer = _dbContext.ListOfPoss
                    .FirstOrDefault(w => w.Code == posCode);

                if (answer == null)
                {
                    return string.Empty;
                }

                return answer.Name;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public List<ValueConstModel> GetListPosOfBranch(string mainPos)
        {
            try
            {
                var answer = _dbContext.ListOfPoss
                .Where(w => w.MainPosCode == mainPos && w.Status == "O")
                .OrderBy(x => x.Code)
                .Select(x => new ValueConstModel
                {
                    Code = x.Code,
                    Description = x.Code + " - " + x.Name
                })
                .ToList();

                return answer;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<long> SaveTranferPosMaster(
    TransferDataPosMasterViewModel tranferMaster,
    string pUserNameUpd,
    string pFlagCall,
    string pButtonType, string mainPos)
        {
            int iSaveChanges = 0;

            long iRetIdUpd = 0;

            DateTime dCurrentDateTmp = DateTime.Now;

            try
            {
                if (tranferMaster == null)
                {
                    return 0;
                }

                // INSERT
                if (tranferMaster.Id <= 0)
                {
                    var entity = new TransferDataPosMaster
                    {
                        MainPos = mainPos,
                        FromPosCode = tranferMaster.FromPosCode,
                        ToPosCode = tranferMaster.ToPosCode,

                        EffectiveDate = tranferMaster.EffectiveDate,

                        Remark = tranferMaster.Remark,

                        Status = TranferDataPosStatus.INIT.Value,

                        CreatedBy = pUserNameUpd,
                        CreatedDate = dCurrentDateTmp,

                        ModifiedBy = pUserNameUpd,
                        ModifiedDate = dCurrentDateTmp
                    };

                    await _dbContext.TransferDataPosMasters.AddAsync(entity);

                    iSaveChanges = await _dbContext.SaveChangesAsync();

                    if (iSaveChanges > 0)
                    {
                        iRetIdUpd = entity.Id;
                    }
                }
                else
                {
                    // UPDATE
                    var entityUpd = await _dbContext.TransferDataPosMasters
                        .FirstOrDefaultAsync(x => x.Id == tranferMaster.Id);

                    if (entityUpd == null)
                    {
                        return 0;
                    }

                    entityUpd.FromPosCode = tranferMaster.FromPosCode;
                    entityUpd.ToPosCode = tranferMaster.ToPosCode;

                    entityUpd.EffectiveDate = tranferMaster.EffectiveDate;

                    entityUpd.Remark = tranferMaster.Remark;

                    entityUpd.ModifiedBy = pUserNameUpd;
                    entityUpd.ModifiedDate = dCurrentDateTmp;

                    _dbContext.TransferDataPosMasters.Update(entityUpd);

                    iSaveChanges = await _dbContext.SaveChangesAsync();

                    if (iSaveChanges > 0)
                    {
                        iRetIdUpd = entityUpd.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return iRetIdUpd;
        }

        public List<ValueConstModel> GetListSubCommuneOfPos(string posCd)
        {
            try
            {
                var answer = _dbContext.ListOfCommunes
                .Where(w => w.PosCode == posCd && w.RecordStatus == "A")
                .OrderBy(x => x.SubCommuneCode)
                .Select(x => new ValueConstModel
                {
                    Code = x.SubCommuneCode,
                    Description = x.CommuneCode + " - " + x.CommuneName + " -> " + x.SubCommuneCode + " - " + x.SubCommuneName
                })
                .ToList();

                return answer;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool CheckExistTransferDataPosMaster(
        string fromPosCode,
        string toPosCode,
        DateTime? effectiveDate)
        {
            try
            {
                if (string.IsNullOrEmpty(fromPosCode)
                    || string.IsNullOrEmpty(toPosCode)
                    || effectiveDate == null)
                {
                    return false;
                }

                bool isExist = _dbContext.TransferDataPosMasters.Any(x =>

                    x.FromPosCode == fromPosCode
                    && x.ToPosCode == toPosCode

                    && x.EffectiveDate.HasValue
                    && x.EffectiveDate.Value.Date
                        == effectiveDate.Value.Date
                );

                return isExist;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<long> SaveAttachedFile(
    long documentId,
    IFormFile file,
    string documentNumber,
    string userName)
        {
            long fileId = 0;

            try
            {
                if (file == null || file.Length <= 0)
                {
                    return 0;
                }

                // =========================
                // ROOT FOLDER
                // =========================
                string uploadFolder =
                    _config["AttachedFileSettings:FolderUpload"];

                string rootFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    uploadFolder);

                // =========================
                // SUB FOLDER
                // =========================
                string subFolder = "TranferDataPos";

                string fullFolder = Path.Combine(
                    rootFolder,
                    subFolder);

                // tạo folder nếu chưa có
                if (!Directory.Exists(fullFolder))
                {
                    Directory.CreateDirectory(fullFolder);
                }

                // =========================
                // FILE INFO
                // =========================
                string fileExtension =
                    Path.GetExtension(file.FileName);

                string fileNameOrigin =
                    Path.GetFileName(file.FileName);

                // tên file tạm
                string fileNameNew =
                    Guid.NewGuid().ToString("N")
                    + fileExtension;

                // full path
                string fullPath = Path.Combine(
                    fullFolder,
                    fileNameNew);

                // =========================
                // SAVE FILE PHYSICAL
                // =========================
                using (var stream = new FileStream(
                    fullPath,
                    FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // =========================
                // SAVE DB
                // =========================
                var entity = new AttachedFileInfo
                {
                    DocumentId = documentId,

                    FileType = "8",

                    FileName = fileNameOrigin,

                    FileExtension = fileExtension,

                    PathFile = Path.Combine(
                        uploadFolder,
                        subFolder),

                    FileNameNew = fileNameNew,

                    DocumentNumber = documentNumber,

                    Status = 1,

                    CreatedBy = userName,

                    CreatedDate = DateTime.Now
                };

                await _dbContext.AttachedFileInfos
                    .AddAsync(entity);

                await _dbContext.SaveChangesAsync();

                fileId = entity.FileId;
            }
            catch (Exception)
            {
                throw;
            }

            return fileId;
        }

        public async Task<AttachedFileInfo> DownloadTransferAttachFile(
    long documentId)
        {
            try
            {
                return await _dbContext.AttachedFileInfos

                    .Where(x =>

                        x.DocumentId == documentId
                        && x.FileType == "8"
                        && x.Status == 1)

                    .OrderByDescending(x => x.FileId)

                    .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteTransferDataPos(
    long pId,
    string userName)
        {
            try
            {
                var entity =
                    await _dbContext.TransferDataPosMasters
                        .FirstOrDefaultAsync(x =>
                            x.Id == pId);

                if (entity == null)
                {
                    return false;
                }

                entity.IsDeleted = true;

                entity.ModifiedBy = userName;

                entity.ModifiedDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TransferDataPosMaster GetTransferDataPosMasterById(long pId)
        {
            try
            {
                return _dbContext.TransferDataPosMasters
                    .FirstOrDefault(x =>
                        x.Id == pId
                        && x.IsDeleted == false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> ApproveTransferDataPos(long pId, string pRemark, string pAction, string pUserName)
        {
            try
            {
                var entity =
                    await _dbContext
                        .TransferDataPosMasters
                        .FirstOrDefaultAsync(x =>
                            x.Id == pId);

                if (entity == null)
                {
                    return 0;
                }

                // =========================
                // PHÊ DUYỆT
                // =========================
                if (pAction == "APPROVE")
                {
                    entity.Status = 2;
                }

                // =========================
                // TỪ CHỐI
                // =========================
                else if (pAction == "REJECT")
                {
                    if (string.IsNullOrWhiteSpace(
                        pRemark))
                    {
                        return 2;
                    }

                    entity.Status = 3;
                }

                entity.Remark = pRemark;

                entity.ModifiedBy =
                    pUserName;

                entity.ModifiedDate =
                    DateTime.Now;

                await _dbContext
                    .SaveChangesAsync();

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ApproveTransferDataPos");

                return 0;
            }
        }
    }
}
