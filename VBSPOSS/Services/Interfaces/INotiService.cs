
using VBSPOSS.Data.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface INotiService
    {
        int UpdateNotiTemp(NotiTempViewModel model, string pUserName);

        string GetExtentionByFileName(string sfileName);

        List<NotiTempViewModel> GetNotiTypeList(string pId, string pNotiType);

        Task<List<NotiTempViewModel>> GetNotiTemplate(string pStatus, string pNotiTemp);

        Task<string> DeleteNotiTemp(string pNotiTemp, string pStatus);

    }
}
