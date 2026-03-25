using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.NetworkInformation;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.Model;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class NotiService : INotiService
    {
        /// <summary>
        /// Defines the _dbContext.
        /// </summary>
        private readonly ApplicationDbContext _dbContext;
       

        /// <summary>
        /// Defines the _mapper.
        /// </summary>
        private readonly IMapper _mapper;
        private readonly IApiNotiGatewayService _notiService;
        private readonly ILogger<NotiService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfValueService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        public NotiService(ApplicationDbContext dbContext, IMapper mapper, IApiNotiGatewayService notiService, ILogger<NotiService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _notiService = notiService;
            _logger = logger;
        }

        /// <summary>
        /// Hàm lấy danh sách loại thông báo tại bảng NotiTemp
        /// </summary>
        /// <returns>Danh sách Xã/Phường/Thị trấn</returns>
        public List<NotiTempViewModel> GetNotiTypeList(string pId, string pNotiType)
        {
            var answer = new List<NotiTempViewModel>();
            try
            {
                var profileNotiType = _dbContext.NotiTemps.Where(w => string.IsNullOrEmpty(pId)|| (w.Id ==  Int32.Parse(pId))
                                        && (string.IsNullOrEmpty(pNotiType) || w.NotiType == pNotiType)
                                        ).OrderBy(o => o.Id).ThenBy(o => o.NotiType).ToList();
                
                if(profileNotiType != null)
                {
                    foreach (var item in profileNotiType)
                    {
                        NotiTempViewModel objItem = new NotiTempViewModel();
                        objItem = _mapper.Map<NotiTempViewModel>(item);                      
                        answer.Add(objItem);
                    }
                }                               
                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Hàm cập nhật dữ liệu (Thêm/Sửa) vào bảng dữ liệu NOTI_TEMP (Thông tin Danh mục đề mục)
        /// </summary>
        /// <param name="model">NOTI_TEMP (Thông tin Danh mục Đề mục)</param>
        /// <param name="pUserName">Tên đăng nhập của người dùng thao tác chức năng</param>
        /// <returns>Id bảng ghi vừa được cập nhật</returns>
        public int UpdateNotiTemp(NotiTempViewModel model, string pUserName)
        {
            int iResultId = 0, iSaveChanges = 0;
            try
            {
                DateTime currentDateVal = DateTime.Now;
                var objNotiTemp = _dbContext.NotiTemps.Where(m => m.Id == model.Id).FirstOrDefault();

                if (objNotiTemp != null && objNotiTemp.Id != 0)
                {
                    objNotiTemp.NotiType = model.NotiType;
                    objNotiTemp.SmsTemp = model.SmsTemp;
                    objNotiTemp.OttTemp = model.OttTemp;
                    objNotiTemp.EmailTemp = model.EmailTemp;
                    objNotiTemp.Status = model.Status;
                    objNotiTemp.ModifiedBy = pUserName;
                    objNotiTemp.ModifiedDate = currentDateVal;
                    objNotiTemp.MailSubject = model.MailSubject;
                    _dbContext.Entry(objNotiTemp).Property(x => x.NotiType).IsModified = true;
                    _dbContext.Entry(objNotiTemp).Property(x => x.SmsTemp).IsModified = true;
                    _dbContext.Entry(objNotiTemp).Property(x => x.OttTemp).IsModified = true;
                    _dbContext.Entry(objNotiTemp).Property(x => x.EmailTemp).IsModified = true;
                    _dbContext.Entry(objNotiTemp).Property(x => x.Status).IsModified = true;
                    _dbContext.Entry(objNotiTemp).Property(x => x.ModifiedBy).IsModified = true;
                    _dbContext.Entry(objNotiTemp).Property(x => x.ModifiedDate).IsModified = true;
                    _dbContext.Entry(objNotiTemp).Property(x => x.MailSubject).IsModified = true;

                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = objNotiTemp.Id;
                }
                else
                {
                    NotiTemp objModelNotiTemp = new NotiTemp();
                    objModelNotiTemp.NotiType = model.NotiType;
                    objModelNotiTemp.SmsTemp = model.SmsTemp;
                    objModelNotiTemp.OttTemp = model.OttTemp;
                    objModelNotiTemp.EmailTemp = model.EmailTemp;
                    objModelNotiTemp.Status = "1";
                    objModelNotiTemp.CreatedBy = pUserName;
                    objModelNotiTemp.CreatedDate = currentDateVal;
                    objModelNotiTemp.ModifiedBy = pUserName;
                    objModelNotiTemp.ModifiedDate = currentDateVal;
                    objModelNotiTemp.MailSubject = model.MailSubject;

                    _dbContext.NotiTemps.Add(objModelNotiTemp);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = objModelNotiTemp.Id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iResultId;
        }
        
         public string GetExtentionByFileName(string sfileName)
         {
            string sExtFile = "";
            if (!string.IsNullOrEmpty(sfileName))
            {
                sExtFile = sfileName.Substring(sfileName.LastIndexOf(".") + 1);
            }
            return sExtFile.ToLower();
         }

        /// <summary>
        /// Hàm Truy vấn thông báo NotiGateway
        /// </summary>
        /// <returns>Danh sách bản ghi các trong logging</returns>
        public async Task<List<NotiTempViewModel>> GetNotiTemplate(string pStatus, string pNotiTemp)
        {
            var answer = new List<NotiTempViewModel>();
            try
            {
                int iCount = 0;
                var listNoti = await _notiService.GetListNotiTempAsync(pStatus);

                List<NotiTempViewModel> notiListLogs = new List<NotiTempViewModel>();
                if (listNoti != null)
                {
                    notiListLogs = listNoti.Where(s => string.IsNullOrEmpty(pNotiTemp)|| s.NotiType == pNotiTemp
                                   && (string.IsNullOrEmpty(pStatus) || s.Status == pStatus)).OrderBy(o => o.NotiType).ToList();                    
                    if (notiListLogs != null)
                    {
                        foreach (var item in notiListLogs)
                        {
                            NotiTempViewModel objItem = new NotiTempViewModel();
                            iCount++;
                            objItem.Id = iCount;
                            objItem.NotiType = item.NotiType;
                            objItem.SmsTemp = item.SmsTemp;
                            objItem.OttTemp = item.OttTemp;
                            objItem.EmailTemp = item.EmailTemp;
                            objItem.Status = item.Status;
                            objItem.CreatedBy = item.CreatedBy;
                            objItem.CreatedDate = item.CreatedDate;
                            objItem.ModifiedBy = item.ModifiedBy;
                            objItem.ModifiedDate = item.ModifiedDate;
                            objItem.NotiSend = item.NotiSend;
                            objItem.Detail = item.Detail;
                            objItem.Description = item.Description;
                            objItem.NotiLink = item.NotiLink;
                            objItem.MailSubject = item.MailSubject;
                            if (item.Status.Contains("A"))
                                objItem.StatusHT = "Mở";
                            else if (item.Status.Contains("C"))
                                objItem.StatusHT = "Đóng";
                            objItem.SmsTempHT = string.IsNullOrEmpty(item.SmsTemp)?"":"x";
                            objItem.OttTempHT = string.IsNullOrEmpty(item.OttTemp)?"":"x";
                            objItem.EmailTempHT = string.IsNullOrEmpty(item.EmailTemp)?"":"x";
                            answer.Add(objItem);
                        }
                    }
                }
                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Hàm xóa dữ liệu Noti
        /// </summary>
        /// <returns>Id bảng ghi vừa được cập nhật</returns>
        public async Task<string> DeleteNotiTemp(string pNotiType, string pStatus)
        {
            string answer = "";
            try
            {
                var listNoti = await _notiService.GetListNotiTempAsync(pStatus);
                if (listNoti != null)
                {
                    var notiListLogs = listNoti.Where(s => s.NotiType == pNotiType
                                   && (s.Status == pStatus)).OrderBy(o => o.NotiType).ToList();                    
                    if (notiListLogs != null)
                    {
                        foreach (var item in notiListLogs)
                        {
                            NotiMsgTempRequest notiMsg = new NotiMsgTempRequest();
                            notiMsg.NotiType= pNotiType;
                            notiMsg.Status= "C";
                            var resultNoti = await _notiService.UpdateNotiMsgTempAsync(notiMsg);
                            if (resultNoti != null)
                            {
                                answer = "0";
                            }
                            else
                                answer = "1";
                        }
                    }                    
                }
                else
                    answer = "1";
                return answer;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi xóa tài khoản người dùng: {ex.Message}");
                return "2";
            }
        }
    }
}
