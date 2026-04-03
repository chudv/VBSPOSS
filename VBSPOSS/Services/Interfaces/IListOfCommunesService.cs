// IListOfCommunesService.cs
using System.Collections.Generic;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IListOfCommunesService
    {
        List<ListOfCommunesViewModel> GetLovCommuneList(
            string pProvinceCode,
            string pDistrictCode,
            string pCommuneCode,
            string pPosCode,
            string pSubCommuneCode
        );

        bool CreateCommune(ListOfCommunesViewModel model, string createdBy);
        bool UpdateCommune(ListOfCommunesViewModel model, string modifiedBy);
        bool DeleteCommune(string pCommuneCode, string pPosCode);
    }
}