using AutoMapper;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.NetworkInformation;
using VBSPOSS.Constants;
using VBSPOSS.Data;
using VBSPOSS.Data.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.Utils;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class ListOfValueService : IListOfValueService
    {
        /// <summary>
        /// Defines the _dbContext.
        /// </summary>
        private readonly ApplicationDbContext _dbContext;
       

        /// <summary>
        /// Defines the _mapper.
        /// </summary>
        private readonly IMapper _mapper;


        /// <summary>
        /// Initializes a new instance of the <see cref="ListOfValueService"/> class.
        /// </summary>
        /// <param name="dbContext">The dbContext<see cref="ApplicationDbContext"/>.</param>
        /// <param name="mapper">The mapper<see cref="IMapper"/>.</param>
        public ListOfValueService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Hàm thực hiện trả về danh sách giá trị Trực thuộc để lấy danh sách Phòng ban theo mã POS truyền vào.
        /// </summary>
        /// <param name="pPosCode">Mã Pos truyền vào</param>
        /// <returns>Chỉ số xác định trực thuộc với Quy ước: 
        ///                              "1" - Hoi so chinh;
        ///                              "2" - Chi nhanh Tinh/TP;
        ///                              "3" - Van phong dai dien;
        ///                              "4" - Phong giao dich Quan/Huyen;
        ///                              "5" - Trung tam CNTT;
        ///                              "7" - Trung tam Đao tạo;
        ///                              "9" - Sở giao dịch;
        /// </returns>
        public string GetCodeApplyByPosCode(string pPosCode)
        {
            string sResultVal = "";
            if (pPosCode == "000100" || pPosCode == "000101" || pPosCode == "000196" || pPosCode == "000197" || pPosCode == "000199")
            {
                switch (pPosCode)
                {
                    case "000199":
                        sResultVal = "1";  //Hội sở chính
                        break;
                    case "000100":
                        sResultVal = "1";  //Hội sở chính
                        break;
                    case "000101":
                        sResultVal = "9";  //Sở giao dịch
                        break;
                    case "000196":
                        sResultVal = "5";  //TT Công nghệ thông tin 
                        break;
                    case "000197":
                        sResultVal = "7";  //Trung tâm đào tạo
                        break;
                    default:
                        sResultVal = "3";  //Văn phòng đại diện
                        break;
                }
            }
            else
            {
                if (pPosCode == "002821" || pPosCode == "001114" || pPosCode == "002734" || pPosCode == "003799" || pPosCode == "004532" || pPosCode == "005399")
                    sResultVal = "6";
                else
                {
                    string sSQL = string.Format($"Select Top 1 X.MaSoCN Code From ChiNhanh X Where X.MaSo = '{pPosCode}' Order By X.TrangThai Desc");
                    var detailPosCode = _dbContext.CellValues.FromSqlRaw(sSQL).FirstOrDefault();
                    if (detailPosCode != null)
                        sResultVal = (detailPosCode.Code == pPosCode) ? "2" : "4";
                }
            }
            return sResultVal;
        }

        /// <summary>
        /// Hàm thực hiện chuẩn hóa tên danh mục. Vú dụ: "(CN cấp I &tương đương)" thay bằng rỗng
        /// </summary>
        /// <param name="pValueName">Chuỗi cần chuẩn hóa</param>
        /// <returns>Chuỗi được chuẩn hóa</returns>
        public string ReplaceName_ListMain(string pValueName)
        {
            string resultName = "", nameTemp = "";
            if (pValueName != "")
            {
                nameTemp = pValueName.Trim();
                nameTemp = nameTemp.Replace("Ban Kiểm tra kiểm soát nội bộ khu vực Miền nam", "Ban KTKSNB khu vực Miền nam").Trim();
                nameTemp = nameTemp.Replace("đối tượng chính sách", "ĐTCS").Trim();
                nameTemp = nameTemp.Replace("(CN)", "").Trim();
                nameTemp = nameTemp.Replace("(CN cấp I &tương đương)", "").Trim();
                nameTemp = nameTemp.Replace("(Phòng giao dịch)", "").Trim();
                resultName = nameTemp.Trim();
            }
            return resultName;
        }

        /// <summary>
        /// Hàm lấy giá trị của ô (Cell) trong bảng dữ liệu cần lấy theo câu truy vấn SQL truyền vào
        /// </summary>
        /// <param name="pQuerySelect">Truy vấn SQL truyền vào. Ex: "Select Code From ListOfValue Where ParentId=14" /param>
        /// <returns>Giá trị trả về. Ex: '1400'</returns>
        public string GetCellValueForQuery(string pQuerySelect)
        {
            string sResult = "";
            if (!string.IsNullOrEmpty(pQuerySelect))
            {
                var valueList = _dbContext.CellValues.FromSqlRaw(pQuerySelect).ToList();
                var detailCell = valueList.FirstOrDefault();
                if (detailCell != null)
                {
                    sResult = detailCell.Code.ToString();
                }
            }
            return sResult;
        }

        /// <summary>
        /// Hàm sinh mã tự động cho danh mục cần thêm mới
        /// </summary>
        /// <param name="pParentId">Phân loại danh mục. Ex: 14 - Chức vụ</param>
        /// <param name="pLevelList">Cấp của danh mục cần thêm mới. Ex: Cấp danh mục cần thêm. Ex: 2</param>
        /// <returns>Mã danh mục tự sinh</returns>
        public string GetCodeOfList_AutoGen(int pParentId, int pLevelList)
        {
            string retVal = "";
            try
            {
                if (pLevelList == 1)
                    retVal = "9900";
                else
                {
                    string sSQL = $"Select dbo.GetCodeOfList_AutoGen({pParentId},{pLevelList}) As Code";
                    retVal = GetCellValueForQuery(sSQL);
                }
            }
            catch
            {
                retVal = "";
            }
            return retVal;
        }

        /// <summary>
        /// Hàm lấy tên danh mục từ mã và phân loại danh mục một số LOV fix cứng
        /// </summary>
        /// <param name="valueCode">Mã danh mục LOV fix cứng</param>
        /// <param name="tyleList">Loại danh mục fix cứng</param>
        /// <returns>Tên danh mục LOV fix cứng</returns>
        public string GetNameByValue(string valueCode, string tyleList)
        {
            string resultName = "";
            if (!string.IsNullOrEmpty(valueCode))
            {
                if (tyleList == TypeLOVValue.PrintType)
                {
                    if (valueCode == "0" || valueCode == PrintTypeValue.BoldValue.ToString())
                        resultName = Constants.PrintTypeValue.BoldValue_Text;
                    else if (valueCode == PrintTypeValue.ItalicValue.ToString())
                        resultName = Constants.PrintTypeValue.ItalicValue_Text;
                    else if (valueCode == PrintTypeValue.NormalValue.ToString())
                        resultName = Constants.PrintTypeValue.NormalValue_Text;
                    else if (valueCode == PrintTypeValue.ItalicBoldValue.ToString())
                        resultName = Constants.PrintTypeValue.ItalicBoldValue_Text;
                    else if (valueCode == PrintTypeValue.UnderlineValue.ToString())
                        resultName = Constants.PrintTypeValue.UnderlineValue_Text;
                    else resultName = "";
                }
                else if (tyleList == TypeLOVValue.CodeOfLovUsed)
                {
                    if (valueCode == CodeOfLovUsed.CodeOfLovUsed_Head)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_Head;
                    else if (valueCode == CodeOfLovUsed.CodeOfLovUsed_Branch)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_Branch;
                    else if (valueCode == CodeOfLovUsed.CodeOfLovUsed_DomainOffice)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_DomainOffice;
                    else if (valueCode == CodeOfLovUsed.CodeOfLovUsed_District)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_District;
                    else if (valueCode == CodeOfLovUsed.CodeOfLovUsed_ITC)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_ITC;
                    else if (valueCode == CodeOfLovUsed.CodeOfLovUsed_TrainingFacility)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_TrainingFacility;
                    else if (valueCode == CodeOfLovUsed.CodeOfLovUsed_TrainingCenter)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_TrainingCenter;
                    else if (valueCode == CodeOfLovUsed.CodeOfLovUsed_BankTransactionOffice)
                        resultName = CodeOfLovUsedText.CodeOfLovUsed_BankTransactionOffice;
                    else resultName = "";
                }
                else if (tyleList == TypeLOVValue.RegionList)
                {
                    if (valueCode == RegionValue.DongBangSongHong)
                        resultName = RegionValueText.DongBangSongHong;
                    else if (valueCode == RegionValue.TrungDuMienNuiPhiaBac)
                        resultName = RegionValueText.TrungDuMienNuiPhiaBac;
                    else if (valueCode == RegionValue.BacTrungBoDuyenHaiMienTrung)
                        resultName = RegionValueText.BacTrungBoDuyenHaiMienTrung;
                    else if (valueCode == RegionValue.TayNguyen)
                        resultName = RegionValueText.TayNguyen;
                    else if (valueCode == RegionValue.DongNamBo)
                        resultName = RegionValueText.DongNamBo;
                    else if (valueCode == RegionValue.DongBangSongCL)
                        resultName = RegionValueText.DongBangSongCL;
                    else resultName = "";
                }
            }
            return resultName;
        }

        /// <summary>
        /// Hàm trả về Danh sách danh mục chung theo những điều kiện truyền vào: ListOfValue
        /// </summary>
        /// <param name="pParentId">Chỉ số phân loại danh mục cha (Không bắt buộc)</param>
        /// <param name="pParentCode">Mã số danh mục cha (Không bắt buộc)</param>
        /// <param name="pId">Chỉ số Id danh mục (Không bắt buộc)</param>
        /// <param name="pCode">Mã danh mục (Không bắt buộc)</param>
        /// <param name="pName">Tên danh mục (Không bắt buộc)</param>
        /// <param name="pStatus">Trạng thái danh mục (Không bắt buộc). Nếu truyền -1 lấy tất; Nếu truyền 1 lấy danh mục mở</param>
        /// <param name="pFlagCallRoot">Cờ xác định Chỉ lấy danh mục gốc/con/tất cả: 0 - Lấy tất cả; 1 - Chỉ Lấy gốc; 2 - Chỉ Lấy dm con</param>
        /// <returns>Danh sách bản ghi</returns>
        public List<ListOfValueViewModel> GetListOfValueSearch(int pParentId, string pParentCode, int pId, string pCode, string pName, int pStatus, int pFlagCallRoot)
        {
            var answer = new List<ListOfValueViewModel>();
            try
            {
                int iCount = 0;
                string sMaApDung = "";
                var profileListRoots = _dbContext.ListOfValues.Where(w => w.ParentId == 0).OrderBy(o => o.Code).ToList();

                var profileListTMPs = _dbContext.ListOfValues.Where(w => w.Id != 0 && (string.IsNullOrEmpty(pParentCode) || w.ParentCode == pParentCode)
                                        && (string.IsNullOrEmpty(pCode) || w.Code == pCode)
                                        && (string.IsNullOrEmpty(pName) || w.Name.Contains(pName))
                                        && (pParentId == -1 || w.ParentId == pParentId)
                                        && (pId == 0 || w.Id == pId)
                                        && (pStatus == -1 || w.Status == pStatus)
                                        ).OrderBy(o => o.Code).ThenBy(o => o.ParentCode).ThenBy(o => o.OrderNo).ThenBy(o => o.OrderNoText).ThenBy(o => o.PrintType).ToList();
                List<ListOfValue> profileLists = new List<ListOfValue>();
                if (pFlagCallRoot == 1)
                    profileLists = profileListTMPs.Where(w => w.EditableFlag == 0).OrderBy(o => o.Code).ThenBy(o => o.ParentCode).ThenBy(o => o.OrderNo).ThenBy(o => o.OrderNoText).ThenBy(o => o.PrintType).ToList();
                else if (pFlagCallRoot == 2)
                    profileLists = profileListTMPs.Where(w => w.EditableFlag == 1).OrderBy(o => o.Code).ThenBy(o => o.ParentCode).ThenBy(o => o.OrderNo).ThenBy(o => o.OrderNoText).ThenBy(o => o.PrintType).ToList();
                else
                    profileLists = profileListTMPs.Where(w => w.Id != 0).OrderBy(o => o.Code).ThenBy(o => o.ParentCode).ThenBy(o => o.OrderNo).ThenBy(o => o.OrderNoText).ThenBy(o => o.PrintType).ToList();

                foreach (var item in profileLists)
                {
                    iCount++;
                    ListOfValueViewModel objItem = new ListOfValueViewModel();
                    objItem = _mapper.Map<ListOfValueViewModel>(item);
                    objItem.StatusText = (item.Status == Constants.StatusLov.StatusOpen) ? Constants.StatusLov.StatusOpen_Text : Constants.StatusLov.StatusClosed_Text;
                    if (item.ParentId != 0)
                        objItem.ParentText = profileListRoots.Where(s => s.Id == item.ParentId).Select(w => w.Name).FirstOrDefault();
                    else objItem.ParentText = item.Name;
                    
                    objItem.PrintTypeText = GetNameByValue(item.PrintType.ToString().Trim(), TypeLOVValue.PrintType);
                    objItem.OrderNoAll = iCount;
                    if (!string.IsNullOrEmpty(objItem.CodeOfLovUsed))
                    {
                        if (item.ParentId == ParentLovValue.Parent_Title_Id || item.ParentId == ParentLovValue.Parent_Department_Id)
                        {
                            sMaApDung = objItem.CodeOfLovUsed.Replace(",", "; ");
                            objItem.CodeOfLovUsedText = sMaApDung.Replace("1", "Hội sở chính").Replace("2", "Chi nhánh Tỉnh/TP").Replace("3", "VP đại diện").Replace("4", "Phòng giao dịch").Replace("5", "TTCNTT").Replace("6", "Cơ sở đào tạo").Replace("7", "TTĐT").Replace("9", "Sở giao dịch");
                        }
                    }
                    answer.Add(objItem);
                }
                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Bản ghi thông tin danh mục theo Id danh mục truyền vào
        /// </summary>
        /// <param name="pId">Chỉ số xác định bản ghi</param>
        /// <param name="pStatus">Trạng thái danh mục (Không bắt buộc). Nếu truyền -1 lấy tất; Nếu truyền 1 lấy danh mục mở</param>
        /// <returns>Bản ghi danh mục trả ra</returns>
        public ListOfValueViewModel GetListOfValueForId(int pId, int pStatus)
        {
            try
            {
                string sMaApDung = "";
                var profileListRoots = _dbContext.ListOfValues.Where(w => w.ParentId == 0).OrderBy(o => o.Id).ToList();

                var profileList = _dbContext.ListOfValues.Where(w => w.Id == pId
                                        && (pStatus == -1 || w.Status == pStatus)
                                        ).OrderBy(o => o.Code).ThenBy(o => o.ParentCode).ThenBy(o => o.OrderNo).ThenBy(o => o.OrderNoText).ThenBy(o => o.PrintType).FirstOrDefault();
                if (profileList == null)
                {
                    return null;
                }
                var answer = new ListOfValueViewModel();
                answer = _mapper.Map<ListOfValueViewModel>(profileList);
                answer.StatusText = (profileList.Status == Constants.StatusLov.StatusOpen) ? Constants.StatusLov.StatusOpen_Text : Constants.StatusLov.StatusClosed_Text;
                if (answer.ParentId != 0)
                    answer.ParentText = profileListRoots.Where(s => s.Id == profileList.ParentId).Select(w => w.Name).FirstOrDefault();
                else answer.ParentText = answer.Name;
                answer.PrintTypeText = GetNameByValue(profileList.PrintType.ToString().Trim(), TypeLOVValue.PrintType);
                answer.OrderNoAll = 1;
                if (!string.IsNullOrEmpty(answer.CodeOfLovUsed))
                {
                    if (answer.ParentId == ParentLovValue.Parent_Title_Id || answer.ParentId == ParentLovValue.Parent_Department_Id)
                    {
                        sMaApDung = answer.CodeOfLovUsed.Replace(",", "; ");
                        answer.CodeOfLovUsedText = sMaApDung.Replace("1", "Hội sở chính").Replace("2", "Chi nhánh Tỉnh/TP").Replace("3", "VP đại diện").Replace("4", "Phòng giao dịch").Replace("5", "TTCNTT").Replace("6", "Cơ sở đào tạo").Replace("7", "TTĐT").Replace("9", "Sở giao dịch");
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
        /// Bản ghi thông tin danh mục theo Mã số danh mục truyền vào
        /// </summary>
        /// <param name="pCode">Mã số xác định bản ghi</param>
        /// <param name="pStatus">Trạng thái danh mục (Không bắt buộc). Nếu truyền -1 lấy tất; Nếu truyền 1 lấy danh mục mở</param>
        /// <returns>Bản ghi danh mục trả ra</returns>
        public ListOfValueViewModel GetListOfValueByCode(string pCode, int pStatus)
        {
            try
            {
                string sMaApDung = "";
                var profileListRoots = _dbContext.ListOfValues.Where(w => w.ParentId == 0).OrderBy(o => o.Id).ToList();

                var profileList = _dbContext.ListOfValues.Where(w => w.Code == pCode
                                        && (pStatus == -1 || w.Status == pStatus)
                                        ).OrderBy(o => o.Code).ThenBy(o => o.ParentCode).ThenBy(o => o.OrderNo).ThenBy(o => o.OrderNoText).ThenBy(o => o.PrintType).FirstOrDefault();
                if (profileList == null)
                {
                    return null;
                }
                var answer = new ListOfValueViewModel();
                answer = _mapper.Map<ListOfValueViewModel>(profileList);
                answer.StatusText = (profileList.Status == Constants.StatusLov.StatusOpen) ? Constants.StatusLov.StatusOpen_Text : Constants.StatusLov.StatusClosed_Text;
                if (answer.ParentId != 0)
                    answer.ParentText = profileListRoots.Where(s => s.Id == profileList.ParentId).Select(w => w.Name).FirstOrDefault();
                else answer.ParentText = answer.Name;
                answer.PrintTypeText = GetNameByValue(profileList.PrintType.ToString().Trim(), TypeLOVValue.PrintType);
                answer.OrderNoAll = 1;
                if (!string.IsNullOrEmpty(answer.CodeOfLovUsed))
                {
                    if (answer.ParentId == ParentLovValue.Parent_Title_Id || answer.ParentId == ParentLovValue.Parent_Department_Id)
                    {
                        sMaApDung = answer.CodeOfLovUsed.Replace(",", "; ");
                        answer.CodeOfLovUsedText = sMaApDung.Replace("1", "Hội sở chính").Replace("2", "Chi nhánh Tỉnh/TP").Replace("3", "VP đại diện").Replace("4", "Phòng giao dịch").Replace("5", "TTCNTT").Replace("6", "Cơ sở đào tạo").Replace("7", "TTĐT").Replace("9", "Sở giao dịch");
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
        /// Hàm Cập nhật (Thêm mới/Sửa đổi) bản ghi vào bảng danh mục chung
        /// </summary>
        /// <param name="model">Thông tin danh mục chung</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <returns>Chỉ số Id danh mục được thêm/sửa</returns>
        public int UpdateListOfValue(ListOfValue objModelUpd, string pUserName)
        {
            int iResultId = 0, iSaveChanges = 0;
            try
            {
                DateTime currentDateVal = DateTime.Now;
                if (objModelUpd.Id != 0)
                {
                    var objListOfValue = _dbContext.ListOfValues.Where(m => m.Id == objModelUpd.Id).FirstOrDefault();
                    if (objListOfValue != null && objListOfValue.Id != 0)
                    {
                        objListOfValue.OrderNo = objModelUpd.OrderNo;
                        objListOfValue.OrderNoText = objModelUpd.OrderNoText;
                        objListOfValue.Name = objModelUpd.Name;
                        objListOfValue.ShortName = objModelUpd.ShortName;
                        objListOfValue.Status = objModelUpd.Status;
                        objListOfValue.CodeOfLovUsed = objModelUpd.CodeOfLovUsed;
                        objListOfValue.Notes = objModelUpd.Notes;
                        objListOfValue.LevelCode = objModelUpd.LevelCode;
                        objListOfValue.SumLevelFlag = objModelUpd.SumLevelFlag;
                        objListOfValue.EditableFlag = objModelUpd.EditableFlag;
                        objListOfValue.PrintType = objModelUpd.PrintType;
                        objListOfValue.CategoryLevel = objModelUpd.CategoryLevel;
                        objListOfValue.ModifiedBy = pUserName;
                        objListOfValue.ModifiedDate = currentDateVal;

                        _dbContext.Entry(objListOfValue).Property(x => x.OrderNo).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.OrderNoText).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.Name).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.ShortName).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.CodeOfLovUsed).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.Notes).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.LevelCode).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.SumLevelFlag).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.EditableFlag).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.PrintType).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.CategoryLevel).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.ModifiedDate).IsModified = true;
                        iSaveChanges = _dbContext.SaveChanges();
                        if (iSaveChanges > 0)
                            iResultId = objListOfValue.Id;
                    }
                } 
                else
                {
                    objModelUpd.ModifiedBy = pUserName;
                    objModelUpd.ModifiedDate = currentDateVal;
                    objModelUpd.CreatedBy = pUserName;
                    objModelUpd.CreatedDate = currentDateVal;
                    _dbContext.ListOfValues.Add(objModelUpd);
                    iSaveChanges = _dbContext.SaveChanges();
                    if (iSaveChanges > 0)
                        iResultId = objModelUpd.Id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iResultId;
        }

        /// <summary>
        /// Hàm Xóa/Đánh dấu xóa bản ghi Danh mục chung
        /// </summary>
        /// <param name="pListId">Chỉ số xác định danh mục</param>
        /// <param name="pUserName">Người cập nhật</param>
        /// <param name="pFlagDelete">Trạng thái quy ước: 1 - Xóa bản ghi; 2 - Đánh dấu xóa (Chuyển trạng thại về 0)</param>
        /// <returns>Tru - Thành công; False - Thất bại</returns>
        public bool DeleteListOfValue(int pListId, string pUserName, int pFlagDelete)
        {
            bool bResult = false;
            try
            {
                var objListOfValue = _dbContext.ListOfValues.Where(m => m.Id == pListId).FirstOrDefault();
                if (objListOfValue != null && objListOfValue.Id != 0)
                {
                    if (pFlagDelete == 1)
                    {
                        _dbContext.ListOfValues.Remove(objListOfValue);
                        return (_dbContext.SaveChanges() > 0);
                    }
                    else if (pFlagDelete == 2)
                    {
                        objListOfValue.Status = StatusLov.StatusClosed;
                        objListOfValue.ModifiedBy = pUserName;
                        objListOfValue.ModifiedDate = DateTime.Now;
                        _dbContext.Entry(objListOfValue).Property(x => x.Status).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.ModifiedBy).IsModified = true;
                        _dbContext.Entry(objListOfValue).Property(x => x.ModifiedDate).IsModified = true;
                        return (_dbContext.SaveChanges() > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bResult;
        }

        /// <summary>
        /// Hàm trả về Cấp của PosCode truyền vào. Giá trị quy ước: 3 - TW; 2 - Chi nhánh; 1 - PGD.
        /// </summary>
        /// <param name="pPosCode">Mã POS truyền vào.</param>
        /// <returns>Chỉ số xác định cấp của POSCODE.</returns>
        public int GetLevelPosCode(string pPosCode)
        {
            int sLevel = 1; 
            string sSQL = "";
            if (pPosCode == "000100" || pPosCode == "000199")
                sLevel = VBSPOSS.Constants.PosGrade.HEAD_POS;
            else if (pPosCode == "000101" || pPosCode == "000197" || pPosCode == "000196")
                sLevel = VBSPOSS.Constants.PosGrade.MAIN_POS;
            else
            {
                sSQL = string.Format($"Select Top 1 X.Code Code From ListOfPos X Where X.Code = '{pPosCode}' Order By X.Status Desc");
                var detailPosCode = _dbContext.CellValues.FromSqlRaw(sSQL).FirstOrDefault();
                if (detailPosCode != null)
                {
                    sLevel = (detailPosCode.Code == pPosCode) ? VBSPOSS.Constants.PosGrade.MAIN_POS : VBSPOSS.Constants.PosGrade.SUB_POS;
                }
            }
            return sLevel;
        }

        /// <summary>
        /// Hàm lấy danh sách bản ghi Chi nhánh theo điều kiện truyền vào
        /// </summary>
        /// <param name="pFlagCondi">Điều kiện lấy dữ liệu chính Chi nhánh. Giá trị quy ước:
        ///          '1' - Lấy duy nhất POS Hội sở chính (000100)
        ///          '2' - Lấy danh sách các POS HSC và Chi nhánh Tỉnh/TP (Danh sách MainPOS ĐK MaSo = MaSoCN Hoặc PhanLoai IN (0,1)
        ///          '3' - Lấy danh sách các POS Chi nhánh/PGD, trừ POS Hội sở chính (000100)
        ///          '4' - Lấy danh sách các POS HSC/Chi nhánh: Cấp TQ lấy tất cả; Cấp Chi nhánh/PGD Chỉ lấy POS của chi nhánh; => Phải truyền thêm pPosCodeUser
        ///          '5' - Lấy danh sách các POS HSC/Chi nhánh/PGD: Cấp TQ lấy tất cả; Cấp Chi nhánh/PGD Chỉ lấy POS của chi nhánh; PGD lấy duy nhất POS PGD => Phải truyền thêm pPosCodeUser
        /// </param>
        ///  <param name="pDefaultValue">Giá trị mặc định (ví dụ: 0 cho logic mặc định, có thể dùng để giới hạn hoặc điều kiện bổ sung)</param>
        /// <param name="pMainPosCode">Mã chi nhánh. Không sử dụng truyền vào là ''</param>
        /// <param name="pPosCode">Mã POS. Không sử dụng truyền vào là ''</param>
        /// <param name="pStatus">Trạng thái bản ghi</param>
        /// <param name="pPosCodeUser">Mã pos của người dùng gọi đến</param>
        /// <param name="pUserName">Tên đăng nhập người dùng</param>
        /// <returns>Danh sách bản ghi Chi nhánh</returns>
        public List<ListOfPosViewModel> GetBranchSearch(string pFlagCondi, int pDefaultValue, string pMainPosCode, string pPosCode, string pStatus, string pPosCodeUser, string pUserName)
        {
            var answer = new List<ListOfPosViewModel>();
            try
            {
                int userNameLevel = 1;
                var profileBranchRoots = _dbContext.ListOfPoss.Where(w => !string.IsNullOrEmpty(w.Code) && (string.IsNullOrEmpty(pMainPosCode) || w.MainPosCode == pMainPosCode)
                                                                                && (string.IsNullOrEmpty(pPosCode) || w.Code == pPosCode)).OrderBy(o => o.Code).ToList();

                var profileBranchTMPs = _dbContext.ListOfPoss.Where(w => !string.IsNullOrEmpty(w.Code) && (string.IsNullOrEmpty(pMainPosCode) || w.MainPosCode == pMainPosCode)
                                        && (string.IsNullOrEmpty(pPosCode) || w.Code == pPosCode) && (string.IsNullOrEmpty(pStatus) || w.Status == pStatus)
                                        ).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();

                List<ListOfPos> profileBranchLists = new List<ListOfPos>();
                userNameLevel = GetLevelPosCode(pPosCodeUser);
                if (pFlagCondi == "1")
                    profileBranchLists = profileBranchTMPs.Where(w => w.Code == "000100").OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                else if (pFlagCondi == "2")
                {
                    if (userNameLevel == PosGrade.HEAD_POS)
                        profileBranchLists = profileBranchTMPs.Where(w => w.Code == w.MainPosCode || (w.PosFlag == PosGrade.PosGrade_HeadPos || w.PosFlag == PosGrade.PosGrade_MainPos)).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                    else if (userNameLevel == PosGrade.MAIN_POS)
                        profileBranchLists = profileBranchTMPs.Where(w => w.MainPosCode == pPosCodeUser &&
                                                (w.Code == w.MainPosCode || w.PosFlag == PosGrade.PosGrade_HeadPos || w.PosFlag == PosGrade.PosGrade_MainPos))
                                                .OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                    else
                        profileBranchLists = profileBranchTMPs.Where(w => w.Code.Substring(0, 4) == pPosCodeUser.Substring(0, 4) &&
                                                    (w.Code == w.MainPosCode || w.PosFlag == PosGrade.PosGrade_HeadPos || w.PosFlag == PosGrade.PosGrade_MainPos))
                                                    .OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                }
                else if (pFlagCondi == "3")
                {
                    if (userNameLevel == PosGrade.HEAD_POS)
                        profileBranchLists = profileBranchTMPs.Where(w => w.Code == w.MainPosCode && w.PosFlag == PosGrade.PosGrade_HeadPos).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                    else if (userNameLevel == PosGrade.MAIN_POS)
                        profileBranchLists = profileBranchTMPs.Where(w => w.MainPosCode == pPosCodeUser
                                    && w.Code == w.MainPosCode && w.PosFlag == PosGrade.PosGrade_HeadPos).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                    else
                        profileBranchLists = profileBranchTMPs.Where(w => w.Code.Substring(0, 4) == pPosCodeUser.Substring(0, 4)
                                    && w.Code == w.MainPosCode && w.PosFlag == PosGrade.PosGrade_HeadPos).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                }
                else if (pFlagCondi == "4")
                {
                    if (userNameLevel == PosGrade.HEAD_POS)
                        profileBranchLists = profileBranchTMPs;
                    else if (userNameLevel == PosGrade.MAIN_POS)
                        profileBranchLists = profileBranchTMPs.Where(w => w.MainPosCode == pPosCodeUser && w.PosFlag == PosGrade.PosGrade_MainPos).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ThenBy(o => o.Status).ToList();
                    else
                        profileBranchLists = profileBranchTMPs.Where(w => w.Code.Substring(0, 4) == pPosCodeUser.Substring(0, 4) && w.PosFlag == PosGrade.PosGrade_MainPos).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ThenBy(o => o.Status).ToList();
                }
                else if (pFlagCondi == "5" || pFlagCondi == "6")
                {
                    if (userNameLevel == PosGrade.HEAD_POS)
                        profileBranchLists = profileBranchTMPs;
                    else if (userNameLevel == PosGrade.MAIN_POS)
                        profileBranchLists = profileBranchTMPs.Where(w => w.MainPosCode == pPosCodeUser).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ThenBy(o => o.Status).ToList();
                    else
                        profileBranchLists = profileBranchTMPs.Where(w => w.Code == pPosCodeUser).OrderBy(o => o.MainPosCode).ThenBy(o => o.PosFlag).ThenBy(o => o.Code).ToList();
                }
                else profileBranchLists = profileBranchTMPs;

                foreach (var item in profileBranchLists)
                {
                    if (item.Code != "999999")
                    {
                        ListOfPosViewModel objItem = new ListOfPosViewModel();
                        objItem = _mapper.Map<ListOfPosViewModel>(item);
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
        /// Hàm lấy danh sách thông tin Xã/Phường/Thị trấn (Bao gồm cả thông tin Mã/Tên/Ngày của điểm GDX)
        /// </summary>
        /// <param name="pProvinceCode">Mã tỉnh/thành phố (Không bắt buộc)</param>
        /// <param name="pDistrictCode">Mã quận/huyện/thị xã (Không bắt buộc)</param>
        /// <param name="pCommuneCode">Mã xã/phường/thị trấn</param>
        /// <param name="pPosCode">Mã POS (Không bắt buộc)</param>
        /// <returns>Danh sách Xã/Phường/Thị trấn</returns>
        public List<ListOfCommuneViewModel> GetLovCommuneList(string pProvinceCode, string pDistrictCode, string pCommuneCode, string pPosCode, string pSubCommuneCode)
        {
            var answer = new List<ListOfCommuneViewModel>();
            try
            {
                if (string.IsNullOrEmpty(pPosCode) && string.IsNullOrEmpty(pProvinceCode) && string.IsNullOrEmpty(pDistrictCode) && string.IsNullOrEmpty(pCommuneCode))
                {
                    return answer;
                }
               
                var profileBranchTMPs = _dbContext.ListOfCommunes.Where(w => string.IsNullOrEmpty(pProvinceCode)|| (w.ProvinceCode ==  pProvinceCode)
                                        && (string.IsNullOrEmpty(pDistrictCode) || w.DistrictCode == pDistrictCode)
                                        && (string.IsNullOrEmpty(pPosCode) || w.PosCode == pPosCode)
                                        && (string.IsNullOrEmpty(pCommuneCode) || w.CommuneCode == pCommuneCode)
                                        && (string.IsNullOrEmpty(pSubCommuneCode) || w.SubCommuneCode == pSubCommuneCode)
                                        ).OrderBy(o => o.ProvinceCode).ThenBy(o => o.DistrictCode).ThenBy(o => o.CommuneCode).ThenBy(o => o.SubCommuneCode).ToList();
                
                if(profileBranchTMPs != null)
                {
                    foreach (var item in profileBranchTMPs)
                    {
                        ListOfCommuneViewModel objItem = new ListOfCommuneViewModel();
                        objItem = _mapper.Map<ListOfCommuneViewModel>(item);                      
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
        /// Hàm lấy danh sách sản phẩm TIDE/CASA theo điều kiện truyền vào (Danh sách trong bảng ListOfProducts)
        /// </summary>
        /// <param name="pProductGroupCode">Loại sản phẩm: CASA/TIDE</param>
        /// <param name="pProductCode">Mã sản phẩm</param>
        /// <param name="pAccountTypeCode">Loại tài khoản</param>
        /// <param name="pCode">Mã sản phẩm/Loại tài khoản</param>
        /// <param name="pName">Tên sản phẩm/Loại tài khoản</param>
        /// <param name="pStatus">Trạng thái (Nếu -1 lấy tất cả)</param>
        /// <param name="pIsApplyPosFlag">Xác định xem có lấy theo sản phẩm được áp dụng cho POS không. 0 - Không xét; 1 - Có xét</param>
        /// <param name="pUserGrade">Cấp của người dùng: PosGrade.SUB_POS = 1; PosGrade.MAIN_POS = 2; PosGrade.HEAD_POS = 3;</param>
        /// <param name="pProductGroupCodeParams">Loại sản phẩm để ánh xạ vào bảng DL tham số cấu hình ProductParameters: CASA/TIDE/DEPOSITPENAL</param>
        /// <returns>Danh sách trong bảng ListOfProducts</returns>
        public List<ListOfProducts> GetListOfProductsSearch(string pProductGroupCode, string pProductCode, string pAccountTypeCode, string pCode, string pName,
                                    int pStatus, int pIsApplyPosFlag, int pUserGrade, string pProductGroupCodeParams)
        {
            var answer = new List<ListOfProducts>();
            try
            {
                List<ListOfProducts> listProductTemps = new List<ListOfProducts>();
                listProductTemps = _dbContext.ListOfProducts.Where(w => (string.IsNullOrEmpty(pProductGroupCode) || w.ProductGroupCode == pProductGroupCode)
                                        && (string.IsNullOrEmpty(pProductCode) || w.ProductCode == pProductCode)
                                        && (string.IsNullOrEmpty(pAccountTypeCode) || w.AccountTypeCode == pAccountTypeCode)
                                        && (string.IsNullOrEmpty(pCode) || w.Code == pCode)
                                        && ((pStatus == -1) || w.Status == pStatus)
                                        )
                                .Where(delegate (ListOfProducts c)
                                {
                                    if (string.IsNullOrEmpty(pName)
                                        || (c.Name != null && c.Name.ToLower().Contains(pName.ToLower()))
                                        || (c.Name != null && Utilities.ConvertToUnSign(c.Name.ToLower()).IndexOf(pName.ToLower(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                                        )
                                        return true;
                                    else
                                        return false;
                                })
                                .OrderBy(o => o.ProductGroupCode).ThenBy(o => o.ProductCode).ThenBy(o => o.AccountTypeCode).ThenBy(o => o.Name).ToList();
                if (pIsApplyPosFlag == 1 && pUserGrade != PosGrade.HEAD_POS)
                {
                    var listProductParameters = _dbContext.ProductParameters.Where(w => w.ApplyPosFlag == 1 && w.EffectedDate <= DateTime.Now.Date
                                        && (string.IsNullOrEmpty(pProductGroupCodeParams) || w.ProductGroupCode == pProductGroupCodeParams)
                                        && (string.IsNullOrEmpty(pProductCode) || w.ProductCode == pProductCode)
                                        && (string.IsNullOrEmpty(pCode) || w.ProductCode == pCode)
                                        && (w.Status == ConfigStatus.AUTHORIZED.Value || w.Status == ConfigStatus.MAKER.Value)).ToList();
                    var dMaxEffectedDate = listProductParameters.Where(w => w.Status == ConfigStatus.AUTHORIZED.Value).Select(s => s.EffectedDate).DefaultIfEmpty().Max();

                    var listProductParameters01 = listProductParameters.Where(w => w.EffectedDate == dMaxEffectedDate.Date).ToList();

                    if (listProductTemps != null && listProductTemps.Count != 0)
                        answer = listProductTemps.Where(w => listProductParameters01.Select(s => s.ProductCode).Contains(w.ProductCode)).ToList();
                    else answer = listProductTemps;
                }
                else answer = listProductTemps;

                return answer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




    }
}
