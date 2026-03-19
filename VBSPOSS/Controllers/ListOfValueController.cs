using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telerik.SvgIcons;
using VBSPOSS.Constants;
using VBSPOSS.Data.Models;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Integration.ViewModel;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Controllers
{
    public class ListOfValueController : BaseController
    {
        /// <summary>
        /// Defines the _logger.
        /// </summary>
        private readonly ILogger<ListOfValueController> _logger;

        /// <summary>
        /// Defines the _service.
        /// </summary>
        private readonly IListOfValueService _serviceLOV;
        //private readonly IAdministrationService _userService;
        private readonly IProductService _productService;

        private readonly IApiInternalService _internalServiceAPI;

        private readonly IInterestRateConfigureService _intRateConfigService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListController"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="service">The service<see cref="IListOfValueService"/>.</param>
        /// <param name="logger">The logger<see cref="ILogger{ListController}"/>.</param>
        /// <param name="menuService">The menuService<see cref="IPermitService"/>.</param>
        public ListOfValueController(ILogger<BaseController> logger, IAdministrationService adminService, IListOfValueService serviceLOV, ISessionHelper sessionHelper, 
                                IProductService productService, IApiInternalService internalServiceAPI, IInterestRateConfigureService intRateConfigService,
                                IAdministrationService userService) : base(logger, adminService, sessionHelper)
        {
            _serviceLOV = serviceLOV;
            _productService = productService;
            _internalServiceAPI = internalServiceAPI;
            _intRateConfigService = intRateConfigService;
            //_userService = userService;
        }

        /// <summary>
        /// Menu gọi hiển thị danh sách Danh mục chung
        /// </summary>
        /// <param name="pMenuId">Chỉ số xác định Menu</param>
        /// <returns>View danh sách danh mục chung</returns>
        public IActionResult IndexListOfValue()
        {
            /*
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            //Set gia tri de kiem tra quyen truy cap
            SetPermitData(actionName, controllerName, menuId);

            int permit = UserPermit;
            TempData["UserName"] = UserName;
            TempData["UserPosCode"] = UserPosCode;
            if (permit == Permit._VIEW)
            {
                return View("IndexListMain_View");
            }
            else if (permit == Permit._EDIT)
            {
                return View("IndexListOfValue");
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            */
            return View("IndexListOfValue");
        }





        /// <summary>
        /// Hiển thị danh mục lên Combobox cho các màn hình chức năng.
        /// </summary>
        /// <param name="pFlagCallRoot">Cờ xác định Chỉ lấy danh mục gốc/con/tất cả. 0 - Lấy tất cả; 1 - Chỉ Lấy gốc; 2 - Chỉ Lấy dm con.</param>
        /// <param name="pParentId">Phân loại danh mục cần lấy dữ liệu. Ex: ParentLovValue.Parent_Title_Id là Chức vụ</param>
        /// <param name="pStatus">Trạng thái: -1: Lấy tất; 1: Lấy danh mục mở.</param>
        /// <param name="pTenVT">Lấy tên theo tên đầy đủ hay viết tắt: "0" - Lấy tên đầy đủ; "1" - Lấy theo tên viết tắt;.</param>
        /// <param name="pTitleChoice">Tiêu đề cho người dùng chọn danh sách.</param>
        /// <param name="pFlagShow">Cách hiển thị: 1 - Hiển thị tên danh mục; 2 - Hiển thị Mã hiệu và tên danh mục.</param>
        /// <param name="pPosCode">Mã POS để lấy những danh mục như Chức vụ/Phòng ban theo đơn vị đó (Đơn vị có liên quan bao nhiêu danh mục CV/PB thì hiển thị theo POS)</param>
        /// <param name="pCodeApply">Mã áp dụng cho danh mục muốn lấy lên. Đối với danh mục Chức vụ chỉ lấy những người ký VB thì truyền vào là 'LANHDAO'</param>
        /// <returns>.</returns>
        public JsonResult GetListMain_ByConditions(string pFlagCallRoot = "0", string pParentId = "", int pStatus = -1, string pTenVT = "1",
                                                   string pTitleChoice = "", string pFlagShow = "1", string pPosCode = "", string pCodeApply = "")
        {
            string sTitleChoice = "", sName = "", sShortName = "", sCode = "", sCodeApply = "";
            int iParentId = 0, iFlagCallRoot = 0;

            sTitleChoice = (pTitleChoice == "" || pTitleChoice == null) ? "Chọn danh mục" : pTitleChoice;
            iParentId = (pParentId == "") ? -1 : Convert.ToInt32(pParentId);
            iFlagCallRoot = (pFlagCallRoot == "") ? 0 : Convert.ToInt32(pFlagCallRoot);
            if (!string.IsNullOrEmpty(pPosCode))
                sCodeApply = _serviceLOV.GetCodeApplyByPosCode(pPosCode);
            else if (!string.IsNullOrEmpty(pCodeApply))
                sCodeApply = pCodeApply;
            ArrayList data = new ArrayList();
            var listProfices = _serviceLOV.GetListOfValueSearch(iParentId, "", 0, "", "", pStatus, iFlagCallRoot);
            if (sTitleChoice != "")
                data.Add(new { id = "", value = sTitleChoice });
            foreach (ListOfValueViewModel item in listProfices)
            {

                sName = _serviceLOV.ReplaceName_ListMain(item.Name).Trim();
                sShortName = _serviceLOV.ReplaceName_ListMain(item.ShortName).Trim();
                sCode = item.Code;
                if (string.IsNullOrEmpty(sCodeApply) || pCodeApply == "LANHDAO")
                {
                    if (pFlagShow == "1") //Hiển thị Tên danh mục
                    {
                        if (pTenVT == "1")
                            data.Add(new { id = sCode, value = sShortName });
                        else
                            data.Add(new { id = sCode, value = sName });
                    }

                    else
                    {
                        if (pTenVT == "1")
                            data.Add(new { id = sCode, value = $"{sCode} - {sShortName}" });
                        else
                            data.Add(new { id = sCode, value = $"{sCode} - {sName}" });
                    }
                }
                else
                {
                    if (item.CodeOfLovUsed.Contains(sCodeApply))
                    {
                        if (pFlagShow == "1") //Hiển thị Tên danh mục
                        {
                            if (pTenVT == "1")
                                data.Add(new { id = sCode, value = sShortName });
                            else
                                data.Add(new { id = sCode, value = sName });
                        }
                        else
                        {
                            if (pTenVT == "1")
                                data.Add(new { id = sCode, value = $"{sCode} - {sShortName}" });
                            else
                                data.Add(new { id = sCode, value = $"{sCode} - {sName}" });
                        }
                    }
                }
            }
            return Json(data);
        }


        /// <summary>
        /// Hàm lấy danh sách Chi nhánh (POS/MainPOS) hiển thị lên Combobox với Id là Mã POS
        /// </summary>
        /// <param name="pFlagCondi">Điều kiện lấy dữ liệu chính Chi nhánh. Giá trị quy ước:
        ///          '1' - Lấy duy nhất POS Hội sở chính (000100)
        ///          '2' - Lấy danh sách các POS HSC và Chi nhánh Tỉnh/TP (Danh sách MainPOS ĐK MaSo = MaSoCN Hoặc ParentId IN (0,1)
        ///          '3' - Lấy danh sách các POS Chi nhánh/PGD, trừ POS Hội sở chính (000100)
        ///          '4' - Lấy danh sách các POS HSC/Chi nhánh: Cấp TQ lấy tất cả; Cấp Chi nhánh/PGD Chỉ lấy POS của chi nhánh;
        ///          '5' - Lấy danh sách các POS HSC/Chi nhánh/PGD: Cấp TQ lấy tất cả; Cấp Chi nhánh/PGD Chỉ lấy POS của chi nhánh; PGD lấy duy nhất POS PGD
        ///          '6' - Lấy danh sách các POS HSC/Chi nhánh/PGD: Cấp TQ lấy 1 bản ghi Toàn hàng; Cấp Chi nhánh/PGD Chỉ lấy POS của chi nhánh; PGD lấy duy nhất POS PGD
        /// </param>
        /// <param name="pStatus">Trạng thái bản ghi. Nếu lấy tất cả truyền vào là '0'</param>
        /// <param name="pShortName">Chỉ số xác định: 1 - Lấy tên viết tắt hiển thị Combobox; 0 - Lấy tên đầy đủ</param>
        /// <param name="pTitleChoice">Tiêu đề dòng đầu chọn danh sách</param>
        /// <param name="pFlagTextShow">'1' - Hiển thị trên combobox là Tên;'0' - Hiển thị trên combobox là Mã - Tên;</param>
        /// <param name="pUserPosCode">Poscode của người dùng thực hiện chương trình. Nếu là cấp Chi nhánh/PGD thì giới hạn</param>
        /// <param name="pFlagAllBank">1: Toàn hàng; 0: Không có bản ghi này</param>
        /// <returns></returns>
        public JsonResult GetListBranchs(string pFlagCondi, string pStatus = "O", string pShortName = "1", string pTitleChoice = "",
                                         string pFlagTextShow = "1", string pUserPosCode = "", string pFlagAllBank = "0")
        {
            string sTitleChoice = "", sName = "", sShortName = "", sPosCode = "", sMainCode = "";
            //sTitleChoice = (pTitleChoice == "" || pTitleChoice == null) ? "---Chọn đơn vị---" : pTitleChoice;
            sTitleChoice = string.IsNullOrEmpty(pTitleChoice) ? "" : pTitleChoice;
            ArrayList data = new ArrayList();
            if (!string.IsNullOrEmpty(pUserPosCode) && pUserPosCode != "000100" && pUserPosCode != "000199")
            {
                string sSQL = $"Select Top 1 As Id, IsNull(MainPosCode,'') Code , IsNull(MainPosCode,'') Value From ListOfPos Where Code = {pUserPosCode}";
                string sMainPosTMP = _serviceLOV.GetCellValueForQuery(sSQL);
                sPosCode = (sMainPosTMP == pUserPosCode) ? "" : pUserPosCode;
                sMainCode = sMainPosTMP;
            }

            var listBranchs = _serviceLOV.GetBranchSearch(pFlagCondi, 0, sMainCode, sPosCode, pStatus, UserPosCode, UserName);

            if (sTitleChoice != "")
                data.Add(new { id = "", value = sTitleChoice });
            if (pFlagAllBank == "1" && (UserPosCode == "000100" || UserPosCode == "000199" || UserPosCode == "000196" || UserGrade == PosGrade.HEAD_POS))
                data.Add(new { id = "0", value = (pFlagTextShow == "1") ? "Toàn hàng" : "0 - Toàn hàng" });
            if (pFlagCondi == "6" && UserGrade == PosGrade.HEAD_POS)
            {
                if (data == null || data.Count <= 0)
                {
                    data.Add(new { id = "0", value = (pFlagTextShow == "1") ? "Toàn hàng" : "0 - Toàn hàng" });
                }
            }
            else
            {
                foreach (ListOfPosViewModel item in listBranchs)
                {
                    if (pFlagCondi == "2")
                    {
                        sName = item.MainPosName.Trim();
                        sShortName = item.MainPosName.Trim();
                    }
                    else if (item.MainPosCode == item.Code)
                    {
                        sName = item.Name.Trim();
                        sShortName = item.ShortName.Trim();
                    }
                    else
                    {
                        sName = $" - {item.Name.Trim()}";
                        sShortName = (pFlagTextShow == "1") ? $" - {item.ShortName.Trim()}" : $"{item.ShortName.Trim()}";
                    }
                    if (pFlagTextShow == "1") //Hiển thị Tên chi nhánh
                    {
                        if (pShortName == "1")
                            data.Add(new { id = item.Code, value = sShortName });
                        else
                            data.Add(new { id = item.Code, value = sName });
                    }
                    else //Hiển thị Mã + Tên chi nhánh
                    {
                        if (pShortName == "1")
                            data.Add(new { id = item.Code, value = $"{item.Code} - {sShortName}" });
                        else
                            data.Add(new { id = item.Code, value = $"{item.Code} - {sName}" });
                    }
                }
            }
            return Json(data);
        }


        public JsonResult GetListBranchForTide(string pFlagCondi, string pStatus = "O", string pShortName = "1", string pTitleChoice = "",
                                         string pFlagTextShow = "1", string pUserPosCode = "", string pFlagAllBank = "0")
        {
            if (UserGrade == PosGrade.HEAD_POS)
            {
                ArrayList data = new ArrayList();
                data.Add(new { id = "0", value = $"0 - Toàn hàng" });
                return Json(data);
            } else
            {
               return GetListBranchs( pFlagCondi,  pStatus,  pShortName,  pTitleChoice,
                                         pFlagTextShow,  pUserPosCode,  pFlagAllBank);  
            }                
        }

        public JsonResult GetListBranchForCasa(string pFlagCondi, string pStatus = "O", string pShortName = "1", string pTitleChoice = "",
                                        string pFlagTextShow = "1", string pUserPosCode = "", string pFlagAllBank = "0")
        {
            if (UserGrade == PosGrade.HEAD_POS)
            {
                ArrayList data = new ArrayList();
                data.Add(new { id = "0", value = $"0 - Toàn hàng" });
                return Json(data);
            }
            else
            {
                return GetListBranchs(pFlagCondi, pStatus, pShortName, pTitleChoice,
                                          pFlagTextShow, pUserPosCode, pFlagAllBank);
            }
        }

        public JsonResult GetListBranchForRateConfig(string pFlagCondi, string pStatus = "O", string pShortName = "1", string pTitleChoice = "",
                                      string pFlagTextShow = "1", string pUserPosCode = "", string pFlagAllBank = "0")
        {
            if (UserGrade == PosGrade.HEAD_POS)
            {
                ArrayList data = new ArrayList();
                data.Add(new { id = "0", value = $"0 - Toàn hàng" });
                return Json(data);
            }
            else
            {
                return GetListBranchs(pFlagCondi, pStatus, pShortName, pTitleChoice,
                                          pFlagTextShow, pUserPosCode, pFlagAllBank);
            }
        }

        /// <summary>
        /// Hàm lấy danh sách lên Combobox hoặc ListBox cho lựa chọn danh sách Sản phẩm/Loại tài khoản/Loại tài khoản phụ
        /// </summary>
        /// <param name="pFlagCallList">Cờ: 1 - Lấy danh sách Sản phẩm; 2 - Danh sách loại tài khoản; 3 - Loại tài khoản phụ</param>
        /// <param name="pProductGroupCode">Phân nhóm sản phẩm TIDE/CASA</param>
        /// <param name="pProductCode">Mã sản phẩm => Nếu pFlagCallList=2 thì truyền vào sản phẩm để lấy loại tài khoản </param>
        /// <param name="pAccountTypeCode">Loại tài khoản => Nếu pFlagCallList=3 thì truyền vào sản phẩm để lấy loại tài khoản phụ</param>
        /// <param name="pTitleChoice">Tiêu đề dòng đầu chọn danh sách</param>
        /// <param name="pFlagTextShow">'1' - Hiển thị trên combobox là Tên;'0' - Hiển thị trên combobox là Mã - Tên;</param>
        /// <param name="pControlFlag">'1' - Dùng danh sách cho Control List hoặc ComboBox; '2' - Dùng cho MultiSelect</param>
        /// <param name="pProductGroupCodeParams">Loại sản phẩm để ánh xạ vào bảng DL tham số cấu hình ProductParameters: CASA/TIDE/DEPOSITPENAL</param>
        /// <returns></returns>
        public JsonResult GetListOfProducts(int pFlagCallList, string pProductGroupCode, string pProductCode, string pAccountTypeCode, 
                                        string pTitleChoice = "", string pFlagTextShow = "1", string pControlFlag = "1", string pProductGroupCodeParams = "")
        {
            string sTitleChoice = "", sNameShow = "";
            sTitleChoice = string.IsNullOrEmpty(pTitleChoice) ? "" : pTitleChoice;
            ArrayList dataListOfProducts = new ArrayList();

            var listListOfProductsTmp = _serviceLOV.GetListOfProductsSearch(pProductGroupCode, pProductCode, pAccountTypeCode, "", "", 1, 1, UserGrade, pProductGroupCodeParams);

            if (sTitleChoice != "")
            {
                if (pControlFlag == "1")
                    dataListOfProducts.Add(new { id = "", value = sTitleChoice });
                else dataListOfProducts.Add(new { Value = "", Text = sTitleChoice });
            }
            List<ListOfProducts> listListOfProducts = new List<ListOfProducts>();
            if (pFlagCallList == 1)
            {
                listListOfProducts = listListOfProductsTmp.Select(p => new ListOfProducts
                {
                    ProductGroupCode = p.ProductGroupCode,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Code = p.ProductCode,
                    Name = p.ProductName,
                    CurrencyCode = p.CurrencyCode
                }).Distinct().ToList();
            }
            else if (pFlagCallList == 2)
            {
                listListOfProducts = listListOfProductsTmp.Select(p => new ListOfProducts
                {
                    ProductGroupCode = p.ProductGroupCode,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    AccountTypeCode = p.AccountTypeCode,
                    AccountTypeName = p.AccountTypeName,
                    Code = p.AccountTypeCode,
                    Name = p.AccountTypeName,
                    CurrencyCode = p.CurrencyCode
                }).Distinct().ToList();
            }
            else if (pFlagCallList == 3)
            {
                listListOfProducts = listListOfProductsTmp.Select(p => new ListOfProducts
                {
                    ProductGroupCode = p.ProductGroupCode,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    AccountTypeCode = p.AccountTypeCode,
                    AccountTypeName = p.AccountTypeName,
                    AccountSubTypeCode = p.AccountSubTypeCode,
                    AccountSubTypeName = p.AccountSubTypeName,
                    Code = p.AccountSubTypeCode,
                    Name = p.AccountSubTypeName,
                    CurrencyCode = p.CurrencyCode
                }).Distinct().ToList();
            }

            if (listListOfProducts != null && listListOfProducts.Count != 0)
            {
                foreach (ListOfProducts item in listListOfProducts)
                {
                    if (pFlagTextShow == "1") //Hiển thị Tên
                    {
                        sNameShow = item.Name;
                    }
                    if (pFlagTextShow == "0") //Hiển thị Tên
                    {
                        sNameShow = $"{item.Code} - {item.Name}";
                    }

                    if (pControlFlag == "1")
                        dataListOfProducts.Add(new { id = item.Code, value = sNameShow });
                    else dataListOfProducts.Add(new { Value = item.Code, Text = sNameShow });
                }
            }
            return Json(dataListOfProducts);
        }

        /// <summary>
        /// Hàm lấy danh sách cán bộ NHCSXH lên Combobox hoặc ListBox theo POS truyền vào
        /// </summary>
        /// <param name="pPosCode">Mã POS</param>
        /// <param name="pTitleChoice">Tiêu đề dòng đầu chọn danh sách</param>
        /// <param name="pFlagTextShow">Quy ước hiển thị trên Combobox hoặc ListBox
        ///                             '0': Mã - Họ tên;
        ///                             '1': Họ tên
        ///                             '2': Mã - Họ tên - Chức vụ - Phòng ban - Đơn vị - Chi nhánh
        ///                             '3': Mã - Họ tên - Chức vụ - Phòng ban
        ///                             '4': Mã - Họ tên - Chức vụ - Tên tập thể
        ///                             '5': Mã - Tên - Chức vụ
        /// </param>
        /// <param name="pFlagCall">Cờ xác định cách gọi. Giá trị: 
        ///                     '0' hoặc Rỗng: Mặc định;
        ///                     '1': Lấy danh sách có loại trừ những người đã tạo tài khoản trong bảng Users
        ///                     '2': Lấy danh sách có loại trừ những người đã tạo tài khoản trong bảng UserIDCMaster
        /// </param>
        /// <returns>Danh sách cán bộ NHCSXH</returns>
        public async Task<JsonResult> GetListStaffVBSP(string pPosCode, string pTitleChoice = "", string pFlagTextShow = "1", string pFlagCall = "0")
        {
            string sTitleChoice = "", sNameShow = "";
            sTitleChoice = string.IsNullOrEmpty(pTitleChoice) ? "" : pTitleChoice;
            ArrayList dataListOfStaffVBSP = new ArrayList();

            var listStaffVBSP = await _internalServiceAPI.GetListStaffVBSP(pPosCode);
            int iCountTemp = 0;
            if (sTitleChoice != "")
                dataListOfStaffVBSP.Add(new { id = "", value = sTitleChoice });
           
            if (listStaffVBSP != null && listStaffVBSP.Success && listStaffVBSP.Result != null && listStaffVBSP.Result.Count > 0)
            {
                List<StaffVbspInforViewModel> listStaffVBSPTemp = new List<StaffVbspInforViewModel>();
                if (pFlagCall == "1")
                {
                    DateTime fromDate = DateTime.Now.AddYears(-60);
                    DateTime toDate = DateTime.Now;
                    List<string> listStaffIdExist = new List<string>();
                    var listStaffIdExistTmp = _administrationService.GetUsers("", "", "", fromDate, toDate, "", "", "", "", "");
                    if (listStaffIdExistTmp != null && listStaffIdExistTmp.Count != 0)
                        listStaffIdExist = listStaffIdExistTmp.Where(w => w.Status != StatusLov.StatusClosed).Select(s => s.StaffId).ToList();
                    listStaffVBSPTemp = listStaffVBSP.Result.Where(w => w.StaffId != "" && !listStaffIdExist.Contains(w.StaffId)).ToList();
                }
                else listStaffVBSPTemp = listStaffVBSP.Result;
                if (listStaffVBSPTemp != null && listStaffVBSPTemp.Count != 0)
                {
                    foreach (StaffVbspInforViewModel item in listStaffVBSP.Result)
                    {
                        iCountTemp++;
                        if (pFlagTextShow == "1") //Hiển thị Tên
                            sNameShow = item.StaffName;
                        else if (pFlagTextShow == "0") //Hiển thị Mã - Tên
                            sNameShow = $"{item.StaffCode} - {item.StaffName}";
                        else if (pFlagTextShow == "2") //Hiển thị Mã - Tên - Chức vụ - Phòng ban - Đơn vị - Chi nhánh
                        {
                            if (item.MainPosCode == item.PosCode)
                            {
                                if (item.PosCode == "000100" || item.PosCode == "000199")
                                    sNameShow = $"{iCountTemp.ToString()}. {item.StaffCode} - {item.StaffName} - {item.StaffPositionName} - {item.StaffDepartmentName}";
                                else if (item.PosCode == "000100" || item.PosCode == "000101" || item.PosCode == "000196" || item.PosCode == "000197" || item.PosCode == "000199")
                                    sNameShow = $"{iCountTemp.ToString()}. {item.StaffCode} - {item.StaffName} - {item.StaffPositionName} - {item.StaffDepartmentName} - {item.StaffPosName}";
                                else sNameShow = $"{iCountTemp.ToString()}. {item.StaffCode} - {item.StaffName} - {item.StaffPositionName} - {item.StaffDepartmentName} - {item.StaffPosName}";
                            }
                            else sNameShow = $"{iCountTemp.ToString()}. {item.StaffCode} - {item.StaffName} - {item.StaffPositionName} - {item.StaffDepartmentName} - {item.StaffPosName} - {item.MainPosName}";
                        }
                        else if (pFlagTextShow == "3") //Hiển thị Mã - Tên - Chức vụ - Phòng ban
                            sNameShow = $"{iCountTemp.ToString()}. {item.StaffCode} - {item.StaffName} - {item.StaffPositionName} - {item.StaffDepartmentName}";
                        else if (pFlagTextShow == "4") //Hiển thị Mã - Tên - Chức vụ - Tên tập thể
                            sNameShow = $"{iCountTemp.ToString()}. {item.StaffCode} - {item.StaffName} - {item.StaffPositionName} - {item.DepartmentUnitName}";
                        else if (pFlagTextShow == "5") //Hiển thị Mã - Tên - Chức vụ
                            sNameShow = $"{iCountTemp.ToString()}. {item.StaffCode} - {item.StaffName} - {item.StaffPositionName}";
                        dataListOfStaffVBSP.Add(new { id = item.StaffId, value = sNameShow });
                    }
                }
            }
            return Json(dataListOfStaffVBSP);
        }

        /// <summary>
        /// Hàm lấy thông tin chi tiết theo Id cán bộ truyền vào
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pStaffId">Id Cán bộ cần lấy thông tin chi tiết</param>
        /// <param name="pFlagCall">Cờ xác định cách gọi. Giá trị: 
        ///                     '0' hoặc Rỗng: Mặc định;
        ///                     '1': Lấy danh sách có loại trừ những người đã tạo tài khoản trong bảng Users
        ///                     '2': Lấy danh sách có loại trừ những người đã tạo tài khoản trong bảng UserIDCMaster
        /// </param>
        /// <returns>Thông tin chi tiết của cán bộ</returns>
        public async Task<JsonResult> GetListStaffVBSPByStaffId([DataSourceRequest] DataSourceRequest request, string pStaffId, string pFlagCall = "0")
        {
            if (string.IsNullOrEmpty(pStaffId))
                pStaffId = "";
            ArrayList dataListOfStaffVBSP = new ArrayList();
            var listStaffVBSP = await _internalServiceAPI.GetListStaffByStaffId(pStaffId);
            if (listStaffVBSP != null && listStaffVBSP.Success && listStaffVBSP.Result != null && listStaffVBSP.Result.Count > 0)
            {
                List<StaffVbspInforViewModel> listStaffVBSPTemp = new List<StaffVbspInforViewModel>();
                pFlagCall = "1";
                if (pFlagCall == "1")
                {
                    DateTime fromDate = DateTime.Now.AddYears(-60);
                    DateTime toDate = DateTime.Now;
                    List<string> listStaffIdExist = new List<string>();
                    var listStaffIdExistTmp = _administrationService.GetUsers("", "", "", fromDate, toDate, "", "", "", "", "");
                    if (listStaffIdExistTmp != null && listStaffIdExistTmp.Count != 0)
                        listStaffIdExist = listStaffIdExistTmp.Where(w => w.Status != StatusLov.StatusClosed).Select(s => s.StaffId).ToList();
                    listStaffVBSPTemp = listStaffVBSP.Result.Where(w => w.StaffId != "" && !listStaffIdExist.Contains(w.StaffId)).ToList();
                }
                else listStaffVBSPTemp = listStaffVBSP.Result;

                if (listStaffVBSPTemp != null && listStaffVBSPTemp.Count != 0)
                {
                    foreach (StaffVbspInforViewModel item in listStaffVBSPTemp)
                    {
                        dataListOfStaffVBSP.Add(new
                        {
                            Id = item.Id,
                            MainPosCode = item.MainPosCode,
                            MainPosName = item.MainPosName,
                            PosCode = item.PosCode,
                            PosName = item.PosName,
                            StaffId = item.StaffId,
                            StaffCode = item.StaffCode,
                            StaffName = item.StaffName,
                            DateOfBirth = item.DateOfBirth,
                            DateOfBirthText = item.DateOfBirth.ToString("dd/MM/yyyy"),
                            GenderCode = item.GenderCode,
                            GenderText = item.GenderText,
                            StaffPosCode = item.StaffPosCode,
                            StaffPosName = item.StaffPosName,
                            StaffDepartmentCode = item.StaffDepartmentCode,
                            StaffDepartmentName = item.StaffDepartmentName,
                            StaffPositionCode = item.StaffPositionCode,
                            StaffPositionName = item.StaffPositionName,
                            StaffMobileNo = item.StaffMobileNo,
                            StaffEmail = item.StaffEmail,
                            AddressDetail = item.AddressDetail,
                            IdNo = item.IdNo,
                            IssuedDate = item.IssuedDate,
                            IssuedPlace = item.IssuedPlace,
                            DegreeCode = item.DegreeCode,
                            StaffStatus = item.StaffStatus,
                            StaffStatusText = item.StaffStatusText,
                            DepartmentUnitCode = item.DepartmentUnitCode,
                            DepartmentUnitName = item.DepartmentUnitName,
                            RetirementDate = item.RetirementDate,
                            Notes = item.Notes,
                            CreatedBy = item.CreatedBy,
                            CreatedDate = item.CreatedDate,
                            ModifiedBy = item.ModifiedBy,
                            ModifiedDate = item.ModifiedDate,
                        });
                    }
                } 
                
            }
            return Json(dataListOfStaffVBSP);
        }


        public JsonResult GetProductList(string productGroupCode)
        {
            ArrayList data = new ArrayList();
            var _products = _productService.GetProductList(productGroupCode);
            for(int i = 0; i < _products.Count; i++)
            {
                data.Add(new { id = _products[i].ProductCode, value = $"{_products[i].ProductCode} - {_products[i].ProductName}"  });
            }
            return Json(data);
        }

        public JsonResult GetDepositTypeList()
        {
            ArrayList data = new ArrayList();
            
            data.Add(new { id = "B", value = "Đầu kỳ" });
            data.Add(new { id = "P", value = "Định kỳ" });
            data.Add(new { id = "E", value = "Cuối kỳ" });

            return Json(data);
        }

        public JsonResult GetTermTypeList()
        {
            ArrayList data = new ArrayList();

            data.Add(new { id = "M", value = "Tháng" });
            //data.Add(new { id = "Q", value = "Quý" });
            //data.Add(new { id = "Y", value = "Năm" });

            return Json(data);
        }

        public JsonResult GetMoneyLevelSerialList()
        {
            ArrayList data = new ArrayList();
            data.Add(new { id = 1, value = "1 - Upto 100000000000000" });            
            return Json(data);
        }


        public JsonResult GetInclusionFlagList()
        {
            ArrayList data = new ArrayList();

            data.Add(new { id = "INCLUSIVE", value = "INCLUSIVE (Bao gồm)" });
            data.Add(new { id = "EXCLUSIVE", value = "EXCLUSIVE (Không bao gồm)" });

            return Json(data);
        }

        public JsonResult GetCustomerTypeList()
        {
            ArrayList data = new ArrayList();

            data.Add(new { id = "A", value = "Tất cả" });
            data.Add(new { id = "I", value = "Cá nhân" });
            data.Add(new { id = "O", value = "Tổ chức" });

            return Json(data);
        }


        public JsonResult GetAccoutTypeList(string productCode)
        {
            ArrayList data = new ArrayList();
            var _products = _productService.GetAccountTypes(productCode);
            for (int i = 0; i < _products.Count; i++)
            {
                data.Add(new { id = _products[i].Value, value = $"{_products[i].Value} - {_products[i].Text}" });
            }
            return Json(data);
        }

        public JsonResult GetSubAccoutTypeList(string accountType)
        {
            ArrayList data = new ArrayList();
            var _products = _productService.GetAccountSubTypes(accountType);
            for (int i = 0; i < _products.Count; i++)
            {
                data.Add(new { id = _products[i].Value, value = $"{_products[i].Value} - {_products[i].Text}" });
            }
            return Json(data);
        }

        /// <summary>
        ///  Hàm lấy danh sách Tỉnh/Thành phố hiển thị lên Combobox với Id là Mã Tỉnh/TP
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh/thành phố (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái. Nếu là 0 lấy tất</param>
        /// <param name="pTitleChoice">Tiêu đề lựa chọn danh sách</param>
        /// <param name="pFlagTextShow">Trang thái hiển thị trên Combobox. Giá trị:
        ///                      1 - Hiển thị duy nhất Tên trên danh sách ComBoBox
        ///                      2 - Hiển thị [Mã - Tên] trên danh sách ComBoBox
        ///                      3 - Hiển thị [Vùng -> Mã - Tên] trên danh sách ComBoBox
        /// </param>
        /// <returns>Danh sách Tỉnh/Thành phố trên Combobox</returns>
        public JsonResult GetListProvinces(string pProvinceCode = "", string pStatus = "0", string pTitleChoice = "", string pFlagTextShow = "1")
        {
            string sTitleChoice = "";
            sTitleChoice = (pTitleChoice == "" || pTitleChoice == null) ? "---Chọn Tỉnh/Thành phố---" : pTitleChoice;
            ArrayList data = new ArrayList();
            var listProvinces = _serviceLOV.GetLovCommuneList(pProvinceCode, "", "", "", "");
            if (sTitleChoice != "")
                data.Add(new { id = "", value = sTitleChoice });
            foreach (ListOfCommuneViewModel item in listProvinces)
            {
                if ((pStatus == "1" && item.Status == Constants.StatusLov.StatusOpenPOS) || (pStatus == "0"))
                {
                    if (pFlagTextShow == "1") //Hiển thị duy nhất Tên trên danh sách ComBoBox
                        data.Add(new { id = item.ProvinceCode, value = item.ProvinceName.Trim() });
                    else if (pFlagTextShow == "2") //Hiển thị [Mã - Tên] trên danh sách ComBoBox
                        data.Add(new { id = item.ProvinceCode, value = $"{item.ProvinceCode} - {item.ProvinceName}" });
                    else if (pFlagTextShow == "3") //Hiển thị [Vùng -> Mã - Tên] trên danh sách ComBoBox
                        data.Add(new { id = item.ProvinceCode, value = $"{item.Region_01} => {item.ProvinceCode} - {item.ProvinceName}" });
                }
            }
            return Json(data);
        }

        /// <summary>
        /// Hàm lấy danh sách Quận/Huyện/Thị xã hiển thị lên Combobox với Id là Mã Quận/Huyện/Thị xã
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh/thành phố (Không bắt buộc)</param>
        /// <param name="pDistrictCode">Mã quận/huyện/thị xã (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái. Nếu là 0 lấy tất</param>
        /// <param name="pTitleChoice">Tiêu đề lựa chọn danh sách</param>
        /// <param name="pFlagTextShow">Trang thái hiển thị trên Combobox. Giá trị:
        ///                       1 - Hiển thị duy nhất Tên trên danh sách ComBoBox
        ///                       2 - Hiển thị [Mã - Tên] trên danh sách ComBoBox
        ///                       3 - Hiển thị [Mã => Tên Tỉnh - tên Huyện] trên danh sách ComBoBox
        ///                       4 - Hiển thị [Tên Tỉnh - tên Huyện] trên danh sách ComBoBox
        /// </param>
        /// <returns>Danh sách Quận/Huyện/Thị xã trên Combobox</returns>
        public JsonResult GetListDistricts(string pProvinceCode = "", string pDistrictCode = "", string pStatus = "0", string pTitleChoice = "", string pFlagTextShow = "1")
        {
            string sTitleChoice = "";
            sTitleChoice = (pTitleChoice == "" || pTitleChoice == null) ? "---Chọn Quận/Huyện/Thị xã---" : pTitleChoice;
            ArrayList data = new ArrayList();
            var listDistricts = _serviceLOV.GetLovCommuneList(pProvinceCode, pDistrictCode, "", "", "");
            if (sTitleChoice != "" && string.IsNullOrEmpty(pDistrictCode))
                data.Add(new { id = "", value = sTitleChoice });
            foreach (ListOfCommuneViewModel item in listDistricts)
            {
                if ((pStatus == "1" && item.Status == Constants.StatusLov.StatusOpenPOS) || (pStatus == "0"))
                {
                    if (pFlagTextShow == "1") //Hiển thị duy nhất Tên trên danh sách ComBoBox
                        data.Add(new { id = item.DistrictCode, value = item.DistrictName.Trim() });
                    else if (pFlagTextShow == "2") //Hiển thị [Mã - Tên] trên danh sách ComBoBox
                        data.Add(new { id = item.DistrictCode, value = $"{item.DistrictCode} - {item.DistrictName}" });
                    else if (pFlagTextShow == "3") //Hiển thị [Mã => Tên Tỉnh - tên Huyện] trên danh sách ComBoBox
                        data.Add(new { id = item.DistrictCode, value = $"{item.DistrictCode} => {item.ProvinceName} - {item.DistrictName}" });
                    else if (pFlagTextShow == "4") //Hiển thị [Tên Tỉnh - tên Huyện] trên danh sách ComBoBox
                        data.Add(new { id = item.DistrictCode, value = $"{item.ProvinceName} - {item.DistrictName}" });
                }
            }
            return Json(data);
        }
        //return Json(data, new Newtonsoft.Json.JsonSerializerSettings());



        /// <summary>
        /// Hàm lấy danh sách Xã/Phường/Thị trấn hiển thị lên Combobox với Id là Mã Xã/Phường/Thị trấn
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh/thành phố (Không bắt buộc)</param>
        /// <param name="pDistrictCode">Mã quận/huyện/thị xã (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã/phường/thị trấn (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã POS (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái. Nếu là 0 lấy tất</param>
        /// <param name="pTitleChoice">Tiêu đề lựa chọn danh sách</param>
        /// <param name="pFlagTextShow">Trang thái hiển thị trên Combobox. Giá trị:
        ///                       1 - Hiển thị duy nhất Tên trên danh sách ComBoBox
        ///                       2 - Hiển thị [Mã - Tên] trên danh sách ComBoBox
        ///                       3 - Hiển thị [Mã => Tên huyện - Tên Xã] trên danh sách ComBoBox
        ///                       4 - Hiển thị [Mã => Tên tỉnh - Tên huyện - Tên Xã] trên danh sách ComBoBox
        ///                       5 - Hiển thị [Tên huyện - Tên Xã] trên danh sách ComBoBox
        ///                       6 - Hiển thị [Tên tỉnh - Tên huyện - tên xã] trên danh sách ComBoBox
        /// </param>
        /// <returns>Danh sách  Xã/Phường/Thị trấn trên Combobox</returns>
        public JsonResult GetListCommunes(string pProvinceCode = "", string pDistrictCode = "", string pCommuneCode = "", string pPosCode = "", string pStatus = "0", string pTitleChoice = "", string pFlagTextShow = "1")
        {
            string sTitleChoice = "";
            sTitleChoice = (pTitleChoice == "" || pTitleChoice == null) ? "---Chọn Xã/Phường/Thị trấn---" : pTitleChoice;
            ArrayList data = new ArrayList();
            var listCommunes = _serviceLOV.GetLovCommuneList(pProvinceCode, pDistrictCode, pCommuneCode, pPosCode, "");
            if (sTitleChoice != "" && string.IsNullOrEmpty(pCommuneCode))
                data.Add(new { id = "", value = sTitleChoice });
            foreach (ListOfCommuneViewModel item in listCommunes)
            {
                if ((pStatus == "1" && item.Status == Constants.StatusLov.StatusOpenPOS) || (pStatus == "0"))
                {
                    if (pFlagTextShow == "1") //Hiển thị duy nhất Tên trên danh sách ComBoBox
                        data.Add(new { id = item.CommuneCode, value = item.CommuneName.Trim() });
                    else if (pFlagTextShow == "2") //Hiển thị [Mã - Tên] trên danh sách ComBoBox
                        data.Add(new { id = item.CommuneCode, value = $"{item.CommuneCode} - {item.CommuneName}" });
                    else if (pFlagTextShow == "3") //Hiển thị [Mã => Tên huyện - Tên Xã] trên danh sách ComBoBox
                        data.Add(new { id = item.CommuneCode, value = $"{item.CommuneCode} => {item.DistrictName} - {item.CommuneName}" });
                    else if (pFlagTextShow == "4") //Hiển thị [Mã => Tên tỉnh - Tên huyện - Tên Xã] trên danh sách ComBoBox
                        data.Add(new { id = item.CommuneCode, value = $"{item.CommuneCode} => {item.ProvinceName} - {item.DistrictName} - {item.CommuneName}" });
                    else if (pFlagTextShow == "5") //Hiển thị [Tên huyện - Tên Xã] trên danh sách ComBoBox
                        data.Add(new { id = item.CommuneCode, value = $"{item.DistrictName} - {item.CommuneName}" });
                    else if (pFlagTextShow == "6") //Hiển thị [Tên tỉnh - Tên huyện - tên xã] trên danh sách ComBoBox
                        data.Add(new { id = item.CommuneCode, value = $"{item.ProvinceName} - {item.DistrictName} - {item.CommuneName}" });
                }
            }
            return Json(data);
        }

        /// <summary>
        /// Hàm lấy danh sách Thôn/Bản/Làng hiển thị lên Combobox với Id là Mã Thôn/Bản/Làng
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh/thành phố (Không bắt buộc)</param>
        /// <param name="pDistrictCode">Mã quận/huyện/thị xã (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã/phường/thị trấn (Không bắt buộc)</param>
        /// <param name="pSubcommuneCode">Mã thôn/bản/làng (Không bắt buộc)</param>
        /// <param name="pPosCode">Mã POS (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái. Nếu là 0 lấy tất</param>
        /// <param name="pTitleChoice">Tiêu đề lựa chọn danh sách</param>
        /// <param name="pFlagTextShow">Trang thái hiển thị trên Combobox. Giá trị:
        ///                       1 - Hiển thị duy nhất Tên trên danh sách ComBoBox
        ///                       2 - Hiển thị [Mã - Tên] trên danh sách ComBoBox
        ///                       3 - Hiển thị [Mã => Tên Xã - Tên thôn] trên danh sách ComBoBox
        ///                       4 - Hiển thị [Mã => Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
        ///                       5 - Hiển thị [Mã => Tên tỉnh - Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
        ///                       6 - Hiển thị [Tên Xã - Tên thôn] trên danh sách ComBoBox
        ///                       7 - Hiển thị [Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
        ///                       8 - Hiển thị [Tên tỉnh - Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
        /// </param>
        /// <returns>Danh sách Thôn/Bản/Làng trên Combobox</returns>
        public JsonResult GetListSubCommunes(string pProvinceCode = "", string pDistrictCode = "", string pCommuneCode = "", string pSubcommuneCode = "", string pPosCode = "", string pStatus = "0", string pTitleChoice = "", string pFlagTextShow = "1")
        {
            string sTitleChoice = "";
            sTitleChoice = (pTitleChoice == "" || pTitleChoice == null) ? "---Chọn Thôn/Bản/Làng---" : pTitleChoice;
            ArrayList data = new ArrayList();
            var listSubCommunes = _serviceLOV.GetLovCommuneList(pProvinceCode, pDistrictCode, pCommuneCode, pPosCode, pSubcommuneCode);
            if (sTitleChoice != "" && string.IsNullOrEmpty(pSubcommuneCode))
                data.Add(new { id = "", value = sTitleChoice });
            foreach (ListOfCommuneViewModel item in listSubCommunes)
            {
                if ((pStatus == "1" && item.Status == Constants.StatusLov.StatusOpenPOS) || (pStatus == "0"))
                {
                    if (pFlagTextShow == "1") //Hiển thị duy nhất Tên trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = item.SubCommuneName.Trim() });
                    else if (pFlagTextShow == "2") //Hiển thị [Mã - Tên] trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = $"{item.SubCommuneCode} - {item.SubCommuneName}" });
                    else if (pFlagTextShow == "3") //Hiển thị [Mã => Tên Xã - Tên thôn] trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = $"{item.SubCommuneCode} => {item.CommuneName} - {item.SubCommuneName}" });
                    else if (pFlagTextShow == "4") //Hiển thị [Mã => Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = $"{item.SubCommuneCode} => {item.DistrictName} - {item.CommuneName} - {item.SubCommuneName}" });
                    else if (pFlagTextShow == "5") //Hiển thị [Mã => Tên tỉnh - Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = $"{item.SubCommuneCode} => {item.ProvinceName} - {item.DistrictName} - {item.CommuneName} - {item.SubCommuneName}" });
                    else if (pFlagTextShow == "6") //Hiển thị [Tên Xã - Tên thôn] trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = $"{item.CommuneName} - {item.SubCommuneName}" });
                    else if (pFlagTextShow == "7") //Hiển thị [Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = $"{item.DistrictName} - {item.CommuneName} - {item.SubCommuneName}" });
                    else if (pFlagTextShow == "8") //Hiển thị [Tên tỉnh - Tên huyện - Tên Xã - Tên thôn] trên danh sách ComBoBox
                        data.Add(new { id = item.SubCommuneCode, value = $"{item.ProvinceName} - {item.DistrictName} - {item.CommuneName} - {item.SubCommuneName}" });
                }
            }
            return Json(data);
        }

        /// <summary>
        /// The LoadGridData_ListMain.
        /// </summary>
        /// <param name="request">The request<see cref="DataSourceRequest"/>.</param>
        /// <param name="pParentId_TKiem">The pParentId_TKiem<see cref="string"/>.</param>
        /// <param name="pTenGoi_TKiem">The pTenGoi_TKiem<see cref="string"/>.</param>
        /// <param name="pMaSo_TKiem">The pMaSo_TKiem<see cref="string"/>.</param>
        /// <returns>The <see cref="IActionResult"/>.</returns>
        public IActionResult LoadGridData_ListMain([DataSourceRequest] DataSourceRequest request, string pParentId_TKiem, string pTenGoi_TKiem, string pMaSo_TKiem)
        {
            int iParentId = 0;
            if (!string.IsNullOrEmpty(pParentId_TKiem))
            {
                //Lấy Id danh mục từ mã số danh mục
                var listObj = _serviceLOV.GetListOfValueByCode(pParentId_TKiem, DefaultValue.StatusClosed);
                if (listObj != null)
                    iParentId = listObj.Id;
                else iParentId = -1;
            }
            else iParentId = -1;

            string sTenGoi_TKiem = "", sMaSo_TKiem = "";
            sTenGoi_TKiem = (string.IsNullOrEmpty(pTenGoi_TKiem) || pTenGoi_TKiem == "") ? "" : pTenGoi_TKiem;
            sMaSo_TKiem = (string.IsNullOrEmpty(pMaSo_TKiem) || pMaSo_TKiem == "") ? "" : pMaSo_TKiem;

            var profileLists = _serviceLOV.GetListOfValueSearch(iParentId, "", 0, sMaSo_TKiem, sTenGoi_TKiem, DefaultValue.StatusOpen, 0);
            return Json(profileLists.ToDataSourceResult(request));
        }

        /// <summary>
        /// The ShowUpdateList.
        /// </summary>
        /// <param name="pListId">The pListId<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ShowUpdateList(string pListId)
        {
            ListOfValueViewModel listViewModel = new ListOfValueViewModel();
            int iRowId = 0;
            iRowId = (string.IsNullOrEmpty(pListId) || pListId == "") ? 0 : Convert.ToInt32(pListId);
            if (iRowId > 0)  //Sửa đổi
            {
                ListOfValueViewModel profileList = _serviceLOV.GetListOfValueForId(iRowId, Constants.DefaultValue.StatusOpen);
                if (profileList != null)
                {
                    listViewModel.Id = profileList.Id;
                    listViewModel.OrderNoAll = profileList.OrderNoAll;
                    listViewModel.OrderNo = profileList.OrderNo;
                    listViewModel.OrderNoText = (string.IsNullOrEmpty(profileList.OrderNoText)) ? "" : profileList.OrderNoText;
                    listViewModel.ParentId = profileList.ParentId;
                    listViewModel.ParentId_01 = profileList.ParentId_01;
                    listViewModel.ParentId_02 = profileList.ParentId_02;
                    listViewModel.ParentId_03 = profileList.ParentId_03;
                    listViewModel.ParentId_04 = profileList.ParentId_04;
                    listViewModel.ParentId_05 = profileList.ParentId_05;
                    listViewModel.ParentCode = (string.IsNullOrEmpty(profileList.ParentCode)) ? "" : profileList.ParentCode;
                    listViewModel.ParentText = (string.IsNullOrEmpty(profileList.ParentText)) ? "" : profileList.ParentText;
                    listViewModel.Code = profileList.Code;
                    listViewModel.Name = profileList.Name;
                    listViewModel.ShortName = profileList.ShortName;
                    listViewModel.Status = profileList.Status;
                    listViewModel.StatusText = profileList.StatusText;
                    listViewModel.CodeOfLovUsed = profileList.CodeOfLovUsed;
                    listViewModel.CodeOfLovUsedText = profileList.CodeOfLovUsedText;
                    listViewModel.Notes = profileList.Notes;
                    listViewModel.LevelCode = profileList.LevelCode;
                    listViewModel.SumLevelFlag = profileList.SumLevelFlag;
                    listViewModel.PrintType = profileList.PrintType;
                    listViewModel.PrintTypeText = profileList.PrintTypeText;
                    listViewModel.EditableFlag = profileList.EditableFlag;
                    listViewModel.CategoryLevel = profileList.CategoryLevel;
                    listViewModel.CreatedBy = profileList.CreatedBy;
                    listViewModel.CreatedDate = profileList.CreatedDate;
                    listViewModel.ModifiedBy = profileList.ModifiedBy;
                    listViewModel.ModifiedDate = profileList.ModifiedDate;
                    listViewModel.CodeOfLovUsedList = profileList.CodeOfLovUsedList;
                    listViewModel.StrTmp01 = profileList.StrTmp01;
                    listViewModel.StrTmp02 = profileList.StrTmp02;

                    var maApDungList = new List<String>();
                    if (!string.IsNullOrEmpty(listViewModel.CodeOfLovUsed))
                    {
                        string[] arrItem = Utilities.Splip_Strings(listViewModel.CodeOfLovUsed, ",");
                        if (arrItem.Length != 0)
                            maApDungList.AddRange(arrItem);
                    }
                    listViewModel.CodeOfLovUsedList = maApDungList;
                    string sSQL = "";
                    switch (listViewModel.CategoryLevel)
                    {
                        case 1:
                            listViewModel.ParentId = profileList.ParentId;
                            break;
                        case 2:
                            listViewModel.ParentId = profileList.ParentId;
                            break;
                        case 3:
                            listViewModel.ParentId = profileList.ParentId;
                            listViewModel.ParentId_01 = (int)profileList.ParentId;
                            sSQL = $"Select Cast(IsNull(ParentId,0) As Varchar(32)) Code From ListOfValue Where Id = {listViewModel.ParentId_01}";
                            listViewModel.ParentId = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));
                            break;
                        case 4:
                            listViewModel.ParentId_02 = (int)profileList.ParentId;
                            sSQL = $"Select Cast(IsNull(ParentId,0) As Varchar(32)) Code From ListOfValue Where Id = {listViewModel.ParentId_02}";
                            listViewModel.ParentId_01 = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));

                            sSQL = $"Select Cast(IsNull(ParentId,0) As Varchar(32)) Code From ListOfValue Where Id = {listViewModel.ParentId_01}";
                            listViewModel.ParentId = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));
                            break;
                        case 5:
                            listViewModel.ParentId_03 = (int)profileList.ParentId;

                            sSQL = $"Select Cast(IsNull(ParentId,0) As Varchar(32)) Code From ListOfValue Where Id = {listViewModel.ParentId_03}";
                            listViewModel.ParentId_02 = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));

                            sSQL = $"Select Cast(IsNull(ParentId,0) As Varchar(32)) Code From ListOfValue Where Id = {listViewModel.ParentId_02}";
                            listViewModel.ParentId_01 = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));

                            sSQL = $"Select Cast(IsNull(ParentId,0) As Varchar(32)) Code From ListOfValue Where Id = {listViewModel.ParentId_01}";
                            listViewModel.ParentId = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));
                            break;
                        default:
                            listViewModel.ParentId = 0;
                            listViewModel.ParentId_01 = 0;
                            listViewModel.ParentId_02 = 0;
                            listViewModel.ParentId_03 = 0;
                            listViewModel.ParentId_04 = 0;
                            listViewModel.ParentId_05 = 0;
                            break;
                    }

                    TempData["FlagCall"] = "2";
                }
            }
            else    //Trường hợp thêm mới
            {
                listViewModel.Id = 0;
                listViewModel.OrderNoAll = 0;
                listViewModel.OrderNo = 0;
                listViewModel.OrderNoText = "";
                listViewModel.ParentId = 0;
                listViewModel.ParentId_01 = 0;
                listViewModel.ParentId_02 = 0;
                listViewModel.ParentId_03 = 0;
                listViewModel.ParentId_04 = 0;
                listViewModel.ParentId_05 = 0;
                listViewModel.ParentCode = "";
                listViewModel.ParentText = "";
                listViewModel.Code = "";
                listViewModel.Name = "";
                listViewModel.ShortName = "";
                listViewModel.Status = Constants.DefaultValue.StatusOpen;
                listViewModel.StatusText = "";
                listViewModel.CodeOfLovUsed = "";
                listViewModel.CodeOfLovUsedText = "";
                listViewModel.Notes = "";
                listViewModel.LevelCode = "";
                listViewModel.SumLevelFlag = 0;
                listViewModel.PrintType = Constants.PrintTypeValue.NormalValue;
                listViewModel.PrintTypeText = "";
                listViewModel.EditableFlag = 0;
                listViewModel.CategoryLevel = 0;
                listViewModel.CreatedBy = UserName;
                listViewModel.CreatedDate = DateTime.Now;
                listViewModel.ModifiedBy = UserName;
                listViewModel.ModifiedDate = DateTime.Now;
                listViewModel.StrTmp01 = "";
                listViewModel.StrTmp02 = "";
                TempData["FlagCall"] = "1";
            }
            return PartialView("UpdateListMain", listViewModel);
        }

        /// <summary>
        /// The GetListMain_ByConditions_BindId.
        /// </summary>
        /// <param name="pFlagCallRoot">The pFlagCallRoot<see cref="string"/>.</param>
        /// <param name="pPhanLoai">The pPhanLoai<see cref="string"/>.</param>
        /// <param name="pStatus">The pStatus<see cref="string"/>.</param>
        /// <param name="pTenVT">The pTenVT<see cref="string"/>.</param>
        /// <param name="pTitleChoice">The pTitleChoice<see cref="string"/>.</param>
        /// <param name="pFlagShow">The pFlagShow<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult GetListMain_ByConditions_BindId(string pFlagCallRoot = "0", string pParentId = "", int pStatus = 0, string pShortName = "1", string pTitleChoice = "", string pFlagShow = "1")
        {
            string sTitleChoice = "", sName = "", sShortName = "";
            int iPhanLoai = 0, iFlagCallRoot = 0;

            sTitleChoice = (pTitleChoice == "" || pTitleChoice == null) ? "Chọn danh mục" : pTitleChoice;
            iPhanLoai = (pParentId == "") ? -1 : Convert.ToInt32(pParentId);
            iFlagCallRoot = (pFlagCallRoot == "") ? 0 : Convert.ToInt32(pFlagCallRoot);

            ArrayList data = new ArrayList();
            var lists = _serviceLOV.GetListOfValueSearch(iPhanLoai, "", 0, "", "", pStatus, iFlagCallRoot);
            if (sTitleChoice != "")
                data.Add(new { id = "0", value = sTitleChoice });
            foreach (ListOfValueViewModel item in lists)
            {
                sName = _serviceLOV.ReplaceName_ListMain(item.Name).Trim();
                sShortName = _serviceLOV.ReplaceName_ListMain(item.ShortName).Trim();
                if (pFlagShow == "1") //Hiển thị Tên danh mục
                {
                    if (pShortName == "1")
                        data.Add(new { id = item.Id, value = sShortName });
                    else
                        data.Add(new { id = item.Id, value = sName });
                }
                else
                {
                    if (pShortName == "1")
                        data.Add(new { id = item.Id, value = $"{item.Code} - {sShortName}" });
                    else
                        data.Add(new { id = item.Id, value = $"{item.Code} - {sName}" });
                }
            }
            return Json(data);

        }

        /// <summary>
        /// Hàm lấy mã danh mục tự sinh để trả ra trên màn hình khi người dùng thêm mới danh mục.
        /// </summary>
        /// <param name="pTypeList">Chỉ số xác định nhóm danh mục.</param>
        /// <param name="pLevelList">Cấp danh mục cần thêm.</param>
        /// <returns>Mã danh mục tự sinh.</returns>
        public string GetCodeOfList_ByType(string pTypeList, string pLevelList)
        {
            string retVal = "", codeListAuto = "", sSQL = "", orderNoList = "";
            int iTypeList = 0, iLevelList = 0, iOrderNo = 0;

            iTypeList = (string.IsNullOrEmpty(pTypeList) || pTypeList == "") ? 0 : Convert.ToInt32(pTypeList);
            iLevelList = (string.IsNullOrEmpty(pLevelList) || pLevelList == "") ? 0 : Convert.ToInt32(pLevelList);
            try
            {
                codeListAuto = _serviceLOV.GetCodeOfList_AutoGen(iTypeList, iLevelList);
                //Lấy thêm Số thứ tự danh mục
                if (codeListAuto != "")
                {
                    sSQL = $"Select Cast(IsNull(Max(OrderNo),0) As Varchar(16)) Code From ListOfValue Where (ParentId = {pTypeList} Or Id={pTypeList}) And CategoryLevel={pLevelList}";
                    orderNoList = _serviceLOV.GetCellValueForQuery(sSQL);
                    iOrderNo = Convert.ToInt32(orderNoList) + 1;

                    retVal = $"{codeListAuto}#{iOrderNo.ToString().Trim()}";
                }

                return retVal;
            }
            catch
            {
                retVal = "";
            }
            return retVal;
        }

        /// <summary>
        /// The Save_List.
        /// </summary>
        /// <param name="request">The request<see cref="DataSourceRequest"/>.</param>
        /// <param name="objListViewModel">The objListViewModel<see cref="ListOfValueViewModel"/>.</param>
        /// <returns>The <see cref="IActionResult"/>.</returns>
        [AcceptVerbs("Post")]
        public IActionResult Save_List([DataSourceRequest] DataSourceRequest request, ListOfValueViewModel objListViewModel)
        {
            try
            {
                string result = "0";
                int iRowId = 0;
                if (result == "0" && objListViewModel != null && ModelState.IsValid)
                {
                    if (objListViewModel.CodeOfLovUsedList != null && objListViewModel.CodeOfLovUsedList.Count > 0)
                    {
                        objListViewModel.CodeOfLovUsed = "";
                        string sListTMP = "";
                        List<String> _ARLItems = objListViewModel.CodeOfLovUsedList;
                        for (int i = 0; i < _ARLItems.Count; i++)
                        {
                            sListTMP = sListTMP + "," + _ARLItems[i].ToString().Trim();
                        }
                        objListViewModel.CodeOfLovUsed = Utilities.DeleteChar_FirstAndLast(sListTMP, ",");
                        objListViewModel.CodeOfLovUsed = (objListViewModel.CodeOfLovUsed == "0") ? "" : objListViewModel.CodeOfLovUsed;
                    }
                    //Lấy phân nhóm danh mục cập nhật theo các lựa chọn phân nhóm cấp 1, 2, ...
                    if (objListViewModel.ParentId_05 != 0)
                        objListViewModel.StrTmp01 = objListViewModel.ParentId_05.ToString();
                    else if (objListViewModel.ParentId_04 != 0)
                        objListViewModel.StrTmp01 = objListViewModel.ParentId_04.ToString();
                    else if (objListViewModel.ParentId_03 != 0)
                        objListViewModel.StrTmp01 = objListViewModel.ParentId_03.ToString();
                    else if (objListViewModel.ParentId_02 != 0)
                        objListViewModel.StrTmp01 = objListViewModel.ParentId_02.ToString();
                    else if (objListViewModel.ParentId_01 != 0)
                        objListViewModel.StrTmp01 = objListViewModel.ParentId_01.ToString();
                    else if (objListViewModel.ParentId != 0)
                        objListViewModel.StrTmp01 = objListViewModel.ParentId.ToString();
                    else objListViewModel.StrTmp01 = "0";
                    objListViewModel.ParentId = string.IsNullOrEmpty(objListViewModel.StrTmp01.Trim()) ? 0 : Convert.ToInt32(objListViewModel.StrTmp01);
                    if (objListViewModel.Id <= 0)
                    {
                        objListViewModel.LevelCode = GetLevelTotalCode(objListViewModel.ParentId, objListViewModel.CategoryLevel);
                        objListViewModel.ParentCode = "";
                        ListOfValueViewModel objRootList = _serviceLOV.GetListOfValueForId(objListViewModel.ParentId, Constants.DefaultValue.StatusOpen);
                        if (objRootList != null)
                            objListViewModel.ParentCode = objRootList.Code;
                    }

                    result = InValid_ListUpdate(objListViewModel).ToString();
                    if (result == "0")
                    {
                        ListOfValue modelUpdate = new ListOfValue();
                        modelUpdate.Id = objListViewModel.Id;
                        modelUpdate.ParentId = objListViewModel.ParentId;
                        modelUpdate.OrderNo = objListViewModel.OrderNo;
                        modelUpdate.OrderNoText = objListViewModel.OrderNoText;
                        modelUpdate.ParentCode = objListViewModel.ParentCode;
                        modelUpdate.Code = objListViewModel.Code;
                        modelUpdate.Name = objListViewModel.Name;
                        modelUpdate.ShortName = objListViewModel.ShortName;
                        modelUpdate.Status = objListViewModel.Status;
                        modelUpdate.CodeOfLovUsed = objListViewModel.CodeOfLovUsed;
                        // Update later
                        //modelUpdate.Note = Utilities.Find_Replace(objListViewModel.Notes);
                        modelUpdate.LevelCode = Utilities.Find_Replace(objListViewModel.LevelCode);
                        modelUpdate.SumLevelFlag = objListViewModel.SumLevelFlag;
                        modelUpdate.EditableFlag = objListViewModel.EditableFlag;
                        modelUpdate.LevelCode = objListViewModel.LevelCode;
                        modelUpdate.PrintType = objListViewModel.PrintType;
                        modelUpdate.CategoryLevel = objListViewModel.CategoryLevel;

                        iRowId = _serviceLOV.UpdateListOfValue(modelUpdate, UserName);
                        result = (iRowId != 0) ? "0" : "1";
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{System.Reflection.MethodBase.GetCurrentMethod()} Error: {ex.Message}");
                return new JsonResult("99");
            }
        }

        /// <summary>
        /// Hàm trả lại mã Cấp tổng hợp của Danh mục. Ví dụ 01, 0101, 0102.
        /// </summary>
        /// <param name="pTypeList">Chỉ số xác định phân loại danh mục.</param>
        /// <param name="pLevelList">Cấp Danh mục: 1, 2, 3.</param>
        /// <returns>.</returns>
        public string GetLevelTotalCode(int pTypeList, int pLevelList)
        {
            string retVal = "", sSQL = "";
            sSQL = string.Format("Select Max(LevelCode) Code From ListOfValue Where ParentId={0}", pTypeList);
            string sCapTHTMP = _serviceLOV.GetCellValueForQuery(sSQL);
            int iValTMP = 0;
            if (!string.IsNullOrEmpty(sCapTHTMP))
            {
                switch (pLevelList)
                {
                    case 1:
                        retVal = "01";
                        break;
                    case 2:     //0102
                        iValTMP = Convert.ToInt32(sCapTHTMP.Substring(2, 2)) + 1;
                        retVal = sCapTHTMP.Substring(0, 2) + iValTMP.ToString("D2");
                        break;
                    case 3:     //010203
                        iValTMP = Convert.ToInt32(sCapTHTMP.Substring(4, 2)) + 1;
                        retVal = sCapTHTMP.Substring(0, 4) + iValTMP.ToString("D2");
                        break;
                    case 4:     //01020301
                        iValTMP = Convert.ToInt32(sCapTHTMP.Substring(6, 2)) + 1;
                        retVal = sCapTHTMP.Substring(0, 6) + iValTMP.ToString("D2");
                        break;
                    case 5:     //0102030109
                        iValTMP = Convert.ToInt32(sCapTHTMP.Substring(8, 2)) + 1;
                        retVal = sCapTHTMP.Substring(0, 8) + iValTMP.ToString("D2");
                        break;
                    default:
                        retVal = "00";
                        break;
                }
            }
            return retVal;
        }

        /// <summary>
        /// The InValid_ListUpdate.
        /// </summary>
        /// <param name="_objLovViewModel">The _objLovViewModel<see cref="ListOfValueViewModel"/>.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int InValid_ListUpdate(ListOfValueViewModel _objLovViewModel)
        {
            string sSQL = "";
            int iResult = 0, iValExe = 0;
            try
            {
                if (string.IsNullOrEmpty(_objLovViewModel.ParentId.ToString().Trim()) || _objLovViewModel.ParentId.ToString().Trim() == "" || _objLovViewModel.ParentId.ToString().Trim() == "0")
                    iResult = 1;        //Phân nhóm của danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.OrderNo.ToString().Trim()) || _objLovViewModel.OrderNo.ToString().Trim() == "0")
                    iResult = 2;        //Thứ tự sắp xếp của danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.OrderNoText.ToString().Trim()))
                    iResult = 3;        //Thứ tự hiển thị của danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.CategoryLevel.ToString().Trim()) || _objLovViewModel.CategoryLevel.ToString().Trim() == "0")
                    iResult = 4;        //Cấp (thứ bậc danh mục)
                if (string.IsNullOrEmpty(_objLovViewModel.Code.ToString().Trim()))
                    iResult = 5;        //Mã hiệu danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.Name.ToString().Trim()))
                    iResult = 6;        //Tên gọi danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.ShortName.ToString().Trim()))
                    iResult = 7;        //Tên viết tắt danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.Status.ToString().Trim()) || (_objLovViewModel.Status.ToString().Trim() != "1" && _objLovViewModel.Status.ToString().Trim() != "0"))
                    iResult = 8;        //Trạng thái bản ghi danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.PrintType.ToString().Trim()) || _objLovViewModel.PrintType.ToString().Trim() == "0")
                    iResult = 9;        //Kiểu in của danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.SumLevelFlag.ToString().Trim()))
                    iResult = 10;        //Cờ cộng cấp của danh mục
                if (string.IsNullOrEmpty(_objLovViewModel.EditableFlag.ToString().Trim()))
                    iResult = 11;        //Cờ chỉnh sửa của danh mục
                if (_objLovViewModel.ParentId == Constants.ListOfValueParentValue.ParentIdPosition ||
                    _objLovViewModel.ParentId == Constants.ListOfValueParentValue.ParentIdDepartment ||
                     _objLovViewModel.ParentId == Constants.ListOfValueParentValue.ParentId_UserRoleIDC)
                {
                    if (_objLovViewModel.CodeOfLovUsedList == null || _objLovViewModel.CodeOfLovUsedList.Count <= 0 ||
                        string.IsNullOrEmpty(_objLovViewModel.CodeOfLovUsed) || _objLovViewModel.CodeOfLovUsed == "0")
                        iResult = 12;        //Chọn danh mục được áp dụng
                }
                //Kiểm tra tồn tại mã danh mục cập nhật
                if (_objLovViewModel.Id > 0)
                    sSQL = string.Format("Select Cast(IsNull(Count(Code),0) As NVarchar(32)) Code From ListOfValue Where Code='{0}' And Id <> {1}", _objLovViewModel.Code, _objLovViewModel.Id);
                else
                    sSQL = string.Format("Select Cast(IsNull(Count(Code),0) As NVarchar(32))  Code From ListOfValue Where Code='{0}'", _objLovViewModel.Code);

                iValExe = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));
                if (iValExe != 0)
                    return 13;
                //Kiểm tra tồn tại tên danh mục cập nhật
                if (_objLovViewModel.Id > 0)
                    sSQL = string.Format("Select Cast(IsNull(Count(Code),0) As NVarchar(32)) Code From ListOfValue Where ParentId = {0} And Name=N'{1}' And Id <> {2}", _objLovViewModel.ParentId, _objLovViewModel.Name, _objLovViewModel.Id);
                else
                    sSQL = string.Format("Select Cast(IsNull(Count(Code),0) As NVarchar(32)) Code From ListOfValue Where ParentId = {0} And Name=N'{1}'", _objLovViewModel.ParentId, _objLovViewModel.Name);
                iValExe = Convert.ToInt32(_serviceLOV.GetCellValueForQuery(sSQL));
                if (iValExe != 0)
                    return 14;

                return iResult;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{System.Reflection.MethodBase.GetCurrentMethod()} Error: {ex.Message}");
                return 99;
            }
        }

        /// <summary>
        /// Hàm lấy danh sách file đính kèm của Quyết định/Văn bản/Tài liệu
        /// </summary>
        /// <param name="pDocumentId">Chỉ số xác định Văn bản/Quyết định có file đính kèm</param>
        /// <param name="pFileType">Phân loại:  1 - File cấu hình lãi suất Tide/Casa/DepositPenal;
        ///                                     2 - File đính kèm của người dùng iDC;</param>
        /// <returns>Danh sách file đính kèm của Quyết định/Văn bản/Tài liệu</returns>
        public JsonResult GetListAttachedFileInfo_ForDocumentId(long pDocumentId, string pFileType)
        {
            ArrayList resultData = new ArrayList();
            var listAttachedFileInfo = _intRateConfigService.GetListAttachedFileInfoSearch(0, pDocumentId, pFileType, "", "");
            if (listAttachedFileInfo != null && listAttachedFileInfo.Count != 0)
            {
                foreach (var item in listAttachedFileInfo)
                {
                    string sFileTypeTemp = item.FileType;
                    string[] listItemFileNames = Utilities.Splip_Strings(item.FileNameNew, "_");
                    if (listItemFileNames != null && listItemFileNames.Length != 0)
                    {
                        if (listItemFileNames[0] == FileType.FileType_ConfigIntRate.Code)
                            sFileTypeTemp = FileType.FileType_ConfigIntRate.Value.ToString();
                        else sFileTypeTemp = FileType.FileType_User_IDC.Value.ToString();
                    }
                    if (item.FileNameNew.Contains(item.FileExtension))
                    {
                        resultData.Add(new
                        {
                            OwnerId = item.DocumentId,
                            Id = item.FileId,
                            FileName = item.FileName,
                            Description = item.DocumentNumber,
                            FileNameNew = $"{item.FileNameNew}",
                            FileType = item.FileType,
                            FileExtension = item.FileExtension
                        });
                    }
                    else
                    {
                        resultData.Add(new
                        {
                            OwnerId = item.DocumentId,
                            Id = item.FileId,
                            FileName = item.FileName,
                            Description = item.DocumentNumber,
                            FileNameNew = $"{item.FileNameNew}{item.FileExtension}",
                            FileType = item.FileType,
                            FileExtension = item.FileExtension
                        });
                    }

                }
            }
            return Json(resultData);
        }


        [HttpGet]
        public JsonResult GetConfigStatusOptions()
        {
            var statuses = new List<object>
            {
                new ValueConstModel { Value = -1, Description = "Tất cả", Code = "ALL" },
                ConfigStatus.MAKER,      // { Value = 1, Code = "M", Description = "Tạo lập" }
                ConfigStatus.CLOSED,     // { Value = 0, Code = "C", Description = "Đóng" }
                ConfigStatus.PROCESS,    // { Value = 2, Code = "P", Description = "Chờ duyệt" }
                ConfigStatus.AUTHORIZED, // { Value = 3, Code = "A", Description = "Phê duyệt" }
                ConfigStatus.REJECTED,   // { Value = 4, Code = "R", Description = "Từ chối" }
                ConfigStatus.MODIFIED    // { Value = 5, Code = "M", Description = "Chỉnh sửa" }
            };

            return Json(statuses);
        }

    }
}

