using VBSPOSS.Data.Models;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IInterestRateConfigureService
    {


        /// <summary>
        /// Hàm lấy danh sách cấu hình lãi suất theo tham số truyền vào.    
        /// </summary>
        /// <param name="posCode"></param>
        /// <param name="productCode"></param>
        /// <param name="circularRefNum"></param>
        /// <param name="fromEffectiveDate"></param>
        /// <param name="toEffectiveDate"></param>
        /// 
        /// 
        /// <returns></returns>
        /// 
        //add thêm phần mã sản phẩm
        Task<List<CasaRateProductViewModel>> GetCasaRateByProductsAndTypesAsync(List<string> productCodes,List<string> accountTypes,string posCode,DateTime referenceDate);
        Task<List<InterestRateConfigMasterView>> GetInterestRateConfigMasterViewListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, DateTime? fromDate, DateTime? toDate, string searchText, int? statusDesc= null);

        //    Task<List<InterestRateConfigMaster>> GetInterestRateConfigMasterListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, DateTime? fromEffectiveDate, DateTime? toEffectiveDate);
        //add lại
        Task<List<CasaRateProductViewModel>> GetCasaTermsAsync(List<string> intRateConfigIds);

        /// <summary>
        /// Hàm cập nhật thông tin da sách cấu hình lãi suất.   
        /// </summary>
        /// <param name="interestRateConfigMasters"></param>
        /// <returns></returns>
        Task<int> SaveInterestRateConfigMasterAsync(List<InterestRateConfigMaster> interestRateConfigMasters);


        /// <summary>
        /// Hàm xóa thông tin cấu hình lãi suất theo Id.    
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<int> DeleteInterestRateConfigMasterAsync(long id, string userId);


        /// <summary>
        /// Hàm lấy chi tiết thông tin tờ trình cấu hình lãi suất theo Id. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<DocumentModel> GetDocumentInfoByIdAsync(long id);


        /// <summary>
        /// Hàm lưu thông tin tập tin đính kèm cho cấu hình thay đổi lãi suất.
        /// </summary>
        /// <param name="configureId"></param>
        /// <param name="attachedFiles"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<long>> SaveAttachedFiles(long configureId, List<AttachedFileInfo> attachedFiles, string userId);

        /// <summary>
        /// Hàm xóa tập tin đính kèm theo FileId và userId. 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<int> DeleteAttachedFilesAsync(long fileId, string userId);



        //add
        Task<int> UpdateInterestRateConfigMasterAsync(List<InterestRateConfigMaster> configs);


        Task<int> UpdateInterestRateConfigMasterStatus(string userName, List<long> lstId, int status, long documentId);

        // Task<List<InterestRateConfigMaster>> GetInterestRateConfigMastersAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, DateTime? fromEffectiveDate, DateTime? toEffectiveDate); // Lấy danh sách cấu hình lãi suất


        // Lấy chi tiết sản phẩm theo mã nhóm sản phẩm và mã sản phẩm
        Task<List<TideTermViewModel>> GetTideProdList(string posCode, string productCode, DateTime? effectDate);


        Task<List<TideTermViewModel>> GetTideProdList(string sessionId, string userName, string posCode, List<string> productTypes, DateTime? effectDate, string sourceFlag);

        //Task<List<AddCasaProductViewModel>> GetCasaProdList(string posCode, string productCode, DateTime? effectDate);

        // Task<List<CasaRateProductViewModel>> GetCasaProdList(string posCode, string productCode, DateTime? effectDate);

        Task<string> SaveTideRateConfigureData(TideRateConfigureViewModel intRateConfigMaster, List<TideTermViewModel> intRateConfigDetails, string userId); // Lưu cấu hình lãi suất


        Task<string> DeleteInterestRateConfigureById(long id, string userName);

        Task<string> DeleteInterestRateConfigureByIdList(List<long> lstId, string userName);

        Task<string> DeleteInterestRateConfigureByDocumentId(long documentId, string userName);

        Task<int> UpdateTideConfigureTemp(List<DepositTermModel> depositTerms, double interestRate, string sessionId, string userName, string userPosCode);


        //add Casa

        Task<string> SaveCasaRateConfigureData(AddCasaProductViewModel model, List<CasaRateProductViewModel> gridItems, string userName);
        // Task<string> UpdateCasaConfigureGridData(CasaRateProductViewModel model, string userName);


        //add Casa
        Task<string> CloseCasaById(long id, string userName);
        Task<string> CloseCasaByDocumentId(long documentId, string userName);

        //
        //add update casa
        Task<List<InterestRateConfigMasterModel>> GetCasaListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, string fromEffectiveDate, string toEffectiveDate);
        //  Task<InterestRateConfigMasterModel> GetCasaByIdAsync(long id);
        Task<string> UpdateCasaAsync(InterestRateConfigMasterModel model, string userName);
        //Task<string> SubmitCasaForApprovalAsync(long id, string userId);
        //add attach file
        //Task<List<AttachedFileInfo>> GetAttachedFilesAsync(long documentId);

        Task<long> CreateNewDocumentId();
        Task<long> CreateNewDocumentWithMaster(string userId);
        Task<string> GetCircularRefNumByDocumentIdAsync(long documentId);


        Task<List<InterestRateConfigMasterModel>> GetInterestRateConfigMasterListAsync(string productGroupCode, string posCode, string productCode,string circularRefNum, DateTime? fromEffectiveDate, DateTime? toEffectiveDate);

        //add
        //Task<List<InterestRateConfigMasterModel>> GetInterestRateConfigMasterViewListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, DateTime? fromDate, DateTime? toDate);

        Task<InterestRateConfigMasterModel> GetCasaByIdAsync(long id);

        //Task<List<CasaRateProductViewModel>> GetCasaProdList(string posCode, string productCode, DateTime effectiveDate);

        Task<List<CasaRateProductViewModel>> GetCasaProdList(string posCode, string productCode, DateTime? effectDate);


        Task UpdateCasaDocumentIdAsync(long casaId, long documentId);
        //  Task<(byte[] fileBytes, string fileName, string contentType)> GetAttachedFileForDownloadAsync(long fileId);
        // Task<List<InterestRateConfigMasterModel>> GetCasaListAsync(string productGroupCode, string posCode, string productCode, string circularRefNum, string fromEffectiveDate, string toEffectiveDate);

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
        List<InterestRateConfigMasterView> GetListInterestRateConfigMasterViews(string pMainPosCode, string pPosCode, string pProductGroupCode, string pCircularRefNum,
                    int pFromEffectiveDate, int pToEffectiveDate, long pDocumentId, string pListId);


        Task<string> SaveBatchTideRateConfigureData(TideRateConfigureViewModel intRateConfigMaster, List<TideTermViewModel> intRateConfigDetails, string userId, string userPosCode);

        Task<List<TideTermViewModel>> GetTideTermsAsync(List<string> intRateConfigIds);


        Task<InterestRateConfigMasterViewModel> GetTideInterestRateDetailViews(string circularRefNum, string effectDate);

        Task<InterestRateConfigMasterViewModel> GetCasaInterestRateDetailViews(string circularRefNum, string effectDate);


        //add show approve
        Task<(AddCasaProductViewModel summaryModel, List<long> ids)> GetCasaMasterListByFilterAsync(
            string productGroupCode,
            string posCode,
            string productCode,
            string circularRefNum,
            DateTime? fromDate,
            DateTime? toDate,
            bool isViewModel = false);  // Optional param cho Approval (ViewModel) vs Document (AddCasaProductViewModel)

        // THÊM: Các signature khác nếu dùng (batch update + ApplyPosList)
        Task UpdateCasaDocumentIdBatchAsync(List<long> ids, long documentId);
        Task<string> GetApplyPosListByIdsAsync(List<long> intRateConfigIds);


        // Upload File
        Task<long> SaveAttachedFileAsync(AttachedFileInfo file);


        Task<List<AttachedFileInfo>> GetAttachedFilesAsync(long documentId);

        Task<List<AttachedFileInfo>> GetAttachedFilesAsync(string documentNumber);

        Task<(byte[] fileBytes, string fileName, string contentType)> GetAttachedFileForDownloadAsync(long fileId);

        Task<AttachedFileInfo> GetAttachedFileById(long fileId);

        // Task<(byte[] bytes, string fileName, string contentType)> GetAttachedFileForDownloadAsync(long fileId);
        Task<int> DeleteAttachedFileAsync(long fileId, string deletedBy);

        List<InterestRateConfigMasterViewModel> GetListInterestRateConfigMasterViewsForTide(string userPos, string pPosCode, string pProductGroupCode,
                        string pCircularRefNum, int pFromEffectiveDate, int pToEffectiveDate, long pDocumentId, string searchText, int status);

        /// <summary>
        /// Hàm phê duyệt/từ chối đề nghị thay đổi lãi suất  
        /// </summary>
        /// <param name="userName">Tài khoản phê duyệt</param>
        /// <param name="lstId">Danh sách Id</param>
        /// <param name="rejectFlag">Cờ từ chối: 1 - từ chối, 0 - phê duyệt</param>
        /// <returns></returns>
        Task<int> SaveApprovalDecision(string userName, List<long> lstId, int rejectFlag, string rejectReason);

        /// <summary>
        /// Hàm kiểm tra số quyết định và ngày quyết định đã tồn tại hay chưa   
        /// </summary>
        /// <param name="circularRefNum"></param>
        /// <param name="circularDate"></param>
        /// <returns></returns>
        bool CheckCircular(string circularRefNum, DateTime circularDate);

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
        Task<List<UpdateTidePenalRateConfigViewModel>> GetListDepPenalIntRate(string pPosCode, string pProductCode, string pCurrencyCode, DateTime pEffectDate,
                            DateTime pExpiredDate, string pCircularRefNum, DateTime pCircularDate, decimal pInterestRateNew, decimal pPenalIntRate);

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
        Task<List<UpdateTidePenalRateConfigViewModel>> GetListDepPenalIntRate(string pPosCode, string pProductCode, string pCurrencyCode, DateTime pFromEffectDate,
                                    DateTime pToEffectDate, string pCircularRefNum, DateTime pCircularDate, string pListId);

        /// <summary>
        /// Hàm thực hiện thêm mới hoặc cập nhật thay đổi thông tin cấui hình lãi suất vào bảng InterestRateConfigMaster.
        /// Trường hợp thêm mới thì sẽ bao gồm cả cập nhật nếu Id<>0
        /// </summary>
        /// <param name="pListInterestRateConfigMasterUpds">Danh sách InterestRateConfigMaster cần cập nhật</param>
        /// <param name="pUserNameUpd">Người cập nhật</param>
        /// <param name="pFlagCall">1: Thêm mới; 2: Chỉnh sửa</param>
        /// <returns>Số bản ghi được cập nhật</returns>
        /// <exception cref="Exception"></exception>
        Task<int> SaveInterestRateConfigMaster(List<InterestRateConfigMaster> pListInterestRateConfigMasterUpds, string pUserNameUpd, string pFlagCall);

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
        Task<bool> DeleteInterestRateConfigMaster(string pProductGroupCode, long pDocumentId, long pId, string pListId, string pUserName, int pFlagDelete);

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
        List<AttachedFileInfo> GetListAttachedFileInfoSearch(long pFileId, long pDocumentId, string pFileType, string pDocumentNumber, string pContentDescription);

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
        Task<List<long>> SaveAttachedFileInfo(long pDocumentId, List<AttachedFileInfo> pListAttachedFiles, string pFileType, string pProductGroupCode, string pUserNameUpd);


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
        Task<int> SaveAuthorizeOrRejectTidePenalRateConfig(long pDocumentId, string pUserName, List<long> pListId, int pRejectFlag, string pRejectReason);



        Task<List<CasaRateProductViewModel>> GetCasaRateByProductsAndTypesAsyncWithHO(List<string> productCodes, List<string> accountTypes, string posCode,
      DateTime referenceDate);
    }

}
