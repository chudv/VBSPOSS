using VBSPOSS.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface ITransferDataPosService
    {
        /// <summary>
        /// Hàm lấy danh sách yêu cầu điều chuyển dữ liệu khác pos
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã Pos (Không bắt buộc). Nếu lấy cả chi nhánh thì truyền vào 4 ký tự đầu của POS Chi nhánh</param>
        /// <param name="pTxnPointCode">Mã điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pTxnPointName">Tên điểm gioa dịch</param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất truyền vào -1</param>
        /// <param name="pTxnLocation">Địa điểm giao dịch (Không bắt buộc)</param>
        /// <param name="pEventCode">Tìm kiếm theo bản ghi có yêu cầu nghiệp vụ với điểm giao dịch (Không bắt buộc)</param>
        /// <returns>Danh sách điểm giao dịch theo Model ListOfTransPointWorkViewModel</returns>
        List<TransferDataPosMasterViewModel> GetListOfTranferDataPosSearch(string pProvinceCode, string pPosCode, string pTxnPointCode, string pTxnPointName,
                                int pStatus, string pTxnLocation, string pEventCode);

        List<ValueConstModel> GetListPosOfBranch(string mainPos);

         Task<long> SaveTranferPosMaster(TransferDataPosMasterViewModel tranferMaster, string pUserNameUpd, string pFlagCall, string pButtonType);

        List<ValueConstModel> GetListSubCommuneOfPos(string posCd);

        string GetPosName(string posCode);

         bool CheckExistTransferDataPosMaster(
    string fromPosCode,
    string toPosCode,
    DateTime? effectiveDate);
    }
}
