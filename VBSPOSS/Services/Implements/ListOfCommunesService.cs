// ListOfCommunesService.cs
using AutoMapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using VBSPOSS.Data;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;
using VBSPOSS.Constants;
using VBSPOSS.Utils;
using VBSPOSS.Data.OSS.Models;

namespace VBSPOSS.Services.Implements
{
    public class ListOfCommunesService : IListOfCommunesService
    {
        /// <summary>
        /// Defines the _dbContext.
        /// </summary>
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Defines the _mapper.
        /// </summary>
        private readonly IMapper _mapper;

        private readonly IListOfValueService _serviceLOV;
        private readonly ILogger<ListOfCommunesService> _logger;
        private readonly IApiInternalService _internalServiceAPI;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfCommunesService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        /// <param name="serviceLOV">The serviceLOV<see cref="IListOfValueService"/>.</param>
        /// <param name="logger">The logger<see cref="ILogger{ListOfCommunesService}"/>.</param>
        /// <param name="internalServiceAPI">The internalServiceAPI<see cref="IApiInternalService"/>.</param>
        public ListOfCommunesService(
            ApplicationDbContext dbContext,
            IMapper mapper,
            IListOfValueService serviceLOV,
            ILogger<ListOfCommunesService> logger,
            IApiInternalService internalServiceAPI)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _serviceLOV = serviceLOV;
            _logger = logger;
            _internalServiceAPI = internalServiceAPI;
        }

        /// <summary>
        /// Lấy danh sách xã/phường theo các tiêu chí lọc.
        /// </summary>
        public List<ListOfCommunesViewModel> GetLovCommuneList( string pProvinceCode, string pDistrictCode, string pCommuneCode, string pPosCode, string pSubCommuneCode)
        {
            var answer = new List<ListOfCommunesViewModel>();
            try
            {
                if (string.IsNullOrEmpty(pPosCode)
                    && string.IsNullOrEmpty(pProvinceCode)
                    && string.IsNullOrEmpty(pDistrictCode)
                    && string.IsNullOrEmpty(pCommuneCode))
                {
                    return answer;
                }

                var profileBranchTMPs = _dbContext.ListOfCommune
                    .Where(w =>
                        (string.IsNullOrEmpty(pProvinceCode) || w.ProvinceCode == pProvinceCode)
                        && (string.IsNullOrEmpty(pDistrictCode) || w.DistrictCode == pDistrictCode)
                        && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
                        && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode == pCommuneCode)
                        && (string.IsNullOrEmpty(pSubCommuneCode) || w.SubCommuneCode == pSubCommuneCode)
                    )
                    .OrderBy(o => o.ProvinceCode)
                    .ThenBy(o => o.DistrictCode)
                    .ThenBy(o => o.CommuneCode)
                    .ThenBy(o => o.SubCommuneCode)
                    .ToList();

                answer = _mapper.Map<List<ListOfCommunesViewModel>>(profileBranchTMPs);
                return answer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi GetLovCommuneList: Province={Province}, District={District}",
                    pProvinceCode, pDistrictCode);
                throw;
            }
        }

        /// <summary>
        /// Thêm mới một xã/phường.
        /// </summary>
        public bool CreateCommune(ListOfCommunesViewModel model, string createdBy)
        {
            try
            {
                if (model == null) return false;

                bool exists = _dbContext.ListOfCommune
                    .Any(w => w.CommuneCode == model.CommuneCode
                           && w.PosCode == model.PosCode);
                if (exists)
                    throw new InvalidOperationException(
                        $"Xã/Phường có mã '{model.CommuneCode}' đã tồn tại trong POS '{model.PosCode}'.");

                var entity = _mapper.Map<ListOfCommunes>(model);
                entity.CreatedBy = createdBy;
                entity.CreatedDate = DateTime.Now;
                entity.ModifiedBy = createdBy;
                entity.ModifiedDate = DateTime.Now;

                _dbContext.ListOfCommune.Add(entity);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi CreateCommune: CommuneCode={CommuneCode}", model?.CommuneCode);
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông tin một xã/phường.
        /// </summary>
        public bool UpdateCommune(ListOfCommunesViewModel model, string modifiedBy)
        {
            try
            {
                if (model == null) return false;

                var entity = _dbContext.ListOfCommune
                    .FirstOrDefault(w => w.CommuneCode == model.CommuneCode
                                      && w.PosCode == model.PosCode);
                if (entity == null)
                    throw new KeyNotFoundException(
                        $"Không tìm thấy Xã/Phường mã '{model.CommuneCode}'.");

                _mapper.Map(model, entity);
                entity.ModifiedBy = modifiedBy;
                entity.ModifiedDate = DateTime.Now;

                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi UpdateCommune: CommuneCode={CommuneCode}", model?.CommuneCode);
                throw;
            }
        }

        /// <summary>
        /// Xóa một xã/phường theo mã xã và mã POS.
        /// </summary>
        public bool DeleteCommune(string pCommuneCode, string pPosCode)
        {
            try
            {
                if (string.IsNullOrEmpty(pCommuneCode)) return false;

                var entity = _dbContext.ListOfCommune
                    .FirstOrDefault(w => w.CommuneCode == pCommuneCode
                                      && w.PosCode == pPosCode);
                if (entity == null)
                    throw new KeyNotFoundException(
                        $"Không tìm thấy Xã/Phường mã '{pCommuneCode}'.");

                _dbContext.ListOfCommune.Remove(entity);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi DeleteCommune: CommuneCode={CommuneCode}", pCommuneCode);
                throw;
            }
        }
    }
}