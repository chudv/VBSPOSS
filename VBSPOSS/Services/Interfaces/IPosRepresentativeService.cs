using Kendo.Mvc.UI;
using VBSPOSS.ViewModels;

public interface IPosRepresentativeService
{
    Task<List<PosRepresentativeViewModel>> GetPosRepresentativeListByApi(string pPosCode, string pStaffId);
    Task<string> UpdatePosRepresentative(PosRepresentativeViewModel model, int pFlagCall, string pUserName);
    bool DeletePosRepresentative(string pStaffId, string pEffectDate, string pExpireDate, int pStatus, string pModifiedBy, int pFlagDelete);
    List<PosRepresentativeViewModel> GetPosRepresentativeList(string pPosCode, string pStaffId,string pStaffName, int pStatus);
    bool ChangeStatusPosRepresentative(string pPosCode,string pModifiedBy);
    bool HasOpenPosRepresentative(string posCode);
}
