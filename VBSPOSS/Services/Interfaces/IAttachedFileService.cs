using Microsoft.AspNetCore.Mvc;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IAttachedFileService
    {
        Task<List<AttachedFileInfoView>> GetttachedFileSync(string pPosCode,
       string pFileType, string pFromTranDateFind, string pToTranDateFind, string pFileName);

        Task<string> UploadFileAsync(IFormFile file, string description, string createdBy, string valueFileType, string DocumentNumber);

        DownloadFileResult DownloadFile(long fileId, string fileName);

        /// <summary>
        /// Hàm cập nhật thông tin bảng dữ liệu AttachedFileInfo (Tệp tin đính kèm văn bản/tài liệu/quyết định cấu hình lãi suất)
        /// </summary>
        /// <param name="pDocumentId">Chỉ số xác định băn bản/tài liệu/quyết định có file đính kèm</param>
        /// <param name="pListAttachedFiles">Danh sách file đính kèm</param>
        /// <param name="pFileType">Phân loại:  1 - File cấu hình lãi suất Tide/Casa/DepositPenal;
        ///                                     2 - File đính kèm của người dùng iDC;</param>
        /// <param name="pProductGroupCode">Phân nhóm sản phẩm Tide hay Casa. Giá trị: CASA/TIDE/DEPOSITPENAL</param>
        /// <param name="pUserNameUpd">Người thực hiện</param>
        /// <param name="pTypeChild">Mã phụ nếu muốn đưa vào tên file/param>
        /// <returns>Chuỗi FileId được thêm mới hoặc cập nhật chỉnh sửa</returns>
        /// <exception cref="Exception"></exception>
        Task<List<long>> SaveAttachedFileInfo(long pDocumentId, List<AttachedFileInfo> pListAttachedFiles, string pFileType, string pProductGroupCode,
                    string pUserNameUpd, string pTypeChild);



    }
}
