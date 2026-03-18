using VBSPOSS.Integration.Model;
using VBSPOSS.Integration.ViewModel;

namespace VBSPOSS.Integration.Interfaces
{
    public interface IApiInternalService
    {
        /// <summary>
        /// Hàm lấy danh sách cán bộ NHCSXH theo POS truyền vào. Hàm sẽ gọi lấy dữ liệu từ API internal: list-staff-vbsp-by-poscode?pPosCode=002505
        /// </summary>
        /// <param name="pPosCode">Mã POS cần lấy danh sách cán bộ</param>
        /// <returns>Danh sách cán bộ NHCSXH</returns>
        Task<GenericResultInternalGateway<List<StaffVbspInforViewModel>>> GetListStaffVBSP(string pPosCode);

        /// <summary>
        /// Hàm lấy danh sách cán bộ NHCSXH theo Id cán bộ truyền vào. Hàm sẽ gọi lấy dữ liệu từ API internal: list-staff-vbsp-by-staffId?pStaffId=THGA00000000166
        /// </summary>
        /// <param name="pStaffId">Id cán bộ cần lấy thông tin Ex: THGA00000000166</param>
        /// <returns>Danh sách cán bộ NHCSXH</returns>
        Task<GenericResultInternalGateway<List<StaffVbspInforViewModel>>> GetListStaffByStaffId(string pStaffId);
    }
}
