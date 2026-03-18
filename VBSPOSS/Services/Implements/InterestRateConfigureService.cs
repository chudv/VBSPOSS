using AutoMapper;
using Kendo.Mvc.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;  // THÊM: Cho File operations nếu cần
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Controllers;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Helpers;
using VBSPOSS.Integration.Implements;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

namespace VBSPOSS.Services.Implements
{
    public class InterestRateConfigureService : IInterestRateConfigureService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IApiInternalEsbService _apiInternalEsbService;
        private readonly IProductService _productService;
        private readonly ILogger<InterestRateConfigureService> _logger;
        public InterestRateConfigureService(ApplicationDbContext context, IMapper mapper, IApiInternalEsbService apiInternalEsbService, IProductService productService, ILogger<InterestRateConfigureService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _apiInternalEsbService = apiInternalEsbService;
            _productService = productService;
            _logger = logger;

        }

        //public async Task<List<InterestRateConfigMasterModel>> GetInterestRateConfigMasterListAsync(string productGroupCode, string posCode, string productCode,
        //                        string circularRefNum, DateTime? fromEffectiveDate, DateTime? toEffectiveDate)
        //{
        //    try
        //    {
        //        var query = _context.InterestRateConfigMasters
        //            .Where(x => x.Status == 1);
        //        if (!string.IsNullOrEmpty(productGroupCode))
        //            query = query.Where(x => x.ProductGroupCode == productGroupCode);
        //        if (!string.IsNullOrEmpty(posCode))
        //            query = query.Where(x => x.PosCode == posCode);
        //        if (!string.IsNullOrEmpty(productCode))
        //            query = query.Where(x => x.ProductCode == productCode);
        //        if (!string.IsNullOrEmpty(circularRefNum))
        //            query = query.Where(x => x.CircularRefNum == circularRefNum);
        //        if (fromEffectiveDate.HasValue)
        //            query = query.Where(x => x.EffectiveDate >= fromEffectiveDate.Value);
        //        if (toEffectiveDate.HasValue)
        //            query = query.Where(x => x.EffectiveDate <= toEffectiveDate.Value);
        //        var result = await query
        //            .Select(x => new InterestRateConfigMasterModel
        //            {
        //                Id = x.Id,
        //                PosCode = x.PosCode ?? "0",
        //                PosName = x.PosName ?? "",
        //                ProductCode = x.ProductCode ?? "",
        //                ProductName = x.ProductName ?? "",
        //                AccountTypeCode = x.AccountTypeCode ?? "",
        //                AccountTypeName = x.AccountTypeName ?? "",
        //                AccountSubTypeCode = x.AccountSubTypeCode ?? "0",
        //                CircularRefNum = x.CircularRefNum ?? "",
        //                InterestRate = x.InterestRate,  // FIX: Gán trực tiếp nếu non-nullable
        //                NewInterestRate = x.NewInterestRate,  // FIX: Gán trực tiếp
        //                PenalRate = x.PenalRate,  // FIX: Gán trực tiếp
        //                EffectiveDate = x.EffectiveDate,
        //                ExpiryDate = x.ExpiryDate,
        //                DocumentId = x.DocumentId,
        //                Status = x.Status,
        //                CircularDate = x.CircularDate,
        //                AmountSlab = x.AmountSlab,
        //                TenorSerialNo = x.TenorSerialNo,
        //                IntRateType = x.IntRateType,
        //                SpreadRate = x.SpreadRate,
        //                Remark = x.Remark,
        //                OrtherNotes = x.OrtherNotes,
        //                StatusUpdateCore = x.StatusUpdateCore,
        //                CallApiTxnStatus = x.CallApiTxnStatus,
        //                CallApiReqRecordSl = x.CallApiReqRecordSl != null ? x.CallApiReqRecordSl.ToString() : null,
        //                CallApiResponseCode = x.CallApiResponseCode,
        //                CallApiResponseMsg = x.CallApiResponseMsg,
        //                CreatedBy = x.CreatedBy,
        //                CreatedDate = x.CreatedDate,
        //                ModifiedBy = x.ModifiedBy,
        //                ModifiedDate = x.ModifiedDate,
        //                ApproverBy = x.ApproverBy,
        //                ApprovalDate = x.ApprovalDate
        //            })
        //            .ToListAsync();
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        public async Task<List<InterestRateConfigMasterModel>> GetInterestRateConfigMasterListAsync(string productGroupCode, string posCode, string productCode,
                                string circularRefNum, DateTime? fromEffectiveDate, DateTime? toEffectiveDate)
        {
            try
            {
                var query = _dbContext.InterestRateConfigMasters
                    .Where(x => x.Status == 1);
                if (!string.IsNullOrEmpty(productGroupCode))
                    query = query.Where(x => x.ProductGroupCode == productGroupCode);
                if (!string.IsNullOrEmpty(posCode))
                    query = query.Where(x => x.PosCode == posCode);
                if (!string.IsNullOrEmpty(productCode))
                    query = query.Where(x => x.ProductCode == productCode);
                if (!string.IsNullOrEmpty(circularRefNum))
                {
                    // SỬA: Dùng Contains để match partial/trim space, log để debug
                    query = query.Where(x => x.CircularRefNum.Contains(circularRefNum.Trim()));
                    //  WriteLog(LogType.INFOR, $"Filtered by CircularRefNum.Contains('{circularRefNum.Trim()}')");
                }
                if (fromEffectiveDate.HasValue)
                    query = query.Where(x => x.EffectiveDate >= fromEffectiveDate.Value);
                if (toEffectiveDate.HasValue)
                    query = query.Where(x => x.EffectiveDate <= toEffectiveDate.Value);

                var result = await query
                    .Select(x => new InterestRateConfigMasterModel
                    {
                        Id = x.Id,
                        PosCode = x.PosCode ?? "0",
                        PosName = x.PosName ?? "",
                        ProductCode = x.ProductCode ?? "",
                        ProductName = x.ProductName ?? "",
                        AccountTypeCode = x.AccountTypeCode ?? "",
                        AccountTypeName = x.AccountTypeName ?? "",
                        AccountSubTypeCode = x.AccountSubTypeCode ?? "0",
                        CircularRefNum = x.CircularRefNum ?? "",
                        InterestRate = x.InterestRate,
                        NewInterestRate = x.NewInterestRate,
                        PenalRate = x.PenalRate,
                        EffectiveDate = x.EffectiveDate,
                        ExpiryDate = x.ExpiryDate,
                        DocumentId = x.DocumentId,
                        Status = x.Status,
                        CircularDate = x.CircularDate,
                        AmountSlab = x.AmountSlab,
                        TenorSerialNo = x.TenorSerialNo,
                        IntRateType = x.IntRateType,
                        SpreadRate = x.SpreadRate,
                        Remark = x.Remark,
                        OrtherNotes = x.OrtherNotes,
                        StatusUpdateCore = x.StatusUpdateCore,
                        CallApiTxnStatus = x.CallApiTxnStatus,
                        CallApiReqRecordSl = x.CallApiReqRecordSl != null ? x.CallApiReqRecordSl.ToString() : null,
                        CallApiResponseCode = x.CallApiResponseCode,
                        CallApiResponseMsg = x.CallApiResponseMsg,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        ModifiedBy = x.ModifiedBy,
                        ModifiedDate = x.ModifiedDate,
                        ApproverBy = x.ApproverBy,
                        ApprovalDate = x.ApprovalDate
                    })
                    .ToListAsync();

                // THÊM: Log số records để debug
                // WriteLog(LogType.INFOR, $"GetInterestRateConfigMasterListAsync returned {result.Count} records (circularRefNum='{circularRefNum}', posCode='{posCode}', fromDate={fromEffectiveDate})");

                return result;
            }
            catch (Exception ex)
            {
                //WriteLog(LogType.ERROR, $"Error in GetInterestRateConfigMasterListAsync: {ex.Message}");
                throw;
            }
        }

        // Method cho Index (từ View tổng hợp)
        //public async Task<List<InterestRateConfigMasterModel>> GetInterestRateConfigMasterViewListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, DateTime? fromDate, DateTime? toDate)
        //{
        //    try
        //    {
        //        var query = _dbContext.InterestRateConfigMasterViews
        //            .Where(x => x.Status == 1);
        //        if (!string.IsNullOrEmpty(productGroupCode))
        //            query = query.Where(x => x.ProductGroupCode == productGroupCode);
        //        if (!string.IsNullOrEmpty(posCode))
        //            query = query.Where(x => x.PosCode == posCode);
        //        if (!string.IsNullOrEmpty(productCode))
        //            query = query.Where(x => x.ProductCode == productCode);
        //        if (!string.IsNullOrEmpty(circularRefNum))
        //            query = query.Where(x => x.CircularRefNum == circularRefNum);
        //        if (fromDate.HasValue)
        //            query = query.Where(x => x.EffectiveDate >= fromDate.Value);
        //        if (toDate.HasValue)
        //            query = query.Where(x => x.EffectiveDate <= toDate.Value);
        //        var viewItems = await query.ToListAsync();
        //        var result = viewItems.Select(x => new InterestRateConfigMasterModel
        //        {
        //            Id = x.Id,
        //            PosCode = x.PosCode ?? "0",
        //            PosName = x.PosName ?? "",
        //            ProductCode = x.ProductCode ?? "",
        //            ProductName = x.ProductName ?? "",
        //            AccountTypeCode = x.AccountTypeCode ?? "",
        //            AccountTypeName = x.AccountTypeName ?? "",
        //            AccountSubTypeCode = x.AccountSubTypeCode ?? "0",
        //            CircularRefNum = x.CircularRefNum ?? "",
        //            InterestRate = x.InterestRate,
        //            NewInterestRate = x.NewInterestRate,
        //            PenalRate = x.PenalRate,
        //            EffectiveDate = x.EffectiveDate,
        //            ExpiryDate = x.ExpiryDate,
        //            DocumentId = x.DocumentId,
        //            Status = x.Status,
        //            CircularDate = x.CircularDate,
        //            AmountSlab = x.AmountSlab,
        //            TenorSerialNo = x.TenorSerialNo,
        //            IntRateType = x.IntRateType,
        //            SpreadRate = x.SpreadRate,
        //            Remark = x.Remark,
        //            OrtherNotes = x.OrtherNotes,
        //            StatusUpdateCore = x.StatusUpdateCore,
        //            CallApiTxnStatus = x.CallApiTxnStatus,
        //            CallApiReqRecordSl = x.CallApiReqRecordSl != null ? x.CallApiReqRecordSl.ToString() : null,
        //            CallApiResponseCode = x.CallApiResponseCode,
        //            CallApiResponseMsg = x.CallApiResponseMsg,
        //            CreatedBy = x.CreatedBy,
        //            CreatedDate = x.CreatedDate,
        //            ModifiedBy = x.ModifiedBy,
        //            ModifiedDate = x.ModifiedDate,
        //            ApproverBy = x.ApproverBy,
        //            ApprovalDate = x.ApprovalDate,
        //            IsSelected = false
        //        }).ToList();
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        public async Task<List<InterestRateConfigMasterView>> GetInterestRateConfigMasterViewListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, DateTime? fromDate, DateTime? toDate, string searchText, int? statusValue = null)
        {
            try
            {
                var query = _dbContext.InterestRateConfigMasterViews.AsQueryable();
                // .Where(x => x.Status == 1);
                if (!string.IsNullOrEmpty(productGroupCode))
                    query = query.Where(x => x.ProductGroupCode == productGroupCode);
                if (!string.IsNullOrEmpty(posCode))
                    query = query.Where(x => x.PosCode == posCode);
                if (!string.IsNullOrEmpty(productCode))
                    query = query.Where(x => x.ProductCode == productCode);
                if (!string.IsNullOrEmpty(circularRefNum))
                    query = query.Where(x => x.CircularRefNum.Contains(circularRefNum.Trim()));  // 
                if (fromDate.HasValue)
                    query = query.Where(x => x.EffectiveDate >= fromDate.Value);
                if (toDate.HasValue)
                    query = query.Where(x => x.EffectiveDate < toDate.Value);  // 
                var viewItems = await query.ToListAsync();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var searchTerm = searchText.Trim().ToLower();
                    viewItems = viewItems
                        .Where(x =>
            (x.ProductCode?.ToLower().Contains(searchTerm) ?? false) ||
            (x.CircularRefNum?.ToLower().Contains(searchTerm) ?? false) ||
            (x.AccountTypeList?.ToLower().Contains(searchTerm) ?? false)   // 
        )
                        .ToList();
                }

                if (statusValue.HasValue)
                {
                    if (statusValue.Value == -1)
                    {
                        // "Tất cả" 
                    }
                    else
                    {
                        viewItems = viewItems.Where(x => x.Status == statusValue.Value).ToList();
                    }
                }


                //if (!string.IsNullOrWhiteSpace(statusDesc))
                //{
                //    var searchTerm = statusDesc.Trim().ToUpper();

                //    viewItems = viewItems
                //        .Where(x => !string.IsNullOrEmpty(x.StatusDesc) &&
                //                    x.StatusDesc.ToUpper().Contains(searchTerm))
                //        .ToList();
                //}

                return viewItems;
            }



            // var viewItems = await query.ToListAsync();
            // 
            //  return viewItems;  // Trả về 
            // }
            catch (Exception ex)
            {
                //WriteLog(LogType.ERROR, $"Error in GetInterestRateConfigMasterViewListAsync: {ex.Message}");  // Uncomment nếu có WriteLog
                throw;
            }
        }
        public async Task<int> SaveInterestRateConfigMasterAsync(List<InterestRateConfigMaster> interestRateConfigMasters)
        {
            if (interestRateConfigMasters == null || !interestRateConfigMasters.Any())
            {
                return 0;
            }
            List<InterestRateConfigMaster> interestRateConfigMastersAdd = new();
            List<InterestRateConfigMaster> interestRateConfigMastersUpdate = new();
            foreach (var config in interestRateConfigMasters)
            {
                if (config.Id == 0)
                {
                    config.CreatedDate = DateTime.UtcNow;
                    interestRateConfigMastersAdd.Add(config);
                }
                else
                {
                    var existingConfig = await _dbContext.InterestRateConfigMasters.FindAsync(config.Id);
                    if (existingConfig != null)
                    {
                        _mapper.Map(config, existingConfig);
                        existingConfig.StatusUpdateCore = 1;
                        existingConfig.ModifiedDate = DateTime.UtcNow;
                        interestRateConfigMastersUpdate.Add(existingConfig);
                    }
                }
            }
            if (interestRateConfigMastersUpdate.Any())
            {
                _dbContext.InterestRateConfigMasters.UpdateRange(interestRateConfigMastersUpdate);
            }
            if (interestRateConfigMastersAdd.Any())
            {
                _dbContext.InterestRateConfigMasters.AddRange(interestRateConfigMastersAdd);
            }

            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteInterestRateConfigMasterAsync(long id, string userId)
        {
            var config = await _dbContext.InterestRateConfigMasters.FindAsync(id);
            if (config != null)
            {
                config.Status = 0;
                config.ModifiedBy = userId;
                config.ModifiedDate = DateTime.UtcNow;
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<DocumentModel> GetDocumentInfoByIdAsync(long id)
        {
            var document = await _dbContext.AttachedFileInfos.FindAsync(id);
            if (document == null)
            {
                return null;
            }
            var result = _mapper.Map<DocumentModel>(document);

            var interestRateConfigs = await _dbContext.InterestRateConfigMasters
                .Where(x => x.DocumentId == id).ToListAsync();
            if (interestRateConfigs != null && interestRateConfigs.Any())
            {
                result.InterestRateConfigMasters = new List<InterestRateConfigMaster>();
                result.InterestRateConfigMasters.AddRange(interestRateConfigs);
            }
            return result;
        }

        public async Task<List<AttachedFileInfo>> GetAttachedFilesAsync(long documentId)
        {
            try
            {
                var files = await _dbContext.AttachedFileInfos
                    .Where(f => f.DocumentId == documentId && f.Status == 1)
                    .ToListAsync();
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách file đính kèm: {ex.Message}");
                throw new Exception($"Lỗi khi lấy danh sách file đính kèm: {ex.Message}", ex);
            }
        }

        public async Task<List<AttachedFileInfo>> GetAttachedFilesAsync(string documentNumber)
        {
            try
            {
                var files = await _dbContext.AttachedFileInfos
                    .Where(f => f.DocumentNumber == documentNumber && f.Status == 1)
                    .ToListAsync();
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách file đính kèm: {ex.Message}");
                throw new Exception($"Lỗi khi lấy danh sách file đính kèm: {ex.Message}", ex);
            }
        }


        //public async Task<long> CreateNewDocumentId()
        //{
        //    // Logic tạo DocumentId mới (lấy ID lớn nhất + 1 từ bảng InterestRateConfigMasters)
        //    var maxId = await _context.InterestRateConfigMasters
        //        .Where(x => x.DocumentId.HasValue) // Đảm bảo chỉ lấy các bản ghi có DocumentId
        //        .MaxAsync(x => (long?)x.DocumentId) ?? 0;
        //    return maxId + 1;
        //}

        public async Task<long> CreateNewDocumentId()
        {
            // Lấy max từ InterestRateConfigMasters
            var maxMaster = await _dbContext.InterestRateConfigMasters
                .Where(x => x.DocumentId.HasValue)
                .MaxAsync(x => (long?)x.DocumentId) ?? 0;

            // Lấy max từ AttachedFileInfos
            var maxFiles = await _dbContext.AttachedFileInfos
                .MaxAsync(x => (long?)x.DocumentId) ?? 0;
            var maxId = Math.Max(maxMaster, maxFiles);
            var newId = maxId + 1;
            Console.WriteLine($"Created new DocumentId: {newId} (maxMaster={maxMaster}, maxFiles={maxFiles})");
            return newId;
        }

        //add
        //public async Task<long> CreateNewDocumentWithMaster(string userId)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Tạo DocumentId mới
        //        var maxId = await _context.InterestRateConfigMasters
        //            .Where(x => x.DocumentId.HasValue)
        //            .MaxAsync(x => (long?)x.DocumentId) ?? 0;
        //        var newDocumentId = maxId + 1;

        //        // Lấy PosCode hợp lệ từ DB
        //        var posCode = await _context.ListOfPoss.Select(p => p.Code).FirstOrDefaultAsync() ?? "000100";
        //        Console.WriteLine($"Generated PosCode: {posCode} from userId: {userId ?? "null"}");

        //        // Lấy ProductCode hợp lệ từ DB
        //        var productCode = await _context.ListOfProducts
        //            .Where(p => p.ProductGroupCode == "CASA")
        //            .Select(p => p.ProductCode)
        //            .FirstOrDefaultAsync() ?? "102";
        //        Console.WriteLine($"Generated ProductCode: {productCode}");

        //        // Giá trị mặc định cho các trường
        //        var accountSubTypeCode = "STD";
        //        var accountTypeCode = "SAV";
        //        var circularRefNum = $"CIR-{DateTime.UtcNow:yyyyMMdd}-{newDocumentId}";
        //        var currencyCode = "VND";
        //        var debitCreditFlag = "C";
        //        var intRateType = "FIXED";
        //        var interestRate = 0.0m;
        //        var penalRate = 0.0m; // Gán giá trị mặc định cho PenalRate
        //        var amountSlab = 0.0m;
        //        var tenorSerialNo = 1;
        //        var spreadRate = 0.0m;
        //        var status = 1;
        //        var statusUpdateCore = 0;
        //        var createdBy = userId ?? "UnknownUser";

        //        // Tạo bản ghi InterestRateConfigMaster mới với dữ liệu hợp lệ
        //        var newMaster = new InterestRateConfigMaster
        //        {
        //            DocumentId = newDocumentId,
        //            ProductGroupCode = "CASA",
        //            PosCode = posCode,
        //            PosName = "Default POS",
        //            ProductCode = productCode,
        //            ProductName = "Default Product",
        //            AccountTypeCode = accountTypeCode,
        //            AccountTypeName = "Savings Account",
        //            AccountSubTypeCode = accountSubTypeCode,
        //            AccountSubTypeName = "Standard",
        //            CircularRefNum = circularRefNum,
        //            CurrencyCode = currencyCode,
        //            DebitCreditFlag = debitCreditFlag,
        //            IntRateType = intRateType,
        //            InterestRate = interestRate,
        //            PenalRate = penalRate, // Thêm trường này
        //            AmountSlab = amountSlab,
        //            TenorSerialNo = tenorSerialNo,
        //            SpreadRate = spreadRate,
        //            Status = status,
        //            StatusUpdateCore = statusUpdateCore,
        //            CreatedBy = createdBy,
        //            CreatedDate = DateTime.UtcNow
        //        };

        //        // Kiểm tra tồn tại của PosCode, ProductCode
        //        if (!await _context.ListOfPoss.AnyAsync(p => p.Code == newMaster.PosCode) ||
        //            !await _context.ListOfProducts.AnyAsync(p => p.ProductCode == newMaster.ProductCode))
        //        {
        //            throw new Exception($"PosCode '{newMaster.PosCode}' hoặc ProductCode '{newMaster.ProductCode}' không hợp lệ.");
        //        }

        //        _context.InterestRateConfigMasters.Add(newMaster);
        //        await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //        Console.WriteLine($"Created new DocumentId: {newDocumentId} with Master record. Data: {JsonConvert.SerializeObject(newMaster)}");
        //        return newDocumentId;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        Console.WriteLine($"Error in CreateNewDocumentWithMaster: {ex.Message}\nInner Exception: {ex.InnerException?.Message}\nStackTrace: {ex.StackTrace}");
        //        throw;
        //    }
        //}

        public async Task<long> CreateNewDocumentWithMaster(string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var maxId = await _dbContext.InterestRateConfigMasters
                    .Where(x => x.DocumentId.HasValue)
                    .MaxAsync(x => (long?)x.DocumentId) ?? 0;
                var newDocumentId = maxId + 1;
                await transaction.CommitAsync();
                Console.WriteLine($"Created new DocumentId: {newDocumentId}");
                return newDocumentId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in CreateNewDocumentWithMaster: {ex.Message}\nInner Exception: {ex.InnerException?.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        //public async Task<int> SaveAttachedFiles(long configureId, List<AttachedFileInfo> attachedFiles, string userId)
        //{
        //    if (attachedFiles == null || !attachedFiles.Any())
        //    {
        //        return 0;
        //    }

        //    try
        //    {
        //        List<AttachedFileInfo> attachedFilesAdd = new();
        //        List<AttachedFileInfo> attachedFilesUpdate = new();

        //        foreach (var attachedFile in attachedFiles)
        //        {
        //            if (attachedFile.FileId == 0)
        //            {
        //                if (string.IsNullOrEmpty(attachedFile.CreatedBy))
        //                    attachedFile.CreatedBy = userId ?? "UnknownUser";
        //                if (attachedFile.CreatedDate == default)
        //                    attachedFile.CreatedDate = DateTime.UtcNow;

        //                // Kiểm tra dữ liệu bắt buộc
        //                if (string.IsNullOrEmpty(attachedFile.FileName) ||
        //                    string.IsNullOrEmpty(attachedFile.FileNameNew) ||
        //                    string.IsNullOrEmpty(attachedFile.PathFile))
        //                {
        //                    throw new Exception("FileName, FileNameNew, hoặc PathFile không được để trống.");
        //                }

        //                // Kiểm tra DocumentId và DocumentNumber
        //                if (attachedFile.DocumentId <= 0)
        //                {
        //                    throw new Exception("DocumentId không hợp lệ hoặc không được để trống.");
        //                }
        //                if (string.IsNullOrEmpty(attachedFile.DocumentNumber))
        //                {
        //                    throw new Exception("DocumentNumber không được để trống.");
        //                }

        //                // Kiểm tra tồn tại DocumentId trong InterestRateConfigMasters
        //                if (!await _context.InterestRateConfigMasters.AnyAsync(m => m.DocumentId == attachedFile.DocumentId))
        //                {
        //                    throw new Exception($"DocumentId {attachedFile.DocumentId} không tồn tại trong InterestRateConfigMasters.");
        //                }

        //                attachedFilesAdd.Add(attachedFile);
        //            }
        //            else
        //            {
        //                var existingAttachedFile = await _context.AttachedFileInfos.FindAsync(attachedFile.FileId);
        //                if (existingAttachedFile != null)
        //                {
        //                    _mapper.Map(attachedFile, existingAttachedFile);
        //                    existingAttachedFile.ModifiedBy = userId ?? "UnknownUser";
        //                    existingAttachedFile.ModifiedDate = DateTime.UtcNow;
        //                    attachedFilesUpdate.Add(existingAttachedFile);
        //                }
        //            }
        //        }

        //        if (attachedFilesUpdate.Any())
        //        {
        //            _context.AttachedFileInfos.UpdateRange(attachedFilesUpdate);
        //        }

        //        if (attachedFilesAdd.Any())
        //        {
        //            _context.AttachedFileInfos.AddRange(attachedFilesAdd);
        //        }

        //        var changes = await _context.SaveChangesAsync();
        //        Console.WriteLine($"Saved {changes} attached file records, DocumentId: {attachedFiles.First().DocumentId}, DocumentNumber: {attachedFiles.First().DocumentNumber}, Data: {JsonConvert.SerializeObject(attachedFiles)}");
        //        return changes;
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        var innerException = ex.InnerException?.Message ?? "Không có inner exception";
        //        Console.WriteLine($"DB Update Error in SaveAttachedFiles: {ex.Message}\nInner Exception: {innerException}\nStackTrace: {ex.StackTrace}");
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        var innerException = ex.InnerException?.Message ?? "Không có inner exception";
        //        Console.WriteLine($"Error in SaveAttachedFiles: {ex.Message}\nInner Exception: {innerException}\nStackTrace: {ex.StackTrace}");
        //        throw;
        //    }
        //}
        //add

        public async Task<List<long>> SaveAttachedFiles(long configureId, List<AttachedFileInfo> attachedFiles, string userId)
        {
            if (attachedFiles == null || !attachedFiles.Any())
            {
                return null;
            }
            try
            {

                List<long> result = new List<long>();
                List<AttachedFileInfo> attachedFilesAdd = new();
                List<AttachedFileInfo> attachedFilesUpdate = new();
                foreach (var attachedFile in attachedFiles)
                {
                    if (attachedFile.FileId == 0)
                    {
                        if (string.IsNullOrEmpty(attachedFile.CreatedBy))
                            attachedFile.CreatedBy = userId ?? "UnknownUser";
                        if (attachedFile.CreatedDate == default)
                            attachedFile.CreatedDate = DateTime.UtcNow;
                        if (string.IsNullOrEmpty(attachedFile.FileName) ||
                            string.IsNullOrEmpty(attachedFile.FileNameNew) ||
                            string.IsNullOrEmpty(attachedFile.PathFile))
                        {
                            throw new Exception("FileName, FileNameNew, hoặc PathFile không được để trống.");
                        }
                        //if (attachedFile.DocumentId <= 0)
                        //{
                        //    throw new Exception("DocumentId không hợp lệ hoặc không được để trống.");
                        //}
                        if (string.IsNullOrEmpty(attachedFile.DocumentNumber))
                        {
                            throw new Exception("DocumentNumber không được để trống.");
                        }
                        attachedFilesAdd.Add(attachedFile);
                    }
                    else
                    {
                        var existingAttachedFile = await _dbContext.AttachedFileInfos.FindAsync(attachedFile.FileId);
                        if (existingAttachedFile != null)
                        {
                            _mapper.Map(attachedFile, existingAttachedFile);
                            existingAttachedFile.ModifiedBy = userId ?? "UnknownUser";
                            existingAttachedFile.ModifiedDate = DateTime.UtcNow;
                            attachedFilesUpdate.Add(existingAttachedFile);
                        }
                    }
                }
                if (attachedFilesUpdate.Any())
                {
                    _dbContext.AttachedFileInfos.UpdateRange(attachedFilesUpdate);
                }
                if (attachedFilesAdd.Any())
                {
                    _dbContext.AttachedFileInfos.AddRange(attachedFilesAdd);
                }

                var changes = await _dbContext.SaveChangesAsync();

                if (attachedFilesAdd.Any())
                {
                    result.AddRange(attachedFilesAdd.Select(s => s.FileId).ToList());
                }

                if (attachedFilesUpdate.Any())
                {
                    result.AddRange(attachedFilesUpdate.Select(s => s.FileId).ToList());
                }

                // Console.WriteLine($"Saved {changes} attached file records, DocumentId: {attachedFiles.First().DocumentId}, DocumentNumber: {attachedFiles.First().DocumentNumber}, Data: {JsonConvert.SerializeObject(attachedFiles)}");
                return result;
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? "Không có inner exception";
                Console.WriteLine($"DB Update Error in SaveAttachedFiles: {ex.Message}\nInner Exception: {innerException}\nStackTrace: {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? "Không có inner exception";
                Console.WriteLine($"Error in SaveAttachedFiles: {ex.Message}\nInner Exception: {innerException}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<int> DeleteAttachedFilesAsync(long fileId, string userId)
        {
            var attachedFile = await _dbContext.AttachedFileInfos.FindAsync(fileId);
            if (attachedFile != null)
            {
                attachedFile.Status = 0;
                attachedFile.ModifiedBy = userId;
                attachedFile.ModifiedDate = DateTime.UtcNow;
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<int> UpdateInterestRateConfigMasterAsync(List<InterestRateConfigMaster> configs)
        {
            if (configs == null || !configs.Any())
            {
                return 0;
            }

            var existingConfigs = await _dbContext.InterestRateConfigMasters
                .Where(x => configs.Select(c => c.Id).Contains(x.Id))
                .ToListAsync();
            foreach (var config in configs)
            {
                var existingConfig = existingConfigs.FirstOrDefault(x => x.Id == config.Id);
                if (existingConfig != null)
                {
                    _dbContext.Entry(existingConfig).CurrentValues.SetValues(config);
                    existingConfig.ModifiedDate = DateTime.UtcNow;
                    existingConfig.ModifiedBy = config.ModifiedBy;
                }
            }

            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateInterestRateConfigMasterStatus(string userName, List<long> lstId, int status, long documentId)
        {
            if (lstId == null || !lstId.Any())
            {
                return 0;
            }

            var existingConfigs = await _dbContext.InterestRateConfigMasters.Where(x => lstId.Contains(x.Id)).ToListAsync();

            foreach (var config in lstId)
            {
                var existingConfig = existingConfigs.FirstOrDefault(x => x.Id == config);
                if (existingConfig != null)
                {
                    existingConfig.Status = status;
                    existingConfig.DocumentId = documentId != 0 ? documentId : existingConfig.DocumentId;
                    existingConfig.ModifiedDate = DateTime.UtcNow;
                    existingConfig.ModifiedBy = userName;
                }
            }
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<List<TideTermViewModel>> GetTideProdList(string posCode, string productCode, DateTime? effectDate)
        {
            try
            {
                posCode = string.IsNullOrEmpty(posCode) ? "0" : posCode;
                productCode = string.IsNullOrEmpty(productCode) ? "0" : productCode;
                effectDate = effectDate == null ? DateTime.Today : effectDate;
                string _strEffectDate = effectDate?.ToString("yyyyMMdd");
                var _request = new TideIntRateRequestViewModel();
                _request.PosCode = posCode;
                _request.ProdCode = productCode;
                _request.EffectDate = _strEffectDate;
                _request.UserId = ConstValueAPI.UserId_Call_ApiIDC;
                _request.SourceId = ConstValueAPI.SourceId;
                var response = await _apiInternalEsbService.GetListDepositInterestRate(_request);
                if (response == null || response.Result == null || !response.Result.Any())
                {
                    return new List<TideTermViewModel>();
                }
                List<TideTermViewModel> result = new List<TideTermViewModel>();
                var data = response.Result[0].ProdDetails;
                var _lstProductList = _productService.GetAccountTypes("");
                int _id = 1;
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].TermDetails.Count; j++)
                    {
                        TideTermViewModel item = new TideTermViewModel();
                        item.Id = _id;
                        item.TermProductCode = data[i].ProdCode;
                        item.TermProductName = data[i].ProdName;
                        item.TermAccountTypeCode = data[i].DepositType;
                        var product = _lstProductList.FirstOrDefault(p => p.Value == data[i].DepositType);

                        item.TermAccountTypeName = product == null ? "" : product.Text;
                        item.TermAccountSubTypeCode = data[i].DepositSubType;
                        item.TermCurrencyCode = data[i].Currency;
                        item.TermEffectiveDate = string.IsNullOrEmpty(data[i].EffectDate) ? DateTime.Now : CustConverter.StringToDate(data[i].EffectDate);
                        item.TermAmoutSlab = data[i].SlabRange == null ? 0 : Convert.ToDecimal(data[i].SlabRange);
                        item.TermSerial = int.Parse(data[i].TermDetails[j].Serial);
                        item.TermDesc = data[i].TermDetails[j].TermDesc;
                        item.TermValue = int.Parse(data[i].TermDetails[j].TermValue);
                        item.TermUnit = data[i].TermDetails[j].TermUnit;
                        item.InclusionFlag = data[i].TermDetails[j].InclusionFlag;
                        item.TermIntRate = data[i].TermDetails[j].IntRate == null ? 0 : Convert.ToDecimal(data[i].TermDetails[j].IntRate);
                        item.TermIntRateNew = data[i].TermDetails[j].IntRate == null ? 0 : Convert.ToDecimal(data[i].TermDetails[j].IntRate);

                        //Phan them bien do lai suat
                        if (posCode == PosValue.HEAD_POS)
                        {
                            item.MinInterestRateSpread = 0;
                            item.MaxInterestRateSpread = 0;
                            item.MinTermIntRateNew = item.TermIntRate;
                            item.MaxTermIntRateNew = item.TermIntRate;
                        }
                        else
                        {
                            var productParameter = _productService.GetProductParameter(ProductGroupCode.ProductGroupCode_Tide, data[i].ProdCode, null);
                            item.MinInterestRateSpread = productParameter?.MinInterestRateSpread ?? 0;
                            item.MaxInterestRateSpread = productParameter?.MaxInterestRateSpread ?? 0;
                            item.MinTermIntRateNew = item.TermIntRate - (productParameter?.MinInterestRateSpread ?? 0);
                            item.MaxTermIntRateNew = item.TermIntRate + (productParameter?.MaxInterestRateSpread ?? 0);
                        }

                        result.Add(item);
                        _id++;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception($"Lỗi khi gọi API lấy danh sách sản phẩm Tide: {ex.Message}", ex);
            }
        }

        public async Task<List<TideTermViewModel>> GetTideProdList(string sessionId, string userName, string posCode, List<string> productTypes, DateTime? effectDate, string sourceFlag)
        {
            try
            {
                List<TideTermViewModel> result = new List<TideTermViewModel>();

                if (sourceFlag == "0") // from api
                {
                    //Lấy ra list Products theo productTypes
                    List<string> _lstProducts = _dbContext.ListOfProducts.Where(w => productTypes.Contains(w.AccountTypeCode)).Select(s => s.ProductCode).Distinct().ToList();
                    if (_lstProducts != null && _lstProducts.Count > 0)
                    {
                        for (int i = 0; i < _lstProducts.Count; i++)
                        {
                            var _productCode = _lstProducts[i];
                            List<TideTermViewModel> _lstTideTermViewModelRaw = await GetTideProdList(posCode, _productCode, effectDate);
                            List<TideTermViewModel> _lstTideTermViewModel = _lstTideTermViewModelRaw.Where(w => productTypes.Contains(w.TermAccountTypeCode)).ToList();
                            result.AddRange(_lstTideTermViewModel);
                        }
                    }

                    await SaveToWorkingTableAsync(sessionId, userName, posCode, result);
                }
                else // from database
                {
                    result = await GetWorkingTermsAsync(sessionId);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception($"Lỗi khi gọi API lấy danh sách sản phẩm Tide: {ex.Message}", ex);
            }
        }


        public async Task SaveToWorkingTableAsync(string sessionId, string userName, string posCode, List<TideTermViewModel> list)
        {
            try
            {
                // Xóa bản cũ
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "DELETE FROM TideTermWorking WHERE SessionId = {0}", sessionId);

                foreach (var item in list)
                {
                    var row = new TideTermWorking
                    {
                        SessionId = sessionId,
                        TermProductCode = item.TermProductCode,
                        TermProductName = item.TermProductName,
                        TermAccountTypeCode = item.TermAccountTypeCode,
                        TermAccountTypeName = item.TermAccountTypeName,
                        TermAccountSubTypeCode = item.TermAccountSubTypeCode,
                        TermEffectiveDate = (item.TermEffectiveDate.HasValue && item.TermEffectiveDate.Value.Year >= 1900) ? item.TermEffectiveDate : null,
                        TermPosCode = posCode,
                        TermAmoutSlab = item.TermAmoutSlab,
                        TermIntRate = item.TermIntRate,
                        TermIntRateNew = item.TermIntRateNew,
                        TermIntType = item.TermIntType,
                        TermSpreadRate = item.TermSpreadRate,
                        TermCurrencyCode = item.TermCurrencyCode,
                        TermSerial = item.TermSerial ?? 0,
                        TermDesc = item.TermDesc,
                        TermValue = item.TermValue ?? 0,
                        InclusionFlag = item.InclusionFlag,
                        TermUnit = item.TermUnit,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userName,
                        ChangeFlag = 0,
                        MinInterestRateSpread = item.MinInterestRateSpread,
                        MaxInterestRateSpread = item.MaxInterestRateSpread
                    };

                    _dbContext.TideTermWorkings.Add(row);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu dữ liệu vào bảng làm việc TideTermWorking: {ex.Message}");
                throw new Exception($"Lỗi khi lưu dữ liệu vào bảng làm việc TideTermWorking: {ex.Message}", ex);
            }
        }

        public async Task<List<TideTermViewModel>> GetWorkingTermsAsync(string sessionId)
        {
            return await _dbContext.TideTermWorkings
                .Where(x => x.SessionId == sessionId)
                .OrderBy(x => x.TermAccountTypeCode).ThenBy(o => o.TermSerial)
                .Select(x => new TideTermViewModel
                {
                    Id = x.Id,
                    TermProductCode = x.TermProductCode,
                    TermProductName = x.TermProductName,
                    TermAccountTypeCode = x.TermAccountTypeCode,
                    TermAccountTypeName = x.TermAccountTypeName,
                    TermAccountSubTypeCode = x.TermAccountSubTypeCode,
                    TermEffectiveDate = x.TermEffectiveDate,
                    TermPosCode = x.TermPosCode,
                    TermAmoutSlab = x.TermAmoutSlab,
                    TermIntRate = x.TermIntRate,
                    TermIntRateNew = x.TermIntRateNew,
                    TermIntType = x.TermIntType,
                    TermSpreadRate = x.TermSpreadRate,
                    TermCurrencyCode = x.TermCurrencyCode,
                    TermSerial = x.TermSerial,
                    TermDesc = x.TermDesc,
                    TermValue = x.TermValue,
                    InclusionFlag = x.InclusionFlag,
                    TermUnit = x.TermUnit,
                    ChangeFlag = x.ChangeFlag,
                    MinInterestRateSpread = x.MinInterestRateSpread,
                    MaxInterestRateSpread = x.MaxInterestRateSpread
                })
                .ToListAsync();
        }


        public async Task<int> UpdateTideConfigureTemp(List<DepositTermModel> depositTerms, double interestRate, string sessionId, string userName, string userPosCode)
        {
            try
            {
                foreach (var term in depositTerms)
                {
                    var existingTerms = await _dbContext.TideTermWorkings
                        .Where(x => x.SessionId == sessionId && x.TermValue == term.TermValue && x.TermUnit == term.TermUnitCode).ToListAsync();
                    if (existingTerms != null && existingTerms.Count > 0)
                    {
                        for (int i = 0; i < existingTerms.Count; i++)
                        {
                            //Bổ sung phần validation khi nhập lãi suất
                            double minInterestRate =
                                (double)(existingTerms[i].TermIntRate - existingTerms[i].MinInterestRateSpread ?? 0);
                            double maxInterestRate = (double)(existingTerms[i].TermIntRate + existingTerms[i].MaxInterestRateSpread ?? 0);
                            if (userPosCode != PosValue.HEAD_POS &&( interestRate < minInterestRate || interestRate > maxInterestRate))
                            {
                                Console.WriteLine($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
                                throw new Exception($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
                            }
                            else
                            {
                                existingTerms[i].TermIntRateNew = (decimal)interestRate;
                                existingTerms[i].CreatedBy = userName;
                                existingTerms[i].CreatedDate = DateTime.UtcNow;
                                existingTerms[i].ChangeFlag = 1;
                            }
                        }
                    }
                }
                var updateRowCnt = await _dbContext.SaveChangesAsync();
                return updateRowCnt;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật cấu hình tạm thời Tide: {ex.Message}");
                throw new Exception($"{ex.Message}", ex);
            }
        }

        //add lai
        public async Task<List<CasaRateProductViewModel>> GetCasaProdList(string posCode, string productCode, DateTime? effectDate)
        {
            try
            {
                posCode = string.IsNullOrEmpty(posCode) || (posCode != "0" && posCode != "0000") ? "0" : posCode;
                productCode = string.IsNullOrEmpty(productCode) ? "0" : productCode;
                effectDate = effectDate ?? DateTime.Today;
                string _strEffectDate = effectDate.Value.ToString("yyyyMMdd");
                var _request = new CasaIntRateRequestViewModel
                {
                    PosCode = posCode,
                    ProductCode = productCode,
                    EffectiveDate = _strEffectDate,
                    UserId = ConstValueAPI.UserId_Call_ApiIDC,
                    DebitCreditFlag = "C",
                    Currency = "VND"
                };
                var response = await _apiInternalEsbService.GetListCasaInterestRate(_request);
                if (response == null || response.Result == null || !response.Result.Any())
                {
                    Console.WriteLine("No data returned from GetListCasaInterestRate API");
                    return new List<CasaRateProductViewModel>();
                }
                var _lstProductList = _productService.GetAccountTypes("");
                Console.WriteLine($"AccountTypes: {System.Text.Json.JsonSerializer.Serialize(_lstProductList)}");
                int _id = 1;
                var result = new List<CasaRateProductViewModel>();
                foreach (var data in response.Result)
                {
                    var item = new CasaRateProductViewModel
                    {
                        Id = _id,
                        RateProductCode = data.ProdCode ?? "",
                        RateProductName = data.ProdCode ?? "",
                        RateProductAccountTypeCode = data.AccountType ?? "",
                        RateProductAccountTypeName = _lstProductList.FirstOrDefault(p => p.Value == data.AccountType)?.Text ?? data.AccountType ?? "",
                        RateProductAccountSubTypeCode = data.SubType ?? "",
                        RateProductCurrencyCode = data.Currency ?? "VND",
                        RateProductDebitCreditFlag = data.DebitCreditFlag ?? "C",
                        RateProductEffectiveDate = string.IsNullOrEmpty(data.EffectiveDate) ? DateTime.Now : DateTime.ParseExact(data.EffectiveDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        RateProductInterestRate = string.IsNullOrEmpty(data.InterestRate) ? 0 : Convert.ToDecimal(data.InterestRate, CultureInfo.InvariantCulture),
                        RateProductNewInterestRate = string.IsNullOrEmpty(data.InterestRate) ? 0 : Convert.ToDecimal(data.InterestRate, CultureInfo.InvariantCulture),
                        RateProductPenalRate = string.IsNullOrEmpty(data.PenalRate) ? 0 : Convert.ToDecimal(data.PenalRate, CultureInfo.InvariantCulture),
                        RateProductExpiredDate = string.IsNullOrEmpty(data.PosRateExpiryDate) ? null : DateTime.ParseExact(data.PosRateExpiryDate, "yyyyMMdd", CultureInfo.InvariantCulture),
                        RateProductPosCode = data.PosCode ?? "",
                    };
                    result.Add(item);
                    _id++;
                }
                Console.WriteLine($"GetCasaProdList Result: {System.Text.Json.JsonSerializer.Serialize(result)}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception($"Lỗi khi gọi API lấy danh sách sản phẩm CASA: {ex.Message}", ex);
            }
        }


        public async Task<List<CasaRateProductViewModel>> GetCasaRateByProductsAndTypesAsyncWithHO(List<string> productCodes, List<string> accountTypes, string posCode, DateTime referenceDate)
        {
            try
            {
                List<CasaRateProductViewModel> casaRateProductViewHOModels = await GetCasaRateByProductsAndTypesAsync(productCodes, accountTypes, PosValue.HEAD_POS, referenceDate);


                if (posCode == PosValue.HEAD_POS)
                {
                    foreach (CasaRateProductViewModel item in casaRateProductViewHOModels)
                    {
                        item.InterestRateHO = item.RateProductInterestRate ?? 0;
                        item.MaxInterestRateSpread = 0;
                        item.MinInterestRateSpread = 0;
                    }
                    return casaRateProductViewHOModels;

                }
                else
                {
                    List<CasaRateProductViewModel> casaRateProductViewModels = await GetCasaRateByProductsAndTypesAsync(productCodes, accountTypes, posCode, referenceDate);

                    if (casaRateProductViewModels == null || casaRateProductViewModels.Count == 0)
                    {
                        foreach (CasaRateProductViewModel item in casaRateProductViewHOModels)
                        {
                            item.InterestRateHO = item.RateProductInterestRate ?? 0;
                        }
                        return casaRateProductViewHOModels;
                    }
                    else
                    {
                        foreach (CasaRateProductViewModel item in casaRateProductViewModels)
                        {
                            var interestRateHO = casaRateProductViewHOModels.Where(w => w.RateProductCode == item.RateProductPosCode
                            && w.RateProductAccountTypeCode == item.RateProductAccountTypeCode).FirstOrDefault();
                            if (interestRateHO != null)
                            {
                                item.InterestRateHO = interestRateHO.RateProductInterestRate ?? 0;
                            }
                        }

                        return casaRateProductViewModels;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
                throw e;
            }
        }


        public async Task<List<CasaRateProductViewModel>> GetCasaRateByProductsAndTypesAsync(List<string> productCodes, List<string> accountTypes, string posCode,
      DateTime referenceDate)
        {
            try
            {
                if (string.IsNullOrEmpty(posCode) || posCode == PosValue.HEAD_POS)
                    posCode = "0";

                if (!productCodes.Any() || !accountTypes.Any())
                    return new List<CasaRateProductViewModel>();

                var result = new List<CasaRateProductViewModel>();
                var seenAccountTypes = new HashSet<string>(); // ← THÊM: Loại trùng AccountType
                var accountTypeList = _productService.GetAccountTypes("");

                var normalizedAccountTypes = accountTypes.Select(at => at.Trim().ToUpper()).ToHashSet();

                var request = new CasaIntRateRequestViewModel
                {
                    PosCode = posCode,
                    EffectiveDate = referenceDate.ToString("yyyyMMdd"),
                    UserId = ConstValueAPI.UserId_Call_ApiIDC,
                    DebitCreditFlag = "C",
                    Currency = "VND"
                };


                var listProductParameters = _productService.GetListProductParametersSearch(ProductGroupCode.ProductGroupCode_Casa, "",
                                                                    referenceDate, ConfigStatus.AUTHORIZED.Value);

                int globalIdCounter = 1;

                foreach (var prodCode in productCodes.Distinct())
                {
                    request.ProductCode = prodCode.Trim();

                    var response = await _apiInternalEsbService.GetListCasaInterestRate(request);

                    if (response?.Result == null || !response.Result.Any())
                        continue;

                    foreach (var data in response.Result)
                    {
                        var apiAccountTypeNorm = (data.AccountType ?? "").Trim().ToUpper();

                        if (!normalizedAccountTypes.Contains(apiAccountTypeNorm))
                            continue;

                        var apiAccountTypeOrig = (data.AccountType ?? "").Trim();

                        // ← THÊM: Chỉ add nếu chưa có AccountType này
                        if (seenAccountTypes.Contains(apiAccountTypeNorm))
                            continue;

                        seenAccountTypes.Add(apiAccountTypeNorm);

                        var accountTypeName = accountTypeList
                            .FirstOrDefault(p => p.Value.Trim().ToUpper() == apiAccountTypeNorm)?.Text
                            ?? data.AccountType ?? "Không xác định";

                        decimal minInterestRateSpread = 0;
                        decimal maxInterestRateSpread = 0;

                        if (listProductParameters != null && listProductParameters.Count != 0)
                        {
                            minInterestRateSpread = listProductParameters.Where(w => w.ProductCode == data.ProdCode).OrderByDescending(o => o.Status).FirstOrDefault().MinInterestRateSpread;
                            maxInterestRateSpread = listProductParameters.Where(w => w.ProductCode == data.ProdCode).OrderByDescending(o => o.Status).FirstOrDefault().MaxInterestRateSpread;
                        }


                        result.Add(new CasaRateProductViewModel
                        {
                            Id = globalIdCounter++,
                            RateProductCode = data.ProdCode ?? prodCode,
                            RateProductName = data.ProdCode ?? "Unknown Product",
                            RateProductAccountTypeCode = apiAccountTypeOrig,
                            RateProductAccountTypeName = accountTypeName,
                            RateProductAccountSubTypeCode = (data.SubType ?? "").Trim(),
                            RateProductCurrencyCode = data.Currency ?? "VND",
                            RateProductDebitCreditFlag = data.DebitCreditFlag ?? "C",
                            RateProductEffectiveDate = ParseDate(data.EffectiveDate) ?? referenceDate,
                            RateProductInterestRate = ParseDecimal(data.InterestRate),
                            RateProductNewInterestRate = ParseDecimal(data.InterestRate),
                            RateProductPenalRate = ParseDecimal(data.PenalRate),
                            RateProductExpiredDate = ParseExpiryDate(data.PosRateExpiryDate),
                            RateProductPosCode = data.PosCode ?? posCode,
                            RateProductAmoutSlab = 0m,
                            MinInterestRateSpread = minInterestRateSpread,
                            MaxInterestRateSpread = maxInterestRateSpread
                        });
                    }

                    // Nếu đã đủ số lượng AccountType cần → break sớm để tối ưu
                    if (seenAccountTypes.Count >= accountTypes.Count)
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCasaRateByProductsAndTypesAsync");
                throw;
            }
        }

        // Helper để code sạch hơn
        private DateTime? ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;
            return DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                ? dt : (DateTime?)null;
        }

        private DateTime? ParseExpiryDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;
            return DateTime.TryParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                ? dt : (DateTime?)null;
        }

        private decimal ParseDecimal(string value)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
        }


        //add 
        // Thêm method: GetCasaTermsAsync 
        //public async Task<List<CasaRateProductViewModel>> GetCasaTermsAsync(List<string> intRateConfigIds)
        //{
        //    try
        //    {
        //        // Chuyển string → long (bỏ qua invalid)
        //        var longIds = intRateConfigIds
        //            .Where(id => long.TryParse(id, out _))
        //            .Select(long.Parse)
        //            .ToList();
        //        if (!longIds.Any())
        //            return new List<CasaRateProductViewModel>();

        //        // Query join master (không cần terms cho CASA, chỉ lấy masters matching Ids)
        //        var query = from master in _dbContext.InterestRateConfigMasters
        //                    where longIds.Contains(master.Id) && master.ProductGroupCode == ProductGroupCode.CASA.Code
        //                    orderby master.AccountTypeCode
        //                    select new CasaRateProductViewModel
        //                    {
        //                        Id = master.Id,
        //                        RateProductCode = master.ProductCode ?? "",
        //                        RateProductName = master.ProductName ?? "",
        //                        RateProductAccountTypeCode = master.AccountTypeCode ?? "",
        //                        RateProductAccountTypeName = master.AccountTypeName ?? "",
        //                        RateProductAccountSubTypeCode = master.AccountSubTypeCode ?? "0",
        //                        RateProductCurrencyCode = master.CurrencyCode ?? "VND",
        //                        RateProductDebitCreditFlag = master.DebitCreditFlag ?? "C",
        //                        RateProductEffectiveDate = master.EffectiveDate,
        //                        RateProductInterestRate = master.InterestRate,
        //                        RateProductNewInterestRate = master.NewInterestRate ?? 0m,
        //                        RateProductPenalRate = master.PenalRate ?? 0m,
        //                        RateProductExpiredDate = master.ExpiryDate,
        //                        RateProductPosCode = master.PosCode ?? "0",
        //                        RateProductAmoutSlab = master.AmountSlab
        //                    };
        //        var data = await query.ToListAsync();
        //       // Console.WriteLine($"GetCasaTermsAsync: Loaded {data.Count} products for Ids: {string.Join(",", longIds)}");
        //        return data;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in GetCasaTermsAsync: {ex.Message}");
        //        throw;
        //    }
        //}



        //public async Task<List<CasaRateProductViewModel>> GetCasaTermsAsync(List<string> intRateConfigIds)
        //{
        //    try
        //    {
        //        // Chuyển string → long (bỏ qua invalid)
        //        var longIds = intRateConfigIds
        //            .Where(id => long.TryParse(id, out _))
        //            .Select(long.Parse)
        //            .ToList();
        //        if (!longIds.Any())
        //        {
        //          //  WriteLog(LogType.WARNING, $"GetCasaTermsAsync: No valid longIds from {intRateConfigIds.Count} strings");
        //            return new List<CasaRateProductViewModel>();
        //        }
        //        // Query chỉ từ masters (Casa không có terms), filter ProductGroupCode
        //        var query = from master in _dbContext.InterestRateConfigMasters
        //                    where longIds.Contains(master.Id) && master.ProductGroupCode == ProductGroupCode.CASA.Code && master.Status == 1 // THÊM: Status active
        //                    orderby master.AccountTypeCode // 
        //                    select new CasaRateProductViewModel
        //                    {
        //                        Id = master.Id,
        //                        RateProductCode = master.ProductCode ?? "",
        //                        RateProductName = master.ProductName ?? "",
        //                        RateProductAccountTypeCode = master.AccountTypeCode ?? "",
        //                        RateProductAccountTypeName = master.AccountTypeName ?? "",
        //                        RateProductAccountSubTypeCode = master.AccountSubTypeCode ?? "0",
        //                        RateProductCurrencyCode = master.CurrencyCode ?? "VND",
        //                        RateProductDebitCreditFlag = master.DebitCreditFlag ?? "C",
        //                        RateProductEffectiveDate = master.EffectiveDate,
        //                        RateProductInterestRate = master.InterestRate,
        //                        RateProductNewInterestRate = master.NewInterestRate ?? 0m,
        //                        RateProductPenalRate = master.PenalRate ?? 0m,
        //                        RateProductExpiredDate = master.ExpiryDate,
        //                        RateProductPosCode = master.PosCode ?? "0",
        //                        RateProductAmoutSlab = master.AmountSlab ,

        //                        ChangeFlag = (master.NewInterestRate != master.InterestRate) ? 1 : 0
        //                    };
        //        var data = await query.ToListAsync();
        //       // WriteLog(LogType.INFOR, $"GetCasaTermsAsync: Loaded {data.Count} products for Ids: {string.Join(",", longIds.Take(5))}... (total {longIds.Count})"); // Log debug
        //        return data;
        //    }
        //    catch (Exception ex)
        //    {
        //       // WriteLog(LogType.ERROR, $"Error in GetCasaTermsAsync: {ex.Message}");
        //        throw;
        //    }
        //}

        public async Task<List<CasaRateProductViewModel>> GetCasaTermsAsync(List<string> intRateConfigIds)
        {
            try
            {
                // Chuyển string → long (bỏ qua invalid)
                var longIds = intRateConfigIds
                    .Where(id => long.TryParse(id, out _))
                    .Select(long.Parse)
                    .ToList();

                if (!longIds.Any())
                {
                    // WriteLog(LogType.WARNING, $"GetCasaTermsAsync: No valid longIds from {intRateConfigIds.Count} strings");
                    return new List<CasaRateProductViewModel>();
                }


                var query = from master in _dbContext.InterestRateConfigMasters
                            where longIds.Contains(master.Id)
                               && master.ProductGroupCode == ProductGroupCode.CASA.Code
                            // XÓA HOẶC COMMENT DÒNG SAU ĐỂ HIỆN TẤT CẢ TRẠNG THÁI
                            // && master.Status == 1
                            orderby master.AccountTypeCode
                            select new CasaRateProductViewModel
                            {
                                Id = master.Id,
                                RateProductCode = master.ProductCode ?? "",
                                RateProductName = master.ProductName ?? "",
                                RateProductAccountTypeCode = master.AccountTypeCode ?? "",
                                RateProductAccountTypeName = master.AccountTypeName ?? "",
                                RateProductAccountSubTypeCode = master.AccountSubTypeCode ?? "0",
                                RateProductCurrencyCode = master.CurrencyCode ?? "VND",
                                RateProductDebitCreditFlag = master.DebitCreditFlag ?? "C",
                                RateProductEffectiveDate = master.EffectiveDate,
                                RateProductInterestRate = master.InterestRate,
                                RateProductNewInterestRate = master.NewInterestRate ?? 0m,
                                RateProductPenalRate = master.PenalRate ?? 0m,
                                RateProductExpiredDate = master.ExpiryDate,
                                RateProductPosCode = master.PosCode ?? "0",
                                RateProductAmoutSlab = master.AmountSlab,
                                ChangeFlag = (master.NewInterestRate != master.InterestRate) ? 1 : 0
                            };

                var data = await query.ToListAsync();

                // WriteLog(LogType.INFOR, $"GetCasaTermsAsync: Loaded {data.Count} products for Ids: {string.Join(",", longIds.Take(5))}... (total {longIds.Count})");
                return data;
            }
            catch (Exception ex)
            {
                // WriteLog(LogType.ERROR, $"Error in GetCasaTermsAsync: {ex.Message}");
                throw;
            }
        }


        public async Task<List<InterestRateConfigMasterModel>> GetCasaListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, string fromEffectiveDate, string toEffectiveDate)
        {
            try
            {
                var query = _dbContext.InterestRateConfigMasters
                    .Where(x => x.ProductGroupCode == ProductGroupCode.CASA.Code && x.Status != StatusLov.StatusClosed);
                if (!string.IsNullOrEmpty(posCode))
                    query = query.Where(x => x.PosCode.Contains(posCode));
                if (!string.IsNullOrEmpty(productCode))
                    query = query.Where(x => x.ProductCode.Contains(productCode));
                if (!string.IsNullOrEmpty(circularRefNum))
                    query = query.Where(x => x.CircularRefNum.Contains(circularRefNum));
                if (!string.IsNullOrEmpty(fromEffectiveDate) && DateTime.TryParse(fromEffectiveDate, out var fromDate))
                    query = query.Where(x => x.EffectiveDate >= fromDate);
                if (!string.IsNullOrEmpty(toEffectiveDate) && DateTime.TryParse(toEffectiveDate, out var toDate))
                    query = query.Where(x => x.EffectiveDate <= toDate);
                var result = await query
                    .Select(x => new InterestRateConfigMasterModel
                    {
                        Id = x.Id,
                        PosCode = x.PosCode,
                        PosName = x.PosName,
                        ProductCode = x.ProductCode,
                        ProductName = x.ProductName,
                        AccountTypeName = x.AccountTypeName,
                        CircularRefNum = x.CircularRefNum,
                        InterestRate = x.InterestRate,
                        EffectiveDate = x.EffectiveDate,
                        ExpiryDate = x.ExpiryDate,
                        DocumentId = x.DocumentId,
                        Status = x.Status,
                        IsSelected = false
                    })
                    .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách Casa: {ex.Message}");
                throw new Exception($"Lỗi khi lấy danh sách Casa: {ex.Message}", ex);
            }
        }

        //add thêm mã sản phẩm

        public async Task<string> SaveTideRateConfigureData(TideRateConfigureViewModel intRateConfigMaster, List<TideTermViewModel> intRateConfigDetails, string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var interestRateConfigMaster = _mapper.Map<InterestRateConfigMaster>(intRateConfigMaster);
                interestRateConfigMaster.ProductGroupCode = ProductGroupCode.TIDE.Code;
                interestRateConfigMaster.DebitCreditFlag = "C";
                interestRateConfigMaster.InterestRate = 0;
                interestRateConfigMaster.PenalRate = 0;
                interestRateConfigMaster.AmountSlab = intRateConfigMaster.AmoutSlab.Value;
                interestRateConfigMaster.TenorSerialNo = 1;
                interestRateConfigMaster.IntRateType = "0";
                interestRateConfigMaster.SpreadRate = 0;
                interestRateConfigMaster.CreatedBy = userId;
                interestRateConfigMaster.CreatedDate = DateTime.UtcNow;
                interestRateConfigMaster.Status = 1;
                interestRateConfigMaster.StatusUpdateCore = 0;
                // Phần này đang chờ intellect phản hồi lấy giá trị hoặc trả về mặc định là 1
                interestRateConfigMaster.RecordSerialNo = 1;
                interestRateConfigMaster.ExpiryDate = intRateConfigMaster.ExpiredDate;

                // Cac truong can lay thong tin
                interestRateConfigMaster.PosName = _dbContext.ListOfPoss.Where(w => w.Code == interestRateConfigMaster.PosCode).FirstOrDefault()?.Name;
                interestRateConfigMaster.ProductName = _dbContext.ListOfProducts.Where(w => w.ProductCode == interestRateConfigMaster.ProductCode).FirstOrDefault()?.ProductName;
                interestRateConfigMaster.AccountTypeName = _dbContext.ListOfProducts.Where(w => w.AccountTypeCode == interestRateConfigMaster.AccountTypeCode).FirstOrDefault()?.AccountTypeName;
                interestRateConfigMaster.AccountSubTypeName = "0";

                _dbContext.InterestRateConfigMasters.Add(interestRateConfigMaster);
                await _dbContext.SaveChangesAsync();
                // Lưu InterestRateConfigDetail
                if (intRateConfigDetails != null && intRateConfigDetails.Any())
                {
                    var interestRateConfigDetailsList = intRateConfigDetails.Select(detail =>
                    {
                        var entity = _mapper.Map<InterestRateTermDetail>(detail);
                        entity.Id = 0;
                        entity.IntRateConfigId = interestRateConfigMaster.Id;
                        entity.CreatedBy = userId;
                        entity.CreatedDate = DateTime.UtcNow;
                        entity.Status = 1;
                        entity.Serial = detail.TermSerial ?? 0;
                        entity.IntRate = detail.TermIntRate;
                        entity.IntRateNew = detail.TermIntRateNew;
                        entity.IntRateType = detail.TermIntType;
                        entity.Spread = detail.TermSpreadRate;
                        return entity;
                    }).ToList();
                    // _dbContext.InterestRateTermDetails.AddRange(interestRateConfigDetails);
                    _dbContext.InterestRateTermDetails.AddRange(interestRateConfigDetailsList);
                    await _dbContext.SaveChangesAsync();
                }
                await transaction.CommitAsync();
                return "Success";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"An error occurred: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }


        public async Task<string> SaveBatchTideRateConfigureData(TideRateConfigureViewModel intRateConfigMaster, List<TideTermViewModel> intRateConfigDetails, string userId, string userPosCode)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var _lstAccountTypes = intRateConfigDetails.Select(s => s.TermAccountTypeCode).Distinct().ToList();

                for (int i = 0; i < _lstAccountTypes.Count; i++)
                {

                    var _accountTypeItem = _dbContext.ListOfProducts.Where(w => w.AccountTypeCode == _lstAccountTypes[i]).FirstOrDefault();
                    // Lưu InterestRateConfigMaster
                    var interestRateConfigMaster = _mapper.Map<InterestRateConfigMaster>(intRateConfigMaster);

                    interestRateConfigMaster.PosCode = string.IsNullOrEmpty(intRateConfigMaster.PosCode) || intRateConfigMaster.PosCode == "0" ? PosValue.HEAD_POS : intRateConfigMaster.PosCode;
                    interestRateConfigMaster.ProductGroupCode = ProductGroupCode.TIDE.Code; // Mã nhóm sản phẩm TIDE
                    interestRateConfigMaster.DebitCreditFlag = "C";
                    interestRateConfigMaster.InterestRate = 0;
                    interestRateConfigMaster.PenalRate = 0;
                    interestRateConfigMaster.AmountSlab = intRateConfigMaster.AmoutSlab.Value;
                    interestRateConfigMaster.TenorSerialNo = 1;
                    interestRateConfigMaster.IntRateType = "0";
                    interestRateConfigMaster.SpreadRate = 0;
                    interestRateConfigMaster.CreatedBy = userId;
                    interestRateConfigMaster.CreatedDate = DateTime.UtcNow;
                    interestRateConfigMaster.Status = 1; // Active
                    interestRateConfigMaster.StatusUpdateCore = 0;
                    interestRateConfigMaster.ExpiryDate = intRateConfigMaster.ExpiredDate;


                    // Cac truong can lay thong tin
                    interestRateConfigMaster.PosName = _dbContext.ListOfPoss.Where(w => w.Code == interestRateConfigMaster.PosCode).FirstOrDefault()?.Name;
                    interestRateConfigMaster.ProductCode = _accountTypeItem.ProductCode;
                    interestRateConfigMaster.AccountTypeCode = _accountTypeItem.AccountTypeCode;
                    interestRateConfigMaster.ProductName = _dbContext.ListOfProducts.Where(w => w.ProductCode == _accountTypeItem.ProductCode).FirstOrDefault()?.ProductName;
                    interestRateConfigMaster.AccountTypeName = _accountTypeItem?.AccountTypeName;
                    interestRateConfigMaster.AccountSubTypeCode = "0";
                    interestRateConfigMaster.AccountSubTypeName = "0";

                    _dbContext.InterestRateConfigMasters.Add(interestRateConfigMaster);
                    var _rowInsertCnt = await _dbContext.SaveChangesAsync();

                    // Lưu InterestRateConfigDetail
                    if (intRateConfigDetails != null && intRateConfigDetails.Any())
                    {
                        var interestRateConfigDetails = intRateConfigDetails.Where(w => w.TermAccountTypeCode == _lstAccountTypes[i]).Select(detail =>
                        {
                            var entity = _mapper.Map<InterestRateTermDetail>(detail);
                            entity.Id = 0; // Đảm bảo Id là 0 để EF Core tạo mới    
                            entity.IntRateConfigId = interestRateConfigMaster.Id; // Liên kết với Master vừa tạo
                            entity.CreatedBy = userId;
                            entity.CreatedDate = DateTime.UtcNow;
                            entity.Status = 1; // Active
                            entity.Serial = detail.TermSerial ?? 0;
                            entity.IntRate = detail.TermIntRate;

                            //Bổ sung phần validation khi nhập lãi suất
                            decimal minInterestRate = entity.IntRate - entity.MinInterestRateSpread ?? 0;
                            decimal maxInterestRate = entity.IntRate + entity.MaxInterestRateSpread ?? 0;

                            if (userPosCode != PosValue.HEAD_POS && ( detail.TermIntRateNew < minInterestRate || detail.TermIntRateNew > maxInterestRate))
                            {
                                Console.WriteLine($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
                                throw new Exception($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
                            }
                            entity.IntRateNew = detail.TermIntRateNew;
                            entity.IntRateType = detail.TermIntType;
                            entity.Spread = detail.TermSpreadRate;
                            entity.ChangeFlag = detail.ChangeFlag ?? 0;

                            return entity;
                        }).ToList();

                        _dbContext.InterestRateTermDetails.AddRange(interestRateConfigDetails);

                    }

                    if (intRateConfigMaster.ApplyPosList != null && intRateConfigMaster.ApplyPosList.Count > 0)
                    {
                        for (int j = 0; j < intRateConfigMaster.ApplyPosList.Count; j++)
                        {
                            var _itemPos = new InterestRatePosApply()
                            {
                                IntRateConfigId = interestRateConfigMaster.Id,
                                PosCode = intRateConfigMaster.ApplyPosList[j],
                                CreatedBy = userId,
                                CreatedDate = DateTime.UtcNow,
                                Status = 1
                            };
                            _dbContext.InterestRatePosApplys.Add(_itemPos);
                        }
                    }

                }

                var _rowUpdateCnt = await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return $"{_rowUpdateCnt}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"An error occurred: {ex.Message}");
                //return $"Lỗi: {ex.Message}";
                throw ex;
            }
        }

        public async Task<string> DeleteInterestRateConfigureById(long id, string userName)
        {
            // using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var item = _dbContext.InterestRateConfigMasters.Where(w => w.Id == id).FirstOrDefault();
                if (item != null)
                {
                    item.Status = StatusLov.StatusClosed;
                    item.ModifiedBy = userName;
                    item.ModifiedDate = DateTime.UtcNow;

                    var interestRateConfigDetails = _dbContext.InterestRateTermDetails.Where(w => w.IntRateConfigId == id).ToList();
                    for (int i = 0; i < interestRateConfigDetails.Count; i++)
                    {
                        interestRateConfigDetails[i].Status = StatusLov.StatusClosed;
                        interestRateConfigDetails[i].CreatedBy = userName;
                        interestRateConfigDetails[i].CreatedDate = DateTime.UtcNow;
                    }
                    _dbContext.InterestRateTermDetails.UpdateRange(interestRateConfigDetails);
                    _dbContext.InterestRateConfigMasters.Update(item);

                    int result = await _dbContext.SaveChangesAsync();

                    //   await transaction.CommitAsync();
                    return result.ToString();
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return "0";
            }
        }

        public async Task<string> DeleteInterestRateConfigureByDocumentId(long documentId, string userName)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var lstConfigure = _dbContext.InterestRateConfigMasters.Where(w => w.DocumentId == documentId).ToList();
                if (lstConfigure != null && lstConfigure.Count > 0)
                {
                    for (int i = 0; i < lstConfigure.Count; i++)
                    {
                        lstConfigure[i].Status = StatusLov.StatusClosed;
                        lstConfigure[i].ModifiedBy = userName;
                        lstConfigure[i].ModifiedDate = DateTime.UtcNow;


                        var interestRateConfigDetails = _dbContext.InterestRateTermDetails.Where(w => w.IntRateConfigId == lstConfigure[i].Id).ToList();
                        for (int j = 0; j < interestRateConfigDetails.Count; j++)
                        {
                            interestRateConfigDetails[j].Status = StatusLov.StatusClosed;
                            interestRateConfigDetails[j].CreatedBy = userName;
                            interestRateConfigDetails[j].CreatedDate = DateTime.UtcNow;
                        }
                        _dbContext.InterestRateTermDetails.UpdateRange(interestRateConfigDetails);
                        await _dbContext.SaveChangesAsync();

                    }

                    _dbContext.InterestRateConfigMasters.UpdateRange(lstConfigure);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return "Xóa thành công!";
                }
                else
                {
                    return $"Không tồn tại bản ghi có DocumentId={documentId}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> CloseCasaById(long id, string userName)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var item = await _dbContext.InterestRateConfigMasters.FirstOrDefaultAsync(w => w.Id == id);
                if (item != null)
                {
                    item.Status = StatusLov.StatusClosed;
                    item.ModifiedBy = userName;
                    item.ModifiedDate = DateTime.UtcNow;

                    _dbContext.InterestRateConfigMasters.Update(item);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return "Đóng Casa thành công!";
                }
                else
                {
                    return $"Không tồn tại bản ghi có Id={id}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                await transaction.RollbackAsync();
                return $"Lỗi: {ex.Message}";
            }
        }

        public async Task<string> CloseCasaByDocumentId(long documentId, string userName)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var lstConfigure = await _dbContext.InterestRateConfigMasters
                    .Where(w => w.DocumentId == documentId)
                    .ToListAsync();
                if (lstConfigure != null && lstConfigure.Count > 0)
                {
                    for (int i = 0; i < lstConfigure.Count; i++)
                    {
                        lstConfigure[i].Status = StatusLov.StatusClosed;
                        lstConfigure[i].ModifiedBy = userName;
                        lstConfigure[i].ModifiedDate = DateTime.UtcNow;
                    }

                    _dbContext.InterestRateConfigMasters.UpdateRange(lstConfigure);
                    await _dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return "Đóng Casa thành công!";
                }
                else
                {
                    return $"Không tồn tại bản ghi có DocumentId={documentId}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
                await transaction.RollbackAsync();
                return $"Lỗi: {ex.Message}";
            }
        }

        //public async Task<string> SaveCasaRateConfigureData(AddCasaProductViewModel master, List<CasaRateProductViewModel> details, string userId)
        //{
        //    if (details == null || !details.Any())
        //        return "Error: Không có dữ liệu chi tiết để lưu.";
        //    if (master.ApplyPosList == null || !master.ApplyPosList.Any())
        //        return "Error: Vui lòng chọn ít nhất một POS áp dụng.";
        //    if (string.IsNullOrWhiteSpace(master.CircularRefNum))
        //        return "Error: Số Quyết định không được để trống.";

        //    using var transaction = await _dbContext.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Lấy danh sách AccountType duy nhất từ details
        //        var lstAccountTypes = details.Select(d => d.RateProductAccountTypeCode).Distinct().ToList();

        //        long documentId = master.DocumentId ?? await CreateNewDocumentId();

        //        var effectiveDate = master.EffectiveDate ?? DateTime.Today.AddDays(1);

        //        // Lấy POS đầu tiên 
        //        string firstPosCode = master.ApplyPosList.FirstOrDefault() ?? "0";
        //        string firstPosName = "Toàn hệ thống";

        //        if (firstPosCode != "0")
        //        {
        //            var firstPos = _dbContext.ListOfPoss
        //                .FirstOrDefault(w => w.Code == firstPosCode);

        //            firstPosName = firstPos?.Name ?? firstPosCode; // Tên nếu có, không thì hiện mã
        //        }

        //        foreach (var accountTypeCode in lstAccountTypes)
        //        {
        //            var detailItem = details.First(d => d.RateProductAccountTypeCode == accountTypeCode);


        //            var accountTypeItem = _dbContext.ListOfProducts
        //                .FirstOrDefault(w => w.AccountTypeCode == accountTypeCode);

        //            if (accountTypeItem == null)
        //                continue; // Skip nếu không tìm thấy


        //            //Bổ sung phần validation khi nhập lãi suất
        //            decimal minInterestRate = detailItem.InterestRateHO - detailItem.MinInterestRateSpread ;
        //            decimal maxInterestRate = detailItem.InterestRateHO + detailItem.MaxInterestRateSpread ;

        //            if (detailItem.RateProductNewInterestRate < minInterestRate || detailItem.RateProductNewInterestRate > maxInterestRate)
        //            {
        //                Console.WriteLine($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
        //                throw new Exception($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
        //            }

        //            var configMaster = new InterestRateConfigMaster
        //            {
        //                ProductGroupCode = ProductGroupCode.CASA.Code,
        //                DebitCreditFlag = "C",
        //                PosCode = firstPosCode, // Luôn là POS đầu tiên
        //                PosName = firstPosName, // Luôn là tên POS đầu tiên → Index sẽ hiện cái này
        //                ProductCode = accountTypeItem.ProductCode,
        //                ProductName = _dbContext.ListOfProducts
        //                    .FirstOrDefault(w => w.ProductCode == accountTypeItem.ProductCode)?.ProductName,
        //                AccountTypeCode = accountTypeItem.AccountTypeCode,
        //                AccountTypeName = accountTypeItem.AccountTypeName,
        //                AccountSubTypeCode = "0",
        //                AccountSubTypeName = "0",
        //                CurrencyCode = "VND",
        //                InterestRate = detailItem.RateProductInterestRate ?? 0m,
        //                NewInterestRate = detailItem.RateProductNewInterestRate ?? 0m,
        //                PenalRate = detailItem.RateProductPenalRate ?? 0m,
        //                EffectiveDate = effectiveDate,
        //                ExpiryDate = master.ExpiredDate ?? new DateTime(2050, 12, 31),
        //                CircularRefNum = master.CircularRefNum.Trim(),
        //                CircularDate = master.CircularDate,
        //                AmountSlab = 0m,
        //                TenorSerialNo = 1,
        //                IntRateType = "0",
        //                SpreadRate = 0,
        //                CreatedBy = userId ?? "System",
        //                CreatedDate = DateTime.UtcNow,
        //                Status = 1,
        //                StatusUpdateCore = 0,
        //                DocumentId = documentId
        //            };

        //            _dbContext.InterestRateConfigMasters.Add(configMaster);
        //            await _dbContext.SaveChangesAsync(); // Lưu để có Id

        //            // Lưu tất cả POS áp dụng vào bảng InterestRatePosApply 
        //            var posApplies = master.ApplyPosList.Select(pos => new InterestRatePosApply
        //            {
        //                IntRateConfigId = configMaster.Id,
        //                PosCode = pos.Trim(),
        //                CreatedBy = userId ?? "System",
        //                CreatedDate = DateTime.UtcNow,
        //                Status = 1
        //            }).ToList();

        //            if (posApplies.Any())
        //            {
        //                _dbContext.InterestRatePosApplys.AddRange(posApplies);
        //            }
        //        }

        //        await _dbContext.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        _logger.LogInformation($"[CASA SAVE SUCCESS] Saved {lstAccountTypes.Count} records, DocumentId: {documentId}");
        //        return "Success";
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "[CASA SAVE ERROR]");
        //        return $"Error: {ex.Message}";
        //    }
        //}


        // thay đổi phần hiển thị khi chọn Toàn hàng 
        public async Task<string> SaveCasaRateConfigureData(AddCasaProductViewModel master, List<CasaRateProductViewModel> details, string userId)
        {
            if (details == null || !details.Any())
                return "Error: Không có dữ liệu chi tiết để lưu.";
            if (master.ApplyPosList == null || !master.ApplyPosList.Any())
                return "Error: Vui lòng chọn ít nhất một POS áp dụng.";
            if (string.IsNullOrWhiteSpace(master.CircularRefNum))
                return "Error: Số Quyết định không được để trống.";

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Lấy danh sách AccountType duy nhất từ details
                var lstAccountTypes = details.Select(d => d.RateProductAccountTypeCode).Distinct().ToList();
                long documentId = master.DocumentId ?? await CreateNewDocumentId();
                var effectiveDate = master.EffectiveDate ?? DateTime.Today.AddDays(1);

                // === PHẦN SỬA: Xác định PosCode và PosName 
                string posCodeToSave;
                string posNameToSave;

                const string HEAD_OFFICE_CODE = "000100";  // 


                bool isWholeBankSelected = master.ApplyPosList.Any(p =>
                    p != null && (
                        string.Equals(p.Trim(), "0", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Trim(), "ALL", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Trim(), PosValue.SYSTEM_WIDE, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Trim(), PosValue.BANK_WIDE, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Trim(), "999999", StringComparison.OrdinalIgnoreCase)
                    )
                );

                if (isWholeBankSelected)
                {
                    posCodeToSave = HEAD_OFFICE_CODE;

                    // Lấy tên chính xác từ bảng ListOfPoss
                    var headOffice = _dbContext.ListOfPoss
                        .FirstOrDefault(w => w.Code == HEAD_OFFICE_CODE);

                    posNameToSave = headOffice?.Name ?? "Ngân hàng CSXH";  // fallback nếu không tìm thấy
                }
                else
                {
                    // Trường hợp chỉ chọn POS cụ thể → lấy POS đầu tiên
                    posCodeToSave = master.ApplyPosList.FirstOrDefault()?.Trim() ?? "0";
                    var posEntity = _dbContext.ListOfPoss
                        .FirstOrDefault(w => w.Code == posCodeToSave);

                    posNameToSave = posEntity?.Name ?? posCodeToSave;
                }

                foreach (var accountTypeCode in lstAccountTypes)
                {
                    var detailItem = details.First(d => d.RateProductAccountTypeCode == accountTypeCode);

                    var accountTypeItem = _dbContext.ListOfProducts
                        .FirstOrDefault(w => w.AccountTypeCode == accountTypeCode);

                    if (accountTypeItem == null)
                        continue;

                    // Bổ sung phần validation khi nhập lãi suất
                    decimal minInterestRate = detailItem.InterestRateHO - detailItem.MinInterestRateSpread;
                    decimal maxInterestRate = detailItem.InterestRateHO + detailItem.MaxInterestRateSpread;

                    if (detailItem.RateProductNewInterestRate < minInterestRate ||
                        detailItem.RateProductNewInterestRate > maxInterestRate)
                    {
                        Console.WriteLine($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
                        throw new Exception($"Lãi suất cấu hình không nằm trong khoảng [{minInterestRate}, {maxInterestRate}]");
                    }

                    var configMaster = new InterestRateConfigMaster
                    {
                        ProductGroupCode = ProductGroupCode.CASA.Code,
                        DebitCreditFlag = "C",
                        PosCode = posCodeToSave,          // Đã sửa
                        PosName = posNameToSave,          // Đã sửa
                        ProductCode = accountTypeItem.ProductCode,
                        ProductName = accountTypeItem.ProductName,
                        AccountTypeCode = accountTypeItem.AccountTypeCode,
                        AccountTypeName = accountTypeItem.AccountTypeName,
                        AccountSubTypeCode = "0",
                        AccountSubTypeName = "0",
                        CurrencyCode = "VND",
                        InterestRate = detailItem.RateProductInterestRate ?? 0m,
                        NewInterestRate = detailItem.RateProductNewInterestRate ?? 0m,
                        PenalRate = detailItem.RateProductPenalRate ?? 0m,
                        EffectiveDate = effectiveDate,
                        ExpiryDate = master.ExpiredDate ?? new DateTime(2050, 12, 31),
                        CircularRefNum = master.CircularRefNum.Trim(),
                        CircularDate = master.CircularDate,
                        AmountSlab = 0m,
                        TenorSerialNo = 1,
                        IntRateType = "0",
                        SpreadRate = 0,
                        CreatedBy = userId ?? "System",
                        CreatedDate = DateTime.UtcNow,
                        Status = 1,
                        StatusUpdateCore = 0,
                        DocumentId = documentId
                    };

                    _dbContext.InterestRateConfigMasters.Add(configMaster);
                    await _dbContext.SaveChangesAsync(); // Lưu để có Id

                    // Lưu tất cả POS áp dụng vào bảng InterestRatePosApply
                    var posApplies = master.ApplyPosList.Select(pos => new InterestRatePosApply
                    {
                        IntRateConfigId = configMaster.Id,
                        PosCode = pos.Trim(),
                        CreatedBy = userId ?? "System",
                        CreatedDate = DateTime.UtcNow,
                        Status = 1
                    }).ToList();

                    if (posApplies.Any())
                    {
                        _dbContext.InterestRatePosApplys.AddRange(posApplies);
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"[CASA SAVE SUCCESS] Saved {lstAccountTypes.Count} records, DocumentId: {documentId}");
                return "Success";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "[CASA SAVE ERROR]");
                //return $"Error: {ex.Message}";
                throw ex;
            }
        }


        public async Task<InterestRateConfigMasterModel> GetCasaByIdAsync(long id)
        {
            try
            {
                var item = await _dbContext.InterestRateConfigMasters
                    .Where(x => x.Id == id && x.ProductGroupCode == ProductGroupCode.CASA.Code)
                    .Select(x => new InterestRateConfigMasterModel
                    {
                        Id = x.Id,
                        PosCode = x.PosCode ?? "0",
                        PosName = x.PosName ?? "",
                        ProductCode = x.ProductCode ?? "",
                        ProductName = x.ProductName ?? "",
                        AccountTypeCode = x.AccountTypeCode ?? "",
                        AccountTypeName = x.AccountTypeName ?? "",
                        AccountSubTypeCode = x.AccountSubTypeCode ?? "0",
                        CircularRefNum = x.CircularRefNum ?? "",
                        InterestRate = x.InterestRate,
                        NewInterestRate = x.NewInterestRate,
                        PenalRate = x.PenalRate,
                        EffectiveDate = x.EffectiveDate,
                        ExpiryDate = x.ExpiryDate,
                        DocumentId = x.DocumentId,
                        Status = x.Status,
                        CircularDate = x.CircularDate,
                        AmountSlab = x.AmountSlab,
                        TenorSerialNo = x.TenorSerialNo,
                        IntRateType = x.IntRateType,
                        SpreadRate = x.SpreadRate,
                        Remark = x.Remark,
                        OrtherNotes = x.OrtherNotes,
                        StatusUpdateCore = x.StatusUpdateCore,
                        CallApiTxnStatus = x.CallApiTxnStatus,
                        CallApiReqRecordSl = x.CallApiReqRecordSl != null ? x.CallApiReqRecordSl.ToString() : null,
                        CallApiResponseCode = x.CallApiResponseCode,
                        CallApiResponseMsg = x.CallApiResponseMsg,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate,
                        ModifiedBy = x.ModifiedBy,
                        ModifiedDate = x.ModifiedDate,
                        ApproverBy = x.ApproverBy,
                        ApprovalDate = x.ApprovalDate
                    })
                    .FirstOrDefaultAsync();

                if (item != null)
                {
                    Console.WriteLine($"GetCasaByIdAsync success: Id={item.Id}, ProductCode='{item.ProductCode}', CircularRefNum='{item.CircularRefNum}', Status={item.Status}"); // DEBUG: Log nếu có data
                    return item;
                }
                else
                {
                    Console.WriteLine($"GetCasaByIdAsync no data for id={id}"); // DEBUG
                    return new InterestRateConfigMasterModel
                    {
                        Id = 0,
                        PosCode = "0",
                        ProductCode = "",
                        ProductName = "",
                        AccountTypeCode = "",
                        AccountTypeName = "",
                        AccountSubTypeCode = "0",
                        CircularRefNum = "",
                        InterestRate = 0m,
                        NewInterestRate = 0m,
                        PenalRate = 0m,
                        EffectiveDate = DateTime.Today,
                        ExpiryDate = DateTime.Today,
                        CircularDate = DateTime.Today,
                        Status = 1,
                        AmountSlab = 0m,
                        TenorSerialNo = 1,
                        IntRateType = "",
                        SpreadRate = 0m,
                        CallApiReqRecordSl = null
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCasaByIdAsync error for id={id}: {ex.Message}"); // DEBUG: Log exception
                throw new Exception($"Lỗi khi lấy chi tiết Casa: {ex.Message}", ex);
            }
        }


        public async Task<string> UpdateCasaAsync(InterestRateConfigMasterModel model, string userName)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var item = await _dbContext.InterestRateConfigMasters.FirstOrDefaultAsync(x => x.Id == model.Id && x.ProductGroupCode == ProductGroupCode.CASA.Code);
                DateTime dCurrentDate = DateTime.Now;
                if (item != null)
                {
                    item.PosCode = model.PosCode;
                    item.PosName = _dbContext.ListOfPoss.FirstOrDefault(w => w.Code == model.PosCode)?.Name ?? item.PosName;
                    item.ProductCode = model.ProductCode;
                    item.ProductName = _dbContext.ListOfProducts.FirstOrDefault(w => w.ProductCode == model.ProductCode)?.ProductName ?? item.ProductName;
                    item.AccountTypeName = _dbContext.ListOfProducts.FirstOrDefault(w => w.AccountTypeCode == model.AccountTypeName)?.AccountTypeName ?? item.AccountTypeName;
                    item.CircularRefNum = model.CircularRefNum;
                    item.InterestRate = model.InterestRate ?? 0;
                    item.NewInterestRate = model.NewInterestRate;
                    item.PenalRate = model.PenalRate;
                    item.EffectiveDate = model.EffectiveDate ?? DateTime.Today;  // FIX: Fallback Today thay throw
                    item.ExpiryDate = model.ExpiryDate;
                    item.DocumentId = model.DocumentId;
                    item.ModifiedBy = userName;
                    item.ModifiedDate = dCurrentDate;
                    item.StatusUpdateCore = 1;

                    _dbContext.InterestRateConfigMasters.Update(item);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return "Cập nhật Casa thành công!";
                }
                return $"Không tồn tại bản ghi Casa với Id={model.Id}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Lỗi khi cập nhật Casa: {ex.Message}");
                return $"Lỗi: {ex.Message}";
            }
        }

        public async Task<string> GetCircularRefNumByDocumentIdAsync(long documentId)
        {
            try
            {
                var circularRefNum = await _dbContext.InterestRateConfigMasters
                    .Where(m => m.DocumentId == documentId)
                    .Select(m => m.CircularRefNum)
                    .FirstOrDefaultAsync();
                Console.WriteLine($"Retrieved CircularRefNum: {circularRefNum} for DocumentId: {documentId}");
                return circularRefNum;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving CircularRefNum for DocumentId {documentId}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return null;
            }
        }

        //add
        public async Task UpdateCasaDocumentIdAsync(long casaId, long documentId)
        {
            try
            {
                var casa = await _dbContext.InterestRateConfigMasters.FirstOrDefaultAsync(x => x.Id == casaId && x.ProductGroupCode == ProductGroupCode.CASA.Code);
                if (casa == null)
                {
                    throw new Exception($"Không tìm thấy Casa với Id={casaId}");
                }
                DateTime dCurrentDate = DateTime.Now;
                casa.DocumentId = documentId;
                casa.ModifiedBy = "System";
                casa.ModifiedDate = dCurrentDate;
                _dbContext.InterestRateConfigMasters.Update(casa);
                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật DocumentId: {ex.Message}", ex);
            }
        }

        public async Task<(byte[] fileBytes, string fileName, string contentType)> GetAttachedFileForDownloadAsync(long fileId)
        {
            try
            {
                var fileInfo = await _dbContext.AttachedFileInfos
                    .FirstOrDefaultAsync(f => f.FileId == fileId && f.Status == 1);

                if (fileInfo == null)
                    throw new Exception("File không tồn tại hoặc đã bị xóa.");

                var filePath = fileInfo.PathFile;

                if (!System.IO.File.Exists(filePath))
                    throw new Exception("File không tồn tại trên server.");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = fileInfo.FileNameNew ?? fileInfo.FileName;
                var contentType = fileInfo.FileType ?? "application/octet-stream";

                // Optional: log download
                Console.WriteLine($"Download file success: FileId={fileId}, FileName={fileName}, Size={fileBytes.Length} bytes");

                return (fileBytes, fileName, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file FileId={fileId}: {ex.Message}");
                throw new Exception($"Lỗi khi tải file: {ex.Message}", ex);
            }
        }

        public async Task<AttachedFileInfo> GetAttachedFileById(long fileId)
        {
            try
            {
                var fileInfo = await _dbContext.AttachedFileInfos
                    .FirstOrDefaultAsync(f => f.FileId == fileId && f.Status == 1);

                if (fileInfo == null)
                    throw new Exception("File không tồn tại hoặc đã bị xóa.");

                return fileInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file FileId={fileId}: {ex.Message}");
                throw new Exception($"Lỗi khi tải file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hàm lấy danh sách các Quyết định cấu hình lãi suất trong bảng InterestRateConfigMaster (Lấy danh sách tổng hợp)
        /// </summary>
        /// <param name="pMainPosCode">Mã Chi nhánh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã POS (Không bắt buộc)</param>
        /// <param name="pProductGroupCode">Loại cấu hình: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pCircularRefNum">Số quyết định (Không bắt buộc) - Có thể tìm không dấu và không phân biệt chữ hoa, chữ thường</param>
        /// <param name="pFromEffectiveDate">Ngày hiệu lực - Từ ngày. Định dạng yyyyMMdd</param>
        /// <param name="pToEffectiveDate">Ngày hiệu lực - Đến ngày. Định dạng yyyyMMdd</param>
        /// <param name="pDocumentId">Chỉ số xác định tờ trình (Không bắt buộc)</param>
        /// <param name="pListId">Danh sách Id bảng DL truyền vào cách nhau bởi dấu chấm phẩy (Không bắt buộc)</param>
        /// <returns>Danh sách các Quyết định cấu hình lãi suất trong bảng InterestRateConfigMaster (Lấy danh sách tổng hợp)</returns>
        public List<InterestRateConfigMasterView> GetListInterestRateConfigMasterViews(string pMainPosCode, string pPosCode, string pProductGroupCode,
                        string pCircularRefNum, int pFromEffectiveDate, int pToEffectiveDate, long pDocumentId, string pListId)
        {
            try
            {
                List<InterestRateConfigMasterView> listIntRateConfigMasterViews = new List<InterestRateConfigMasterView>();
                DateTime dFromEffectiveDate = CustConverter.StringToDate(pFromEffectiveDate.ToString(), FormatParameters.FORMAT_DATE_INT);
                DateTime dToEffectiveDate = CustConverter.StringToDate(pToEffectiveDate.ToString(), FormatParameters.FORMAT_DATE_INT);
                listIntRateConfigMasterViews = _dbContext.InterestRateConfigMasterViews.Where(w => w.ProductGroupCode != ""
                        && (string.IsNullOrEmpty(pMainPosCode) || pMainPosCode == "000100" || (w.MainPosCode == pMainPosCode))
                        && (string.IsNullOrEmpty(pPosCode) || pPosCode == "000100" || (w.PosCode == pPosCode))
                        && (string.IsNullOrEmpty(pListId) || w.IdList == pListId)
                        && (string.IsNullOrEmpty(pProductGroupCode) || w.ProductGroupCode == pProductGroupCode)
                        && (pDocumentId == 0 || w.DocumentId == pDocumentId)
                        && (w.EffectiveDate >= dFromEffectiveDate.Date && w.EffectiveDate <= dToEffectiveDate.Date)
                       )
                       .Where(delegate (InterestRateConfigMasterView c)
                       {
                           if (string.IsNullOrEmpty(pCircularRefNum)
                               || (c.CircularRefNum != null && c.CircularRefNum.ToLower().Contains(pCircularRefNum.ToLower()))
                               || (c.CircularRefNum != null && Utilities.ConvertToUnSign(c.CircularRefNum.ToLower()).IndexOf(pCircularRefNum.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                               )
                               return true;
                           else
                               return false;
                       })
                       .OrderByDescending(o => o.PosCode).ThenBy(o => o.EffectiveDate).ToList();

                if (listIntRateConfigMasterViews != null && listIntRateConfigMasterViews.Count != 0)
                {
                    int iCountTemp = 0;
                    foreach (var item in listIntRateConfigMasterViews)
                    {
                        iCountTemp++;
                        item.OrderNo = iCountTemp;
                        //item.StatusDesc = ConfigStatus.GetByValue(item.Status).Description;
                    }
                }
                return listIntRateConfigMasterViews;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<InterestRateConfigMasterViewModel> GetListInterestRateConfigMasterViewsForTide(string userPos, string pPosCode, string pProductGroupCode,
                        string pCircularRefNum, int pFromEffectiveDate, int pToEffectiveDate, long pDocumentId, string searchText, int status)
        {
            try
            {
                List<InterestRateConfigMasterView> listIntRateConfigMasterViews = GetListInterestRateConfigMasterViews("", pPosCode,
                    pProductGroupCode, pCircularRefNum, pFromEffectiveDate, pToEffectiveDate, pDocumentId, "");

                //Lọc thêm: trường hợp user ban khnv không hiển thị các tờ trình của chi nhánh mà chưa phê duyệt
                if (userPos == "000100")
                    listIntRateConfigMasterViews.RemoveAll(item => item.PosCode != "000100" && item.Status == 1);

                if (status != -1)
                {
                    listIntRateConfigMasterViews.RemoveAll(item => item.Status != status);
                }


                List<InterestRateConfigMasterViewModel> data;
                if (!string.IsNullOrEmpty((searchText)))
                {
                    var listSearchResult = listIntRateConfigMasterViews.Where(w => w.ProductCode.Contains(searchText)
                        || w.CircularRefNum.ToLower().Contains(searchText)
                        || w.AccountTypeList.ToLower().Contains(searchText)).ToList();

                    data = _mapper.Map<List<InterestRateConfigMasterViewModel>>(listSearchResult);
                }
                else
                {
                    data = _mapper.Map<List<InterestRateConfigMasterViewModel>>(listIntRateConfigMasterViews);
                }

                foreach (InterestRateConfigMasterViewModel item in data)
                {
                    item.StatusDesc = ConfigStatus.GetByValue(item.Status).Description;
                }

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<List<TideTermViewModel>> GetTideTermsAsync(List<string> intRateConfigIds)
        {
            try
            {
                // Chuyển đổi string → long (bỏ qua giá trị không hợp lệ)
                var longIds = intRateConfigIds
                            .Where(id => long.TryParse(id, out _))
                            .Select(long.Parse)
                            .ToList();

                if (!longIds.Any())
                    return new List<TideTermViewModel>();

                var query =
                    from term in _dbContext.InterestRateTermDetails
                    join master in _dbContext.InterestRateConfigMasters
                          on term.IntRateConfigId equals master.Id
                    where longIds.Contains(term.IntRateConfigId)
                    orderby master.AccountTypeCode, term.Serial
                    select new TideTermViewModel
                    {
                        // Từ InterestRateTermDetail
                        Id = term.Id,
                        TermSerial = term.Serial,
                        TermDesc = term.TermDesc ?? "",
                        TermValue = term.TermValue,
                        TermUnit = term.TermUnit,
                        InclusionFlag = term.InclusionFlag,
                        TermIntRate = term.IntRate,
                        TermIntRateNew = term.IntRateNew,
                        TermIntType = term.IntRateType ?? "",
                        TermSpreadRate = term.Spread,

                        // Từ InterestRateConfigMaster
                        TermProductCode = master.ProductCode,
                        TermProductName = master.ProductName,
                        TermAccountTypeCode = master.AccountTypeCode,
                        TermAccountTypeName = master.AccountTypeName,
                        TermAccountSubTypeCode = master.AccountSubTypeCode,
                        TermEffectiveDate = master.EffectiveDate,
                        TermPosCode = master.PosCode,
                        TermAmoutSlab = master.AmountSlab,
                        TermCurrencyCode = master.CurrencyCode,

                        // Giá trị mặc định
                        ChangeFlag = term.ChangeFlag ?? 0
                    };

                List<TideTermViewModel> data = await query.ToListAsync();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<InterestRateConfigMasterViewModel> GetTideInterestRateDetailViews(string circularRefNum, string effectDate)
        {
            try
            {
                var effectDateInt = int.Parse(effectDate);
                var data = GetListInterestRateConfigMasterViews("", "", "", circularRefNum, effectDateInt, effectDateInt, 0, "").FirstOrDefault();
                InterestRateConfigMasterViewModel result = _mapper.Map<InterestRateConfigMasterViewModel>(data);
                result.StatusDesc = ConfigStatus.GetByValue(result.Status).Description;
                result.StatusUpdateCoreDesc = UpdateStatusValue.GetByValue(result.StatusUpdateCore).Description;

                //Lay phan pos apply
                var lstIds = StringHelper.ConvertToLongList(result.IdList, ';');
                var lstApplyPos = _dbContext.InterestRatePosApplys.Where(w => lstIds.Contains(w.IntRateConfigId)).Select(p => p.PosCode).Distinct().ToList();

                var lstPosInfo = _dbContext.ListOfPoss.Where(w => lstApplyPos.Contains(w.Code)).OrderBy(o => o.Code).ToList();
                string posNameList = "";
                for (int i = 0; i < lstPosInfo.Count; i++)
                {
                    if (i == 0)
                    {
                        posNameList = lstPosInfo[i].Name;
                    }
                    else
                    {
                        posNameList = posNameList + "," + lstPosInfo[i].Name;
                    }
                }
                result.ApplyPosList = posNameList;


                var lstRemark = _dbContext.InterestRateConfigMasters.Where(w => lstIds.Contains(w.Id)).Select(p => p.Remark).Distinct().ToList();
                string remarkList = "";
                for (int i = 0; i < lstRemark.Count; i++)
                {
                    if (i == 0)
                    {
                        remarkList = lstRemark[i];
                    }
                    else
                    {
                        remarkList = remarkList + "," + lstRemark[i];
                    }
                }
                result.Remark = remarkList;

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circularRefNum"></param>
        /// <param name="effectDate"></param>
        /// <returns></returns>
        public async Task<InterestRateConfigMasterViewModel> GetCasaInterestRateDetailViews(string circularRefNum, string effectDate)
        {
            try
            {
                if (string.IsNullOrEmpty(circularRefNum))
                {
                    // WriteLog(LogType.ERROR, "GetCasaInterestRateDetailViews: circularRefNum is empty");
                    return null;
                }
                //
                var effectDateInt = int.Parse(effectDate);
                var data = GetListInterestRateConfigMasterViews("", "", ProductGroupCode.CASA.Code, circularRefNum, effectDateInt, effectDateInt, 0, "").FirstOrDefault();
                if (data == null)
                {
                    //WriteLog(LogType.ERROR, $"GetCasaInterestRateDetailViews: No data for circularRefNum={circularRefNum} in CASA");
                    return null;
                }

                var result = _mapper.Map<InterestRateConfigMasterViewModel>(data);
                if (result == null)
                {
                    //WriteLog(LogType.ERROR, "GetCasaInterestRateDetailViews: Mapping failed");
                    return null;
                }

                result.StatusDesc = ConfigStatus.GetByValue(result.Status).Description; // Giả sử ConfigStatus có sẵn
                                                                                        // dùng IdList (giả sử View đã có IdList concatenated, nếu không → build từ data)
                var lstIds = StringHelper.ConvertToLongList(result.IdList ?? "", ';'); // Fallback empty
                if (string.IsNullOrEmpty(result.IdList) && data != null) // 
                {
                    //
                    var allMasters = await _dbContext.InterestRateConfigMasters
                        .Where(m => m.CircularRefNum == circularRefNum && m.ProductGroupCode == ProductGroupCode.CASA.Code)
                        .Select(m => m.Id.ToString())
                        .ToListAsync();
                    result.IdList = string.Join(";", allMasters); // Build "1;2;3"
                    lstIds = allMasters.Select(long.Parse).ToList();
                    //  WriteLog(LogType.INFOR, $"Built IdList='{result.IdList}' from {lstIds.Count} masters for circular={circularRefNum}");
                }
                var lstApplyPos = _dbContext.InterestRatePosApplys.Where(w => lstIds.Contains(w.IntRateConfigId)).Select(p => p.PosCode).Distinct().ToList();
                var lstPosInfo = _dbContext.ListOfPoss.Where(w => lstApplyPos.Contains(w.Code)).OrderBy(o => o.Code).ToList();
                string posNameList = "";
                for (int i = 0; i < lstPosInfo.Count; i++)
                {
                    if (i == 0)
                    {
                        posNameList = lstPosInfo[i].Name;
                    }
                    else
                    {
                        posNameList = posNameList + "," + lstPosInfo[i].Name;
                    }
                }
                result.ApplyPosList = posNameList;
                // WriteLog(LogType.INFOR, $"GetCasaInterestRateDetailViews success: {result.CircularRefNum}, ApplyPosList={posNameList}, IdList={result.IdList}");
                return result;
            }
            catch (Exception ex)
            {
                // WriteLog(LogType.ERROR, $"Error in GetCasaInterestRateDetailViews: {ex.Message}");
                throw;
            }
        }

        public async Task<(AddCasaProductViewModel summaryModel, List<long> ids)> GetCasaMasterListByFilterAsync(
    string productGroupCode,
    string posCode,
    string productCode,
    string circularRefNum,
    DateTime? fromDate,
    DateTime? toDate,
    bool isViewModel = false)
        {
            try
            {
                Console.WriteLine($"GetCasaMasterListByFilterAsync start: productGroupCode={productGroupCode}, posCode={posCode}, circularRefNum='{circularRefNum}', fromDate={fromDate}, toDate={toDate}");

                // Sửa dòng gọi service: truyền thêm null cho searchText và statusDesc
                var dataList = await GetInterestRateConfigMasterViewListAsync(
                    productGroupCode,
                    posCode,
                    productCode,
                    circularRefNum,
                    fromDate,
                    toDate,
                    null,               // searchText = null (không lọc text)
                    null                // statusDesc = null (không lọc trạng thái)
                );

                Console.WriteLine($"Raw dataList.Count: {dataList?.Count ?? 0}");

                if (dataList == null || !dataList.Any())
                {
                    return (null, new List<long>());
                }

                // Giới hạn 10 bản ghi để test (giữ nguyên)
                dataList = dataList.OrderByDescending(d => d.Id).Take(10).ToList();
                Console.WriteLine($"DataList count after limit: {dataList.Count}");

                var firstData = dataList.First();
                Console.WriteLine($"FirstData debug: Id={firstData.Id}, ProductCode='{firstData.ProductCode}', CircularRefNum='{firstData.CircularRefNum}', EffectiveDate={firstData.EffectiveDate}");

                var summary = new AddCasaProductViewModel
                {
                    Id = firstData.Id,
                    ProductCode = firstData.ProductCode ?? "",
                    ProductName = firstData.ProductName ?? "",
                    AccountTypeCode = firstData.AccountTypeCode ?? "",
                    AccountTypeName = firstData.AccountTypeName ?? "",
                    AccountSubTypeCode = firstData.AccountSubTypeCode ?? "0",
                    CurrencyCode = firstData.CurrencyCode ?? "VND",
                    DebitCreditFlag = firstData.DebitCreditFlag ?? "C",
                    EffectiveDate = firstData.EffectiveDate,
                    ExpiredDate = firstData.ExpiryDate,
                    CircularRefNum = firstData.CircularRefNum ?? "",
                    CircularDate = firstData.CircularDate.GetValueOrDefault(DateTime.Today),
                    PosCode = firstData.PosCode ?? "",
                    InterestRate = firstData.InterestRate,
                    NewInterestRate = firstData.NewInterestRate,
                    PenalRate = firstData.PenalRate,
                    AmoutSlab = firstData.AmountSlab,
                    DocumentId = firstData.DocumentId
                };

                var ids = dataList.Select(d => d.Id).ToList();

                return (summary, ids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR: GetCasaMasterListByFilterAsync error: {ex.Message} | StackTrace: {ex.StackTrace}");
                throw;
            }
        }



        // THÊM: Batch update DocumentId (đã gợi ý trước)
        public async Task UpdateCasaDocumentIdBatchAsync(List<long> ids, long documentId)
        {
            if (!ids.Any()) return;
            var records = await _dbContext.InterestRateConfigMasters.Where(x => ids.Contains(x.Id) && x.ProductGroupCode == ProductGroupCode.CASA.Code).ToListAsync();
            foreach (var rec in records)
            {
                rec.DocumentId = documentId;
                rec.ModifiedBy = "System";
                rec.ModifiedDate = DateTime.UtcNow;
            }
            var changes = await _dbContext.SaveChangesAsync();
            // WriteLog(LogType.INFOR, $"UpdateCasaDocumentIdBatchAsync: Updated {changes} records with DocumentId={documentId}");
        }

        // THÊM: Build ApplyPosList từ list Ids (extract từ service cũ)
        public async Task<string> GetApplyPosListByIdsAsync(List<long> intRateConfigIds)
        {
            try
            {
                if (!intRateConfigIds.Any()) return "";
                var lstApplyPos = await _dbContext.InterestRatePosApplys.Where(w => intRateConfigIds.Contains(w.IntRateConfigId)).Select(p => p.PosCode).Distinct().ToListAsync();
                var lstPosInfo = await _dbContext.ListOfPoss.Where(w => lstApplyPos.Contains(w.Code)).OrderBy(o => o.Code).Select(p => p.Name).ToListAsync();
                var posNameList = string.Join(", ", lstPosInfo);
                // WriteLog(LogType.INFOR, $"GetApplyPosListByIdsAsync: Built '{posNameList}' from {lstApplyPos.Count} POS codes");
                return posNameList;
            }
            catch (Exception ex)
            {
                // WriteLog(LogType.ERROR, $"GetApplyPosListByIdsAsync error: {ex.Message}");
                return "";
            }
        }
        // Upload File
        /// <summary>
        /// Lưu một file đính kèm (dùng cho upload AJAX multiple files)
        /// </summary>
        /// <param name="fileInfo">Thông tin file</param>
        /// <returns>FileId vừa tạo</returns>
        public async Task<long> SaveAttachedFileAsync(AttachedFileInfo fileInfo)
        {
            try
            {
                // Map sang entity (giả sử entity tên là AttachedFileInfo hoặc AttachedFile)
                var entity = _mapper.Map<AttachedFileInfo>(fileInfo); // nếu có AutoMapper config
                                                                      // Nếu không có mapper hoặc muốn map tay:
                                                                      // var entity = new AttachedFileInfo
                                                                      // {
                                                                      //     DocumentId = fileInfo.DocumentId,
                                                                      //     FileType = fileInfo.FileType,
                                                                      //     FileName = fileInfo.FileName,
                                                                      //     FileExtension = fileInfo.FileExtension,
                                                                      //     PathFile = fileInfo.PathFile,
                                                                      //     FileNameNew = fileInfo.FileNameNew,
                                                                      //     DocumentNumber = fileInfo.DocumentNumber,
                                                                      //     ContentDescription = fileInfo.ContentDescription ?? "File đính kèm tờ trình lãi suất CASA",
                                                                      //     Status = 1,
                                                                      //     CreatedBy = fileInfo.CreatedBy,
                                                                      //     CreatedDate = fileInfo.CreatedDate ?? DateTime.UtcNow
                                                                      // };

                _dbContext.AttachedFileInfos.Add(entity);
                await _dbContext.SaveChangesAsync();

                return entity.FileId; // Giả sử primary key là FileId (kiểu long)
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error SaveAttachedFileAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw new Exception("Lỗi khi lưu file đính kèm.", ex);
            }
        }

        /// <summary>
        /// Xóa (soft delete) file đính kèm và xóa file vật lý trên server
        /// </summary>
        public async Task<int> DeleteAttachedFileAsync(long fileId, string deletedBy)
        {
            try
            {
                var entity = await _dbContext.AttachedFileInfos.FindAsync(fileId);
                if (entity == null || entity.Status != 1)
                    return 0;

                // Soft delete
                entity.Status = 0;
                entity.ModifiedBy = deletedBy ?? "System";
                entity.ModifiedDate = DateTime.UtcNow;


                if (System.IO.File.Exists(entity.PathFile))
                {
                    System.IO.File.Delete(entity.PathFile);
                }

                return await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error DeleteAttachedFileAsync fileId={fileId}: {ex.Message}");
                throw;
            }
        }



        /// <summary>
        /// Hàm phê duyệt/từ chối đề nghị thay đổi lãi suất  
        /// </summary>
        /// <param name="userName">Tài khoản phê duyệt</param>
        /// <param name="lstId">Danh sách Id</param>
        /// <param name="rejectFlag">Cờ phê duyệt: 1 - Phê duyệt, 0 - Từ tối</param>
        /// <returns></returns>
        public async Task<int> SaveApprovalDecision(string userName, List<long> lstId, int rejectFlag, string rejectReason)
        {
            try
            {
                int status = 0;

                if (rejectFlag == 1)
                    status = ConfigStatus.REJECTED.Value;
                else
                    status = ConfigStatus.AUTHORIZED.Value;

                if (!lstId.Any())
                    return 0;

                //Bổ sung phần gọi api cập nhật vào CoreBanking
                TideIntRatesRequestViewModel requestData = new TideIntRatesRequestViewModel();
                requestData.UserId = ConstValueAPI.UserId_Call_ApiIDC;
                requestData.BankCircularDate = DateTime.UtcNow.ToString("yyyyMMdd");


                TideInterestRatesRequestViewModel interestRatesRequestViewModel = new TideInterestRatesRequestViewModel();
                List<RecordTideInterestRateViewModel> lstTideIntRate = new List<RecordTideInterestRateViewModel>();

                var records = await _dbContext.InterestRateConfigMasters.Where(x => lstId.Contains(x.Id)).ToListAsync();
                string circularRefNum = "";


                foreach (var rec in records)
                {
                    rec.Status = status;
                    rec.Remark = rejectReason;
                    rec.ApproverBy = userName;
                    rec.ApprovalDate = DateTime.UtcNow;
                    circularRefNum = rec.CircularRefNum;

                    var _lstPosApply = await _dbContext.InterestRatePosApplys
                        .Where(x => x.IntRateConfigId == rec.Id)
                        .Select(x => x.PosCode)
                        .ToListAsync();

                    if (_lstPosApply != null && _lstPosApply.Count > 0)
                    {
                        for (int i = 0; i < _lstPosApply.Count; i++)
                        {

                            RecordTideInterestRateViewModel recordTideInterestRateViewModel = new RecordTideInterestRateViewModel();
                            recordTideInterestRateViewModel.RecordSl = rec.RecordSerialNo.ToString();
                            recordTideInterestRateViewModel.ProductCode = rec.ProductCode;
                            recordTideInterestRateViewModel.AccountType = rec.AccountTypeCode;
                            recordTideInterestRateViewModel.AccountSubType = rec.AccountSubTypeCode;
                            recordTideInterestRateViewModel.CurrencyCode = rec.CurrencyCode;
                            recordTideInterestRateViewModel.EffectiveDate = rec.EffectiveDate.ToString("yyyyMMdd");
                            recordTideInterestRateViewModel.PosRateExpiryDate = rec.ExpiryDate?.ToString("yyyyMMdd") ?? "";
                            recordTideInterestRateViewModel.AmountSlab = rec.AmountSlab.ToString();
                            recordTideInterestRateViewModel.PosCode = _lstPosApply[i] == PosValue.HEAD_POS ? "0" : _lstPosApply[i];

                            ChildTideInterestRateViewModel childTideInterestRateViewModel = new ChildTideInterestRateViewModel();
                            List<ChildRecordTideInterestRateViewModel> childRecordTideInterestRateViewModels = new List<ChildRecordTideInterestRateViewModel>();


                            var termDetails = await _dbContext.InterestRateTermDetails.Where(x => x.IntRateConfigId == rec.Id).ToListAsync();

                            for (int j = 0; j < termDetails.Count; j++)
                            {
                                var term = termDetails[j];
                                ChildRecordTideInterestRateViewModel childRecordTideInterestRateViewModel = new ChildRecordTideInterestRateViewModel();
                                childRecordTideInterestRateViewModel.TenorSl = term.Serial.ToString();
                                childRecordTideInterestRateViewModel.InterestRate = term.IntRateNew.ToString();
                                childRecordTideInterestRateViewModel.IntRateType = term.IntRateType;
                                childRecordTideInterestRateViewModel.SpreadRate = term.Spread.ToString();
                                childRecordTideInterestRateViewModels.Add(childRecordTideInterestRateViewModel);
                            }

                            childTideInterestRateViewModel.ChildTideInterestRateRecord = childRecordTideInterestRateViewModels;
                            recordTideInterestRateViewModel.InterestRates = childTideInterestRateViewModel;

                            lstTideIntRate.Add(recordTideInterestRateViewModel);
                        }
                    }

                }

                interestRatesRequestViewModel.TideIntRates = lstTideIntRate;
                requestData.UpldIntRt = interestRatesRequestViewModel;
                requestData.BankCircularRefNum = circularRefNum;

                if (status == ConfigStatus.AUTHORIZED.Value)
                {
                    var apiResponse = await _apiInternalEsbService.TideIntRates(requestData);
                    // Cập nhật trạng thái update vào core
                    foreach (var rec in records)
                    {
                        rec.CallApiTxnStatus = apiResponse.TxnStatus;
                        rec.CallApiResponseMsg = apiResponse.ResponseMsg;
                        rec.CallApiResponseCode = apiResponse.ResponseCode;
                        rec.CallApiReqRecordSl = int.Parse(apiResponse.StatusList.FirstOrDefault()?.ReqRecordSl ?? "0");
                        rec.StatusUpdateCore = (apiResponse.TxnStatus == ResultValueAPI.ResultValue_Status_Success) ? 1 : 0;
                    }
                    var result = await _dbContext.SaveChangesAsync();
                    return result > 0 && apiResponse.TxnStatus == ResultValueAPI.ResultValue_Status_Success ? 1 : 0;
                }
                else
                {
                    var result = await _dbContext.SaveChangesAsync();
                    return result;
                }

            }
            catch (Exception e)
            {
                _logger.LogError($"SaveApprovalDecision error: {e.Message}");
                throw e;
            }
        }

        public bool CheckCircular(string circularRefNum, DateTime circularDate)
        {
            try
            {
                bool isExists = _dbContext.InterestRateConfigMasters.Any(x =>
        x.CircularRefNum == circularRefNum && x.CircularDate == circularDate.Date
    );
                return isExists;
            }
            catch (Exception e)
            {
                _logger.LogError($"SaveApprovalDecision error: {e.Message}");
                throw e;
            }
        }

        /// <summary>
        /// Hàm lấy dan sách lãi suất rút trước hạn của sản phẩm TG có kỳ hạn
        /// </summary>
        /// <param name="pPosCode">Mã POS. 0 Lấy tất</param>
        /// <param name="pProductCode">Mã sản phẩm. 0 - Lấy tất</param>
        /// <param name="pCurrencyCode">Mã tiền tệ. VND</param>
        /// <param name="pEffectDate">Ngày hiệu lực</param>
        /// <param name="pExpiredDate">Ngày hết hiệu lực (Truyền giá trị mới vào)</param>
        /// <param name="pCircularRefNum">Số quyết định (Truyền giá trị mới vào)</param>
        /// <param name="pCircularDate">Ngày ký quyết định (Truyền giá trị mới vào)</param>
        /// <param name="pInterestRateNew">Lãi suất mới theo quyết định (Truyền giá trị mới vào)</param>
        /// <param name="pPenalIntRate">Lãi suất phạt mới theo quyết định (Truyền giá trị mới vào)</param>
        /// <returns>Danh sách lãi suất rút trước hạn sản phẩm TG có kỳ hạn</returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<UpdateTidePenalRateConfigViewModel>> GetListDepPenalIntRate(string pPosCode, string pProductCode, string pCurrencyCode, DateTime pEffectDate,
                            DateTime pExpiredDate, string pCircularRefNum, DateTime pCircularDate, decimal pInterestRateNew, decimal pPenalIntRate)
        {
            try
            {
                pPosCode = string.IsNullOrEmpty(pPosCode) ? "0" : pPosCode;
                pProductCode = string.IsNullOrEmpty(pProductCode) ? "0" : pProductCode;
                string sEffectDate = pEffectDate.ToString(FormatParameters.FORMAT_DATE_INT);
                DepositPenalIntRateRequestViewModel requestInput = new DepositPenalIntRateRequestViewModel();
                requestInput.UserId = ConstValueAPI.UserId_Call_ApiIDC;
                requestInput.ProductCode = pProductCode;
                requestInput.CurrencyCode = pCurrencyCode;
                requestInput.EffDate = sEffectDate;
                requestInput.PosCode = pPosCode;
                int iCountCheckPos = 0;
                var response = await _apiInternalEsbService.GetListDepositPenalInterestRate(requestInput);
                if (response == null || response.Result == null || response.Result.Count <= 0)
                {
                    if (pPosCode != PosValue.HEAD_POS || pPosCode != PosValue.SYSTEM_WIDE || pPosCode != PosValue.BANK_WIDE || pPosCode != "000199")
                    {
                        iCountCheckPos = 1;
                        requestInput.PosCode = PosValue.BANK_WIDE;
                        response = await _apiInternalEsbService.GetListDepositPenalInterestRate(requestInput);
                    }
                }
                if (response == null || response.Result == null || response.Result.Count <= 0)
                {
                    return new List<UpdateTidePenalRateConfigViewModel>();
                }
                List<DepositPenalIntRateReposeViewModel> listDepositPenalIntRateHO = new List<DepositPenalIntRateReposeViewModel>();
                if (pPosCode != PosValue.BANK_WIDE && pPosCode != PosValue.SYSTEM_WIDE)
                {
                    DepositPenalIntRateRequestViewModel requestInputHO = new DepositPenalIntRateRequestViewModel();
                    requestInputHO.UserId = ConstValueAPI.UserId_Call_ApiIDC;
                    requestInputHO.ProductCode = pProductCode;
                    requestInputHO.CurrencyCode = pCurrencyCode;
                    requestInputHO.EffDate = sEffectDate;
                    requestInputHO.PosCode = PosValue.BANK_WIDE;
                    var responseHO = await _apiInternalEsbService.GetListDepositPenalInterestRate(requestInputHO);
                    if (responseHO.ResponseCode == "00000")
                    {
                        listDepositPenalIntRateHO = responseHO.Result;
                    }
                }
                List<UpdateTidePenalRateConfigViewModel> result = new List<UpdateTidePenalRateConfigViewModel>();
                if (response.ResponseCode == "00000")
                {
                    int iCountTemp = 0;
                    var listProducts = _dbContext.ListOfProducts.Where(w => w.ProductGroupCode == ProductGroupCode.ProductGroupCode_Tide).ToList();
                    var listPoss = _dbContext.ListOfPoss.Where(w => w.Code != "999999").ToList();
                    var listIntRateConfigMasterFind = _dbContext.InterestRateConfigMasters.Where(w => w.ProductGroupCode == ProductGroupCode.ProductGroupCode_DepositPenal
                                                && w.Status != StatusLov.StatusClosed && w.Status != StatusLov.StatusReject).ToList();
                    var listProductParameters = _productService.GetListProductParametersSearch(ProductGroupCode.ProductGroupCode_DepositPenal, "",
                                                                    pEffectDate, ConfigStatus.AUTHORIZED.Value);
                    foreach (var itemPenalIntRate in response.Result)
                    {
                        iCountTemp++;
                        UpdateTidePenalRateConfigViewModel itemRet = new UpdateTidePenalRateConfigViewModel();
                        itemRet.EffectiveDate = CustConverter.StringToDate(itemPenalIntRate.EffDate, FormatParameters.FORMAT_DATE_INT).Date;
                        itemRet.ProductCode = itemPenalIntRate.ProductCode;
                        itemRet.CurrencyCode = itemPenalIntRate.CurrencyCode;
                        itemRet.PosCode = itemPenalIntRate.PosCode;
                        if (iCountCheckPos == 1)
                        {
                            itemRet.PosCode = pPosCode;
                        }
                        itemRet.InterestRate = (itemPenalIntRate.PenalIntRate == null) ? 0 : decimal.Parse(itemPenalIntRate.PenalIntRate);
                        itemRet.InterestRateNew = pInterestRateNew;
                        itemRet.EffectiveDateNew = pEffectDate;
                        itemRet.EffectiveDateTextNew = itemRet.EffectiveDateNew.ToString(FormatParameters.FORMAT_DATE);
                        itemRet.CircularRefNumNew = pCircularRefNum;
                        itemRet.PenalIntRate = pPenalIntRate;
                        itemRet.OrderNo = iCountTemp;
                        itemRet.EffectiveDateText = itemRet.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                        itemRet.DebitCreditFlag = VBSPOSS.Constants.DebitCreditFlag.DebitCreditFlag_Credit;
                        itemRet.Id = 0;
                        itemRet.IdList = "";
                        if (listProducts != null && listProducts.Count != 0)
                        {
                            itemRet.ProductName = listProducts.Where(w => w.ProductCode == itemPenalIntRate.ProductCode).OrderByDescending(o => o.PosCode).FirstOrDefault().ProductName;
                        }
                        else itemRet.ProductName = "";
                        itemRet.AccountTypeCode = "";
                        itemRet.AccountTypeName = "";
                        if (itemRet.PosCode == PosValue.BANK_WIDE || itemRet.PosCode == PosValue.SYSTEM_WIDE || itemRet.PosCode == "ALL" || itemRet.PosCode == "")
                            itemRet.PosName = "Toàn hàng";
                        else
                        {
                            if (listPoss != null && listPoss.Count != 0)
                                itemRet.PosName = listPoss.Where(w => w.Code == itemRet.PosCode).OrderByDescending(o => o.Status).FirstOrDefault().Name;
                            else itemRet.PosName = "";
                        }
                        itemRet.TenorSerialNo = 1;

                        itemRet.ExpiredDate = pExpiredDate;

                        itemRet.ExpiredDate = new DateTime(2050, 12, 31).Date;
                        itemRet.CircularDate = new DateTime(2025, 01, 01).Date;
                        itemRet.CircularRefNum = "";
                        itemRet.Status = StatusLov.StatusOpen;
                        itemRet.StatusUpdateCore = StatusTrans.Status_CallApi_NotUpdated.Value;
                        if (listIntRateConfigMasterFind != null && listIntRateConfigMasterFind.Count != 0)
                        {
                            var objIntRateConfigMasterFind = listIntRateConfigMasterFind.Where(w => w.ProductCode == itemRet.ProductCode && w.PosCode == itemRet.PosCode
                                                && w.CurrencyCode == itemRet.CurrencyCode && w.EffectiveDate == itemRet.EffectiveDate
                                                //&& (string.IsNullOrEmpty(pCircularRefNum) || w.CircularRefNum == pCircularRefNum)
                                                ).OrderByDescending(o => o.EffectiveDate).FirstOrDefault();
                            if (objIntRateConfigMasterFind != null && objIntRateConfigMasterFind.Id != 0)
                            {
                                itemRet.ExpiredDate = objIntRateConfigMasterFind.ExpiryDate.Value;
                                itemRet.CircularDate = objIntRateConfigMasterFind.CircularDate.Value;
                                itemRet.CircularRefNum = string.IsNullOrEmpty(objIntRateConfigMasterFind.CircularRefNum) ? "" : objIntRateConfigMasterFind.CircularRefNum;
                                //itemRet.Status = objIntRateConfigMasterFind.Status;
                                //itemRet.Id = objIntRateConfigMasterFind.Id;
                                //itemRet.IdList = objIntRateConfigMasterFind.Id.ToString();
                                //itemRet.StatusUpdateCore = objIntRateConfigMasterFind.StatusUpdateCore;
                                //itemRet.InterestRate = objIntRateConfigMasterFind.NewInterestRate.Value;
                                //itemRet.PenalIntRate = objIntRateConfigMasterFind.PenalRate.Value;
                                //itemRet.OrderNo = objIntRateConfigMasterFind.RecordSerialNo;
                            }
                        }
                        if (itemRet.PosCode == PosValue.HEAD_POS || itemRet.PosCode == PosValue.BANK_WIDE || itemRet.PosCode == PosValue.SYSTEM_WIDE)
                        {
                            itemRet.MinInterestRateSpread = 0;
                            itemRet.MaxInterestRateSpread = 10;
                        }
                        else
                        {
                            if (listProductParameters != null && listProductParameters.Count != 0)
                            {
                                itemRet.MinInterestRateSpread = listProductParameters.Where(w => w.ProductCode == itemRet.ProductCode).OrderByDescending(o => o.Status).FirstOrDefault().MinInterestRateSpread;
                                itemRet.MaxInterestRateSpread = listProductParameters.Where(w => w.ProductCode == itemRet.ProductCode).OrderByDescending(o => o.Status).FirstOrDefault().MaxInterestRateSpread;
                            }
                            else
                            {
                                itemRet.MinInterestRateSpread = 0;
                                itemRet.MaxInterestRateSpread = 0;
                            }
                        }
                        itemRet.InterestRateHO = 0;
                        if (itemRet.PosCode != PosValue.BANK_WIDE && itemRet.PosCode != PosValue.SYSTEM_WIDE)
                        {
                            if (listDepositPenalIntRateHO != null && listDepositPenalIntRateHO.Count != 0)
                            {
                                itemRet.InterestRateHO = decimal.Parse(listDepositPenalIntRateHO.Where(w => w.ProductCode == itemRet.ProductCode).FirstOrDefault().PenalIntRate);
                            }
                        }
                        result.Add(itemRet);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetListDepPenalIntRate('{pPosCode}','{pProductCode}','{pCurrencyCode}','{pEffectDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}'," +
                                $" '{pExpiredDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}','{pCircularRefNum}','{pCircularDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}',{pInterestRateNew.ToString()},{pPenalIntRate.ToString()}) => Error: {ex.Message}");
                throw new Exception($"Lỗi khi gọi API lấy danh sách sản phẩm Tide GetListDepPenalIntRate('{pPosCode}','{pProductCode}','{pCurrencyCode}','{pEffectDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}'," +
                                $" '{pExpiredDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}','{pCircularRefNum}','{pCircularDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}',{pInterestRateNew.ToString()},{pPenalIntRate.ToString()}) => Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Hàm lấy danh sách các Quyết định cấu hình lãi suất rút trước hạn trong bảng InterestRateConfigMaster
        /// </summary>
        /// <param name="pPosCode">Mã POS (Không bắt buộc)</param>
        /// <param name="pProductCode">Mã sản phẩm (Không bắt buộc)</param>
        /// <param name="pCurrencyCode">Mã tiền tệ (Không bắt buộc)</param>
        /// <param name="pFromEffectDate">Ngày hiệu lực - Từ ngày</param>
        /// <param name="pToEffectDate">Ngày hiệu lực - Đến ngày</param>
        /// <param name="pCircularRefNum">Số quyết định</param>
        /// <param name="pCircularDate">Ngày ký quyết định</param>
        /// <param name="pListId">Danh sách Id bảng DL truyền vào cách nhau bởi dấu chấm phẩy (Không bắt buộc)</param>
        /// <returns>Danh sách các Quyết định cấu hình lãi suất rút trước hạn trong bảng InterestRateConfigMaster</returns>
        public async Task<List<UpdateTidePenalRateConfigViewModel>> GetListDepPenalIntRate(string pPosCode, string pProductCode, string pCurrencyCode, DateTime pFromEffectDate,
                                    DateTime pToEffectDate, string pCircularRefNum, DateTime pCircularDate, string pListId)
        {
            try
            {
                List<UpdateTidePenalRateConfigViewModel> listDepPenalIntRates = new List<UpdateTidePenalRateConfigViewModel>();

                var listIntRateConfigMasterFind = _dbContext.InterestRateConfigMasters.Where(w => w.ProductGroupCode == ProductGroupCode.ProductGroupCode_DepositPenal
                            && w.Status != StatusLov.StatusClosed && w.Status != StatusLov.StatusReject).ToList();
                var listIntRateConfigMasterFind01 = listIntRateConfigMasterFind.Where(w => w.ProductGroupCode == ProductGroupCode.ProductGroupCode_DepositPenal
                            && (string.IsNullOrEmpty(pPosCode) || pPosCode == PosValue.HEAD_POS || (w.PosCode == pPosCode))).ToList();

                var listInterestRateConfigMasterTemp = listIntRateConfigMasterFind.Where(w => w.ProductGroupCode == ProductGroupCode.ProductGroupCode_DepositPenal
                        && (string.IsNullOrEmpty(pPosCode) || pPosCode == PosValue.HEAD_POS || (w.PosCode == pPosCode))
                        && (string.IsNullOrEmpty(pCurrencyCode) || w.CurrencyCode == pCurrencyCode)
                        && (string.IsNullOrEmpty(pProductCode) || w.ProductCode == pProductCode)
                        && (w.EffectiveDate >= pFromEffectDate.Date && w.EffectiveDate <= pToEffectDate.Date)
                       )
                       .Where(delegate (InterestRateConfigMaster c)
                       {
                           if (string.IsNullOrEmpty(pCircularRefNum)
                               || (c.CircularRefNum != null && c.CircularRefNum.ToLower().Contains(pCircularRefNum.ToLower()))
                               || (c.CircularRefNum != null && Utilities.ConvertToUnSign(c.CircularRefNum.ToLower()).IndexOf(pCircularRefNum.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                               )
                               return true;
                           else
                               return false;
                       })
                       .OrderByDescending(o => o.PosCode).ThenBy(o => o.ProductCode).ThenBy(o => o.EffectiveDate).ToList();

                if (listInterestRateConfigMasterTemp != null && listInterestRateConfigMasterTemp.Count != 0)
                {
                    List<DepositPenalIntRateReposeViewModel> listDepositPenalIntRateHO = new List<DepositPenalIntRateReposeViewModel>();
                    if (pPosCode != PosValue.HEAD_POS && pPosCode != PosValue.BANK_WIDE && pPosCode != PosValue.SYSTEM_WIDE)
                    {
                        DepositPenalIntRateRequestViewModel requestInputHO = new DepositPenalIntRateRequestViewModel();
                        requestInputHO.UserId = ConstValueAPI.UserId_Call_ApiIDC;
                        requestInputHO.ProductCode = pProductCode;
                        requestInputHO.CurrencyCode = pCurrencyCode;
                        requestInputHO.EffDate = pFromEffectDate.ToString(FormatParameters.FORMAT_DATE_INT);
                        requestInputHO.PosCode = PosValue.BANK_WIDE;
                        var responseHO = await _apiInternalEsbService.GetListDepositPenalInterestRate(requestInputHO);
                        if (responseHO.ResponseCode == "00000")
                            listDepositPenalIntRateHO = responseHO.Result;
                    }
                    List<InterestRateConfigMaster> listIntRateConfigMasters = new List<InterestRateConfigMaster>();
                    if (!string.IsNullOrEmpty(pListId))
                    {
                        string[] listIdRows = Utilities.Splip_Strings(pListId, ";");
                        listIntRateConfigMasters = listInterestRateConfigMasterTemp.Where(w => w.Id != 0 && listIdRows.Contains(w.Id.ToString())).OrderBy(o => o.RecordSerialNo).ToList();
                    }
                    else listIntRateConfigMasters = listInterestRateConfigMasterTemp.Where(w => w.Id != 0).OrderBy(o => o.RecordSerialNo).ToList();
                    int iCountTemp = 0;

                    var listProductParameters = _productService.GetListProductParametersSearch(ProductGroupCode.ProductGroupCode_DepositPenal, "",
                                                                    pToEffectDate, ConfigStatus.AUTHORIZED.Value);
                    if (listIntRateConfigMasters != null && listIntRateConfigMasters.Count != 0)
                    {
                        foreach (var itemTemp in listIntRateConfigMasters)
                        {
                            iCountTemp++;
                            UpdateTidePenalRateConfigViewModel itemRet = new UpdateTidePenalRateConfigViewModel();
                            itemRet.Id = itemTemp.Id;
                            itemRet.IdList = itemTemp.Id.ToString();
                            itemRet.ProductCode = itemTemp.ProductCode;
                            itemRet.ProductName = itemTemp.ProductName;
                            itemRet.AccountTypeCode = itemTemp.AccountTypeCode;
                            itemRet.AccountTypeName = itemTemp.AccountTypeName;
                            itemRet.CurrencyCode = itemTemp.CurrencyCode;
                            itemRet.DebitCreditFlag = itemTemp.DebitCreditFlag;
                            itemRet.EffectiveDate = itemTemp.EffectiveDate;
                            itemRet.EffectiveDateNew = itemTemp.EffectiveDate;
                            itemRet.InterestRate = itemTemp.InterestRate;
                            itemRet.InterestRateNew = itemTemp.NewInterestRate.Value;
                            itemRet.ExpiredDate = itemTemp.ExpiryDate.Value;
                            itemRet.CircularRefNum = itemTemp.CircularRefNum;
                            itemRet.CircularRefNumNew = itemTemp.CircularRefNum;
                            itemRet.PosCode = itemTemp.PosCode;
                            itemRet.PenalIntRate = itemTemp.PenalRate.Value;
                            itemRet.TenorSerialNo = itemTemp.TenorSerialNo;
                            itemRet.CircularDate = itemTemp.CircularDate.Value;
                            itemRet.OrderNo = itemTemp.RecordSerialNo;
                            itemRet.PosName = itemTemp.PosName;
                            itemRet.EffectiveDateText = itemRet.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                            itemRet.EffectiveDateTextNew = itemRet.EffectiveDate.ToString(FormatParameters.FORMAT_DATE);
                            itemRet.Status = itemTemp.Status;
                            itemRet.StatusText = StatusTrans.GetByValue(itemTemp.Status).Description;
                            if (string.IsNullOrEmpty(itemRet.StatusText))
                                itemRet.StatusText = "";
                            itemRet.StatusUpdateCore = itemTemp.StatusUpdateCore;
                            if (itemRet.PosCode == PosValue.HEAD_POS || itemRet.PosCode == PosValue.BANK_WIDE || itemRet.PosCode == PosValue.SYSTEM_WIDE)
                            {
                                itemRet.MinInterestRateSpread = 0;
                                itemRet.MaxInterestRateSpread = 10;
                            }
                            else
                            {
                                if (listProductParameters != null && listProductParameters.Count != 0)
                                {
                                    itemRet.MinInterestRateSpread = listProductParameters.Where(w => w.ProductCode == itemRet.ProductCode).OrderByDescending(o => o.Status).FirstOrDefault().MinInterestRateSpread;
                                    itemRet.MaxInterestRateSpread = listProductParameters.Where(w => w.ProductCode == itemRet.ProductCode).OrderByDescending(o => o.Status).FirstOrDefault().MaxInterestRateSpread;
                                }
                                else
                                {
                                    itemRet.MinInterestRateSpread = 0;
                                    itemRet.MaxInterestRateSpread = 0;
                                }
                            }
                            itemRet.InterestRateHO = 0;
                            if (itemRet.PosCode != PosValue.HEAD_POS && itemRet.PosCode != PosValue.BANK_WIDE && itemRet.PosCode != PosValue.SYSTEM_WIDE)
                            {
                                if (listDepositPenalIntRateHO != null && listDepositPenalIntRateHO.Count != 0)
                                {
                                    itemRet.InterestRateHO = decimal.Parse(listDepositPenalIntRateHO.Where(w => w.ProductCode == itemRet.ProductCode).FirstOrDefault().PenalIntRate);
                                }
                            }
                            listDepPenalIntRates.Add(itemRet);
                        }
                    }
                }
                return listDepPenalIntRates;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi gọi hàm Lấy danh sách Quyết định cấu hình lãi suất rút trước hạn trong bảng InterestRateConfigMaster" +
                    $"GetListDepPenalIntRate('{pPosCode}', '{pProductCode}', '{pCurrencyCode}', '{pFromEffectDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}', " +
                        $" '{pToEffectDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}', '{pCircularRefNum}', " +
                        $"'{pCircularDate.ToString(FormatParameters.FORMAT_DATE_TIME_SHORT_UPD)}', '{pListId}') => Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Hàm thực hiện thêm mới hoặc cập nhật thay đổi thông tin cấui hình lãi suất vào bảng InterestRateConfigMaster.
        /// Trường hợp thêm mới thì sẽ bao gồm cả cập nhật nếu Id<>0
        /// </summary>
        /// <param name="pListInterestRateConfigMasterUpds">Danh sách InterestRateConfigMaster cần cập nhật</param>
        /// <param name="pUserNameUpd">Người cập nhật</param>
        /// <param name="pFlagCall">1: Thêm mới; 2: Chỉnh sửa</param>
        /// <returns>Số bản ghi được cập nhật</returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> SaveInterestRateConfigMaster(List<InterestRateConfigMaster> pListInterestRateConfigMasterUpds, string pUserNameUpd, string pFlagCall)
        {
            int iCountUpdate = 0, iCountTemp = 0;
            DateTime dCurrentDateTmp = DateTime.Now;
            try
            {
                if (pListInterestRateConfigMasterUpds != null && pListInterestRateConfigMasterUpds.Count != 0)
                {
                    if (pFlagCall == EventFlag.EventFlag_Edit.Value.ToString())
                    {
                        #region --- Cập nhật chỉnh sửa thông tin ---
                        List<InterestRateConfigMaster> listUpdateIntRateCofigDetail = new List<InterestRateConfigMaster>();
                        foreach (var itemUpd in pListInterestRateConfigMasterUpds)
                        {
                            var objIntRateCofigUpdNew = _dbContext.InterestRateConfigMasters.Where(m => m.Id == itemUpd.Id).FirstOrDefault();
                            if (objIntRateCofigUpdNew != null && !string.IsNullOrEmpty(objIntRateCofigUpdNew.ProductGroupCode)
                                    && !string.IsNullOrEmpty(objIntRateCofigUpdNew.ProductCode) && objIntRateCofigUpdNew.Id != 0)
                            {
                                objIntRateCofigUpdNew.PosCode = itemUpd.PosCode;
                                objIntRateCofigUpdNew.PosName = itemUpd.PosName;
                                objIntRateCofigUpdNew.ProductGroupCode = string.IsNullOrEmpty(itemUpd.ProductGroupCode) ? "" : itemUpd.ProductGroupCode;//ProductGroupCode.ProductGroupCode_DepositPenal
                                objIntRateCofigUpdNew.UserId = string.IsNullOrEmpty(itemUpd.UserId) ? ConstValueAPI.UserId_Call_ApiIDC : itemUpd.UserId;
                                objIntRateCofigUpdNew.CircularDate = itemUpd.CircularDate.Value.Date;
                                objIntRateCofigUpdNew.CircularRefNum = itemUpd.CircularRefNum;
                                objIntRateCofigUpdNew.RecordSerialNo = itemUpd.RecordSerialNo;
                                objIntRateCofigUpdNew.ProductCode = itemUpd.ProductCode;
                                objIntRateCofigUpdNew.ProductName = itemUpd.ProductName;
                                objIntRateCofigUpdNew.AccountTypeCode = itemUpd.AccountTypeCode;
                                objIntRateCofigUpdNew.AccountTypeName = itemUpd.AccountTypeName;
                                objIntRateCofigUpdNew.AccountSubTypeCode = itemUpd.AccountSubTypeCode;
                                objIntRateCofigUpdNew.AccountSubTypeName = itemUpd.AccountSubTypeName;
                                objIntRateCofigUpdNew.CurrencyCode = itemUpd.CurrencyCode;
                                objIntRateCofigUpdNew.EffectiveDate = itemUpd.EffectiveDate.Date;
                                objIntRateCofigUpdNew.ExpiryDate = itemUpd.ExpiryDate.Value.Date;
                                objIntRateCofigUpdNew.DebitCreditFlag = itemUpd.DebitCreditFlag;

                                objIntRateCofigUpdNew.InterestRate = itemUpd.InterestRate;
                                objIntRateCofigUpdNew.PenalRate = itemUpd.PenalRate;
                                objIntRateCofigUpdNew.AmountSlab = itemUpd.AmountSlab;
                                objIntRateCofigUpdNew.TenorSerialNo = itemUpd.TenorSerialNo;
                                objIntRateCofigUpdNew.IntRateType = itemUpd.IntRateType;
                                objIntRateCofigUpdNew.SpreadRate = itemUpd.SpreadRate;
                                objIntRateCofigUpdNew.Remark = string.IsNullOrEmpty(objIntRateCofigUpdNew.Remark) ? itemUpd.Remark : objIntRateCofigUpdNew.Remark;
                                objIntRateCofigUpdNew.OrtherNotes = itemUpd.OrtherNotes;
                                objIntRateCofigUpdNew.Status = StatusTrans.Status_Modified.Value;
                                objIntRateCofigUpdNew.StatusUpdateCore = itemUpd.StatusUpdateCore;
                                objIntRateCofigUpdNew.CallApiTxnStatus = itemUpd.CallApiTxnStatus;
                                objIntRateCofigUpdNew.CallApiReqRecordSl = itemUpd.CallApiReqRecordSl;
                                objIntRateCofigUpdNew.CallApiResponseCode = itemUpd.CallApiResponseCode;
                                objIntRateCofigUpdNew.CallApiResponseMsg = itemUpd.CallApiResponseMsg;

                                objIntRateCofigUpdNew.ModifiedDate = dCurrentDateTmp;
                                objIntRateCofigUpdNew.ModifiedBy = pUserNameUpd;

                                listUpdateIntRateCofigDetail.Add(objIntRateCofigUpdNew);
                                iCountTemp++;
                            }
                        }
                        if (iCountTemp > 0)
                        {
                            _dbContext.InterestRateConfigMasters.UpdateRange(listUpdateIntRateCofigDetail);
                            int iSaveChanges = await _dbContext.SaveChangesAsync();

                            if (iSaveChanges > 0)
                                iCountUpdate = iCountUpdate + iCountTemp;
                        }
                        #endregion
                    }
                    else if (pFlagCall == EventFlag.EventFlag_Add.Value.ToString())
                    {
                        #region --- Cập nhật thêm mới thông tin (Bao gồm cả chỉnh sửa với bản ghi có Id != 0) ---

                        List<InterestRateConfigMaster> listAddNewIntRateCofig = new List<InterestRateConfigMaster>();
                        List<InterestRateConfigMaster> listUpdateIntRateCofig = new List<InterestRateConfigMaster>();

                        listAddNewIntRateCofig = pListInterestRateConfigMasterUpds.Where(w => w.Id == 0).ToList();
                        listUpdateIntRateCofig = pListInterestRateConfigMasterUpds.Where(w => w.Id != 0).ToList();
                        iCountTemp = 0;
                        if (listAddNewIntRateCofig != null && listAddNewIntRateCofig.Count != 0)
                        {
                            foreach (var itemAdd in listAddNewIntRateCofig)
                            {
                                itemAdd.Status = StatusTrans.Status_Created.Value;
                                itemAdd.CreatedBy = pUserNameUpd;
                                itemAdd.CreatedDate = dCurrentDateTmp;
                                itemAdd.ModifiedBy = pUserNameUpd;
                                itemAdd.ModifiedDate = dCurrentDateTmp;
                                itemAdd.ApproverBy = pUserNameUpd;
                                itemAdd.ApprovalDate = dCurrentDateTmp;

                                itemAdd.StatusUpdateCore = StatusTrans.Status_CallApi_NotUpdated.Value;
                                itemAdd.CallApiTxnStatus = "";
                                itemAdd.CallApiReqRecordSl = 0;
                                itemAdd.CallApiResponseCode = "";
                                itemAdd.CallApiResponseMsg = "";
                                iCountTemp++;
                            }
                            if (iCountTemp > 0)
                            {
                                _dbContext.InterestRateConfigMasters.AddRange(listAddNewIntRateCofig);
                                int iSaveChanges = await _dbContext.SaveChangesAsync();
                                if (iSaveChanges > 0)
                                    iCountUpdate = iCountUpdate + iCountTemp;
                            }
                        }
                        iCountTemp = 0;
                        if (listUpdateIntRateCofig != null && listUpdateIntRateCofig.Count != 0)
                        {
                            int iCountTempUpd = 0;
                            List<InterestRateConfigMaster> listUpdateIntRateCofigNew = new List<InterestRateConfigMaster>();
                            foreach (var itemUpd in listUpdateIntRateCofig)
                            {
                                var objIntRateUpdate = _dbContext.InterestRateConfigMasters.Where(m => m.Id == itemUpd.Id).FirstOrDefault();
                                if (objIntRateUpdate != null && !string.IsNullOrEmpty(objIntRateUpdate.ProductGroupCode) && !string.IsNullOrEmpty(objIntRateUpdate.ProductCode)
                                        && objIntRateUpdate.Id != 0)
                                {
                                    objIntRateUpdate.PosCode = itemUpd.PosCode;
                                    objIntRateUpdate.PosName = itemUpd.PosName;
                                    objIntRateUpdate.ProductGroupCode = string.IsNullOrEmpty(itemUpd.ProductGroupCode) ? "" : itemUpd.ProductGroupCode;//ProductGroupCode.ProductGroupCode_DepositPenal
                                    objIntRateUpdate.UserId = string.IsNullOrEmpty(itemUpd.UserId) ? ConstValueAPI.UserId_Call_ApiIDC : itemUpd.UserId;
                                    objIntRateUpdate.CircularDate = itemUpd.CircularDate.Value.Date;
                                    objIntRateUpdate.CircularRefNum = itemUpd.CircularRefNum;
                                    objIntRateUpdate.RecordSerialNo = itemUpd.RecordSerialNo;
                                    objIntRateUpdate.ProductCode = itemUpd.ProductCode;
                                    objIntRateUpdate.ProductName = itemUpd.ProductName;
                                    objIntRateUpdate.AccountTypeCode = itemUpd.AccountTypeCode;
                                    objIntRateUpdate.AccountTypeName = itemUpd.AccountTypeName;
                                    objIntRateUpdate.AccountSubTypeCode = itemUpd.AccountSubTypeCode;
                                    objIntRateUpdate.AccountSubTypeName = itemUpd.AccountSubTypeName;
                                    objIntRateUpdate.CurrencyCode = itemUpd.CurrencyCode;
                                    objIntRateUpdate.EffectiveDate = itemUpd.EffectiveDate.Date;
                                    objIntRateUpdate.ExpiryDate = itemUpd.ExpiryDate.Value.Date;
                                    objIntRateUpdate.DebitCreditFlag = itemUpd.DebitCreditFlag;

                                    objIntRateUpdate.InterestRate = itemUpd.InterestRate;
                                    objIntRateUpdate.PenalRate = itemUpd.PenalRate;
                                    objIntRateUpdate.AmountSlab = itemUpd.AmountSlab;
                                    objIntRateUpdate.TenorSerialNo = itemUpd.TenorSerialNo;
                                    objIntRateUpdate.IntRateType = itemUpd.IntRateType;
                                    objIntRateUpdate.SpreadRate = itemUpd.SpreadRate;
                                    objIntRateUpdate.Remark = itemUpd.Remark;
                                    objIntRateUpdate.OrtherNotes = itemUpd.OrtherNotes;
                                    objIntRateUpdate.Status = StatusTrans.Status_Modified.Value;
                                    objIntRateUpdate.StatusUpdateCore = itemUpd.StatusUpdateCore;
                                    objIntRateUpdate.CallApiTxnStatus = itemUpd.CallApiTxnStatus;
                                    objIntRateUpdate.CallApiReqRecordSl = itemUpd.CallApiReqRecordSl;
                                    objIntRateUpdate.CallApiResponseCode = itemUpd.CallApiResponseCode;
                                    objIntRateUpdate.CallApiResponseMsg = itemUpd.CallApiResponseMsg;

                                    objIntRateUpdate.ModifiedDate = dCurrentDateTmp;
                                    objIntRateUpdate.ModifiedBy = pUserNameUpd;
                                    listUpdateIntRateCofigNew.Add(objIntRateUpdate);
                                    iCountTempUpd++;
                                }
                            }
                            if (iCountTempUpd > 0)
                            {
                                _dbContext.InterestRateConfigMasters.UpdateRange(listUpdateIntRateCofigNew);
                                int iSaveChanges = await _dbContext.SaveChangesAsync();

                                if (iSaveChanges > 0)
                                    iCountUpdate = iCountUpdate + iCountTempUpd;
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                iCountUpdate = -1;
                Console.WriteLine($"SaveInterestRateConfigMaster('{pListInterestRateConfigMasterUpds.FirstOrDefault().ProductGroupCode}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}");
                throw new Exception($"Lỗi gọi hàm cập nhật thông tin cấu hình lãi suất " +
                    $"SaveInterestRateConfigMaster('{pListInterestRateConfigMasterUpds.FirstOrDefault().ProductGroupCode}', '{pUserNameUpd}', '{pFlagCall}') => Error: {ex.Message}", ex);
            }
            return iCountUpdate;
        }

        /// <summary>
        /// Hàm thực hiện xóa/đánh dấu xóa danh sách bản ghi Cấu hình lãi suất InterestRateConfigMaster
        /// </summary>
        /// <param name="pProductGroupCode">Phân nhóm sản phẩm TIDE|CASA|DEPOSITPENAL</param>
        /// <param name="pDocumentId">Chỉ số văn bản của cấu hình lãi suất cần xóa</param>
        /// <param name="pId">Chỉ số Id bản ghi cần xóa</param>
        /// <param name="pListId">Danh sách Id cần xóa cách nhau bởi dấu chấm phẩy</param>
        /// <param name="pUserName">Người thực hiện</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>True - Thành công; False - Thất bại</returns>
        public async Task<bool> DeleteInterestRateConfigMaster(string pProductGroupCode, long pDocumentId, long pId, string pListId, string pUserName, int pFlagDelete)
        {
            int iCountDelete = 0;
            bool bResult = false;
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                List<InterestRateConfigMaster> listIntRateConfigDelete = new List<InterestRateConfigMaster>();
                var listIntRateConfigDeleteTmp = _dbContext.InterestRateConfigMasters.Where(w => w.ProductGroupCode == pProductGroupCode
                            && (pDocumentId == 0 || w.DocumentId == pDocumentId) && (pId == 0 || w.Id == pId)).ToList();
                if (!string.IsNullOrEmpty(pListId))
                {
                    string[] listIdRows = Utilities.Splip_Strings(pListId, ";");
                    listIntRateConfigDelete = listIntRateConfigDeleteTmp.Where(w => w.Id != 0 && listIdRows.Contains(w.Id.ToString())).OrderBy(o => o.RecordSerialNo).ToList();
                }
                else listIntRateConfigDelete = listIntRateConfigDeleteTmp.Where(w => w.Id != 0).OrderBy(o => o.RecordSerialNo).ToList();

                if (listIntRateConfigDelete != null && listIntRateConfigDelete.Count != 0)
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.InterestRateConfigMasters.RemoveRange(listIntRateConfigDelete);
                        iCountDelete = await _dbContext.SaveChangesAsync();
                        bResult = (iCountDelete > 0) ? true : false;
                    }
                    else if (pFlagDelete == 2)
                    {
                        DateTime dCurrentDateVal = DateTime.Now;
                        int iCountTemp = 0;
                        List<InterestRateConfigMaster> listUpdateStatusClosed = new List<InterestRateConfigMaster>();
                        foreach (InterestRateConfigMaster itemUpdate in listIntRateConfigDelete)
                        {
                            itemUpdate.Status = StatusTrans.Status_Closed.Value;
                            itemUpdate.ModifiedBy = pUserName;
                            itemUpdate.ModifiedDate = dCurrentDateVal;
                            listUpdateStatusClosed.Add(itemUpdate);
                            iCountTemp++;
                        }
                        if (iCountTemp > 0)
                        {
                            _dbContext.InterestRateConfigMasters.UpdateRange(listUpdateStatusClosed);
                            iCountDelete = await _dbContext.SaveChangesAsync();
                        }
                        bResult = (iCountDelete > 0);
                    }
                    await transaction.CommitAsync();
                    return bResult;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteInterestRateConfigMaster('{pProductGroupCode}', {pDocumentId}, {pId}, '{pListId}','{pUserName}',{pFlagDelete}) => Error: {ex.Message}");
                return false;
            }
        }

        private string GetFileNameNewUpload(long pFileId, string pFileType, string pProductGroupCode, DateTime pAttachDate)
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
            if (pFileType == "1")
            {
                if (pProductGroupCode == ProductGroupCode.TIDE.Code)
                    sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Tide_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
                else if (pProductGroupCode == ProductGroupCode.CASA.Code)
                    sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Casa_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
                else if (pProductGroupCode == ProductGroupCode.DEPOSITPENAL.Code)
                    sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Penal_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
                else sFileNameNew = $"{FileType.FileType_ConfigIntRate.Code}_Unknown_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
            }
            else if (pFileType == "2")
            {
                sFileNameNew = $"{FileType.FileType_User_IDC.Code}_{pAttachDate.Year.ToString()}_{iFileIdTemp.ToString("D" + 10)}";
            }
            return sFileNameNew;
        }

        /// <summary>
        /// Hàm lấy danh sách tệp tin đính kèm Văn bản/Tài liệu/Quyết định
        /// </summary>
        /// <param name="pFileId">Chỉ số xác định bản ghi file đính kèm</param>
        /// <param name="pDocumentId">Chỉ số xác định văn bản/tài liệu/quyết định có tệp tin đính kèm</param>
        /// <param name="pFileType">Phân loại:  1 - File cấu hình lãi suất Tide/Casa/DepositPenal;
        ///                                     2 - File đính kèm của người dùng iDC;</param>
        /// <param name="pDocumentNumber">Số văn bản tài liệu đính kèm</param>
        /// <param name="pContentDescription">Mô tả file đính kèm</param>
        /// <returns>Danh sách tệp tin đính kèm Văn bản/Tài liệu/Quyết định</returns>
        public List<AttachedFileInfo> GetListAttachedFileInfoSearch(long pFileId, long pDocumentId, string pFileType, string pDocumentNumber, string pContentDescription)
        {
            try
            {
                List<AttachedFileInfo> listAttachedFileInfo = new List<AttachedFileInfo>();
                var listAttachedFileInfoTmp = _dbContext.AttachedFileInfos.Where(w => w.FileId != 0
                                                    && (pFileId == 0 || w.FileId == pFileId) && (pDocumentId == 0 || w.DocumentId == pDocumentId)
                                                    && (string.IsNullOrEmpty(pFileType) || w.FileType == pFileType)
                       )
                       .Where(delegate (AttachedFileInfo c)
                       {
                           if (string.IsNullOrEmpty(pDocumentNumber)
                               || (c.CircularRefNum != null && c.CircularRefNum.ToLower().Contains(pDocumentNumber.ToLower()))
                               || (c.CircularRefNum != null && Utilities.ConvertToUnSign(c.CircularRefNum.ToLower()).IndexOf(pDocumentNumber.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                               )
                               return true;
                           else
                               return false;
                       })
                       .OrderByDescending(o => o.DocumentId).ThenBy(o => o.FileId).ToList();
                if (!string.IsNullOrEmpty(pContentDescription))
                {
                    listAttachedFileInfo = listAttachedFileInfoTmp.Where(w => w.FileId != 0)
                       .Where(delegate (AttachedFileInfo c)
                       {
                           if (string.IsNullOrEmpty(pDocumentNumber)
                               || (c.ContentDescription != null && c.ContentDescription.ToLower().Contains(pDocumentNumber.ToLower()))
                               || (c.ContentDescription != null && Utilities.ConvertToUnSign(c.ContentDescription.ToLower()).IndexOf(pDocumentNumber.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                               )
                               return true;
                           else
                               return false;
                       })
                       .OrderByDescending(o => o.DocumentId).ThenBy(o => o.FileId).ToList();
                }
                else
                {
                    listAttachedFileInfo = listAttachedFileInfoTmp.ToList();
                }
                if (listAttachedFileInfo != null && listAttachedFileInfo.Count != 0)
                {
                    int iCountTemp = 0;
                    foreach (var item in listAttachedFileInfo)
                    {
                        iCountTemp++;
                        if (string.IsNullOrEmpty(item.CircularRefNum))
                            item.CircularRefNum = string.IsNullOrEmpty(item.DocumentNumber) ? "" : item.DocumentNumber;
                    }
                }
                return listAttachedFileInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        /// <returns>Chuỗi FileId được thêm mới hoặc cập nhật chỉnh sửa</returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<long>> SaveAttachedFileInfo(long pDocumentId, List<AttachedFileInfo> pListAttachedFiles, string pFileType, string pProductGroupCode, string pUserNameUpd)
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
                            objAttachedFileUpd.FileNameNew = GetFileNameNewUpload(itemAttachedFile.FileId, itemAttachedFile.FileType, pProductGroupCode, dCurrentDateTmp) + $"{itemAttachedFile.FileExtension}";
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
                            objAttachedFileUpdFileName.FileNameNew = GetFileNameNewUpload(objAttachedFileUpdFileName.FileId, "1", pProductGroupCode, dCurrentDateTmp);
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
                            bIsDeleteFile = Delete_File("", itemOld.PathFile);
                        else
                            bIsDeleteFile = Delete_File(itemOld.FileNameNew, itemOld.PathFile);

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

        public bool Delete_File(string pFileName, string pPathFile)
        {
            var fullPath = string.Format("{0}\\{1}", pPathFile, pFileName);
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }



        /// <summary>
        /// Hàm thực hiện Phê duyệt/Từ chối cấu hình lãi suất rút trước hạn tiền gửi CKH:
        ///  - Phê duyệt: Cập nhật trạng thái, ghi chú và các trường trạng thái gọi API cập nhật vào bảng InterestRateConfigMaster
        ///  - Từ chối: Cập nhật trạng thái, ghi chú cập nhật vào bảng InterestRateConfigMaster
        /// </summary>
        /// <param name="pDocumentId">Chỉ số xác định Quyết định thay đổi LS</param>
        /// <param name="pUserName">Tài khoản phê duyệt</param>
        /// <param name="pListId">Danh sách Id</param>
        /// <param name="pRejectFlag">Cờ phê duyệt: 1 - Từ chối, 0 - Phê duyệt</param>
        /// <param name="pRejectReason">Ghi chú hoặc Lý do từ chối</param>
        /// <returns>1: Thành công; 0 - Không thành công</returns>
        public async Task<int> SaveAuthorizeOrRejectTidePenalRateConfig(long pDocumentId, string pUserName, List<long> pListId, int pRejectFlag, string pRejectReason)
        {
            try
            {
                int iStatusAuthorizeOrReject = 0, iCountData = 0;
                iStatusAuthorizeOrReject = (pRejectFlag == 1) ? StatusTrans.Status_Rejected.Value : StatusTrans.Status_Authorized.Value;

                if (!pListId.Any())
                    return 0;
                if (pListId == null || pListId.Count == 0)
                    return 0;
                string sCircularRefNum = "", sCircularDate = "", sEffectiveDate = "";
                long iDocumentId = 0;
                DateTime dCurrentDateTmp = DateTime.Now;

                var listInterestRateConfigMasterUpd = await _dbContext.InterestRateConfigMasters.Where(x => pListId.Contains(x.Id)).ToListAsync();
                if (listInterestRateConfigMasterUpd != null && listInterestRateConfigMasterUpd.Count != 0)
                {
                    TidePenalIntRatesRequestViewModel objRequestInput = new TidePenalIntRatesRequestViewModel();
                    TidePenalInterestRatesRequestViewModel objTidePenalIntRateRequest = new TidePenalInterestRatesRequestViewModel();
                    List<RecordTidePenalInterestRateViewModel> listRecordTidePenalIntRate = new List<RecordTidePenalInterestRateViewModel>();
                    foreach (var itemUpd in listInterestRateConfigMasterUpd)
                    {
                        iCountData++;
                        itemUpd.Status = iStatusAuthorizeOrReject;
                        itemUpd.Remark = pRejectReason;
                        itemUpd.ApproverBy = pUserName;
                        itemUpd.ApprovalDate = dCurrentDateTmp;
                        sCircularRefNum = itemUpd.CircularRefNum;
                        sCircularDate = itemUpd.CircularDate.Value.ToString(FormatParameters.FORMAT_DATE_INT);
                        sEffectiveDate = itemUpd.EffectiveDate.ToString(FormatParameters.FORMAT_DATE_INT);
                        iDocumentId = itemUpd.DocumentId.Value;
                        RecordTidePenalInterestRateViewModel objChildRequestPenalIntRate = new RecordTidePenalInterestRateViewModel();
                        objChildRequestPenalIntRate.RecordSl = itemUpd.RecordSerialNo.ToString();
                        objChildRequestPenalIntRate.ProductCode = itemUpd.ProductCode;
                        objChildRequestPenalIntRate.CurrencyCode = itemUpd.CurrencyCode;
                        objChildRequestPenalIntRate.EffectiveDate = sEffectiveDate;
                        objChildRequestPenalIntRate.PosCode = (itemUpd.PosCode == "000000") ? "0" : itemUpd.PosCode;
                        objChildRequestPenalIntRate.InterestRate = itemUpd.NewInterestRate.ToString();
                        listRecordTidePenalIntRate.Add(objChildRequestPenalIntRate);
                    }
                    objTidePenalIntRateRequest.RecordTidePenalInterestRateViewModel = listRecordTidePenalIntRate;
                    objRequestInput.UserId = ConstValueAPI.UserId_Call_ApiIDC;
                    objRequestInput.TidePenalInterestRatesRequestViewModel = objTidePenalIntRateRequest;


                    if (iStatusAuthorizeOrReject == StatusTrans.Status_Authorized.Value)
                    {
                        var apiResponse = await _apiInternalEsbService.TidePenalRates(objRequestInput);
                        // Cập nhật trạng thái update vào core
                        foreach (var rec in listInterestRateConfigMasterUpd)
                        {
                            int iStatusUpdateCoreTmp = rec.StatusUpdateCore;
                            rec.CallApiTxnStatus = apiResponse.TxnStatus;
                            rec.CallApiResponseMsg = apiResponse.ResponseMsg;
                            rec.CallApiResponseCode = apiResponse.ResponseCode;
                            rec.CallApiReqRecordSl = int.Parse(apiResponse.StatusList.Where(w => w.ReqRecordSl == rec.RecordSerialNo.ToString()).FirstOrDefault()?.ReqRecordSl ?? "0");

                            if (apiResponse.TxnStatus.ToUpper() == ResultValueAPI.ResultValue_Status_Success.ToUpper())
                                iStatusUpdateCoreTmp++;
                            else iStatusUpdateCoreTmp = 0;

                            rec.StatusUpdateCore = iStatusUpdateCoreTmp;
                        }
                        var result = await _dbContext.SaveChangesAsync();
                        return (result > 0 && apiResponse.TxnStatus.ToUpper() == ResultValueAPI.ResultValue_Status_Success.ToUpper()) ? 1 : 0;
                    }
                    else
                    {
                        var result = await _dbContext.SaveChangesAsync();
                        return result;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SaveAuthorizeOrRejectTidePenalRateConfig({pDocumentId.ToString()},'{pUserName}','{pListId.Count.ToString()}',{pRejectFlag.ToString()},'{pRejectReason}') => Error: {ex.Message}");
                throw new Exception($"SaveAuthorizeOrRejectTidePenalRateConfig({pDocumentId.ToString()},'{pUserName}','{pListId.Count.ToString()}',{pRejectFlag.ToString()},'{pRejectReason}') => Error: {ex.Message}", ex);
            }
        }



        public async Task<string> DeleteInterestRateConfigureByIdList(List<long> lstId, string userName)
        {
            int result = 0;
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var id in lstId)
                {
                    string sResult = await DeleteInterestRateConfigureById(id, userName);
                    int iResult = int.Parse(sResult);
                    if (iResult == 0)
                    {
                        result = iResult;
                        await transaction.RollbackAsync();
                        break;
                    }
                }

                await transaction.CommitAsync();
                return result.ToString();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"An error occurred: {ex.Message}");
                return result.ToString();
            }
        }

    }
}