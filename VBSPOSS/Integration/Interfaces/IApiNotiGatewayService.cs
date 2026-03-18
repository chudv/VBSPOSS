using VBSPOSS.Integration.Model;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Integration.Interfaces
{
    public interface IApiNotiGatewayService
    {
        /// <summary>
        /// Hàm gọi sang ReportGateway để in báo cáo
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        Task<string> GetNotiByTypeAsync(string notiType, string sendType);
        Task<string> UpdateNotiMsgTempAsync(NotiMsgTempRequest request);
        Task<List<NotiTempViewModel>> GetListNotiTempAsync(string pStatus);


    }
}
