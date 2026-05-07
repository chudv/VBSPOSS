using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VBSPOSS.ViewModels
{

    #region ---Model ListOfTransPointViewModel Master - Danh mục điểm giao dịch ---
    public class ListOfTransPointViewModel
    {
        public int OrderNo { get; set; }                        //Thêm

        public string OrderNoText { get; set; }                 //Thêm

        public string ProvinceCode { get; set; }

        public string ProvinceName { get; set; }

        public string PosCode { get; set; }

        public string PosName { get; set; }

        public string DistrictCode { get; set; }

        public string DistrictName { get; set; }

        public string CommuneCode { get; set; }

        public string CommuneName { get; set; }

        public string TxnPointCode { get; set; }

        public string TxnPointName { get; set; }

        public int VisitDate { get; set; }

        public string VisitDateText { get; set; }          //Thêm

        public string Times { get; set; }

        public string TimeBegin { get; set; }

        public string TimeEnd { get; set; }

        public decimal TimeBeginNum { get; set; }

        public decimal TimeEndNum { get; set; }

        public decimal Hours { get; set; }

        public decimal Minutes { get; set; }

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }

        public string IsInCommune { get; set; }

        public string IsInPos { get; set; }

        public string IsInterWard { get; set; }

        public string InterWardName { get; set; }

        public DateTime EffectiveDate { get; set; }

        public string EffectiveDateText { get; set; }     //Thêm

        public string TxnLocation { get; set; }

        public string AddressDetail { get; set; }

        public string AddressCode { get; set; }

        public string AddressFull { get; set; }

        public string PhoneSupport { get; set; }

        public string PhoneSupport01 { get; set; }

        public string PhoneSupport02 { get; set; }

        /// <summary>
        /// Trạng thái. Giá trị quy ước: 'A' - Mở; 'C' - Đóng;
        /// </summary>
        public string TxnStatus { get; set; }

        public string TxnStatusText { get; set; }                   //Thêm

        public int Status { get; set; }

        public string StatusText { get; set; }                      //Thêm

        public string Remark { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ApproverBy { get; set; }

        public DateTime ApprovalDate { get; set; }

        public DateTime BusinessDate { get; set; }

        public long DocumentId { get; set; }

        public int StatusUpdateCore { get; set; }

        public string CallApiTxnStatus { get; set; }

        public int CallApiResRecords { get; set; }

        public string CallApiResponseCode { get; set; }

        public string CallApiResponseMsg { get; set; }

        public int UserNumer { get; set; }      //Thêm
        
        public string VisitDateD6 { get; set; } //Thêm
    }
    #endregion

    #region ---Model ListOfTransPointHistViewModel - Lịch sử thay đổi điểm giao dịch ---
    public class ListOfTransPointHistViewModel
    {
        public int OrderNo { get; set; }                        //Thêm

        public string OrderNoText { get; set; }                 //Thêm

        public string EventCode { get; set; }

        public string EventName { get; set; }           //Thêm
        
        public long Id { get; set; }

        public DateTime DateSync { get; set; }

        public string ProvinceCode { get; set; }

        public string ProvinceName { get; set; }

        public string PosCode { get; set; }

        public string PosName { get; set; }

        public string DistrictCode { get; set; }

        public string DistrictName { get; set; }

        public string CommuneCode { get; set; }

        public string CommuneName { get; set; }

        public string TxnPointCode { get; set; }

        public string TxnPointName { get; set; }

        public int VisitDate { get; set; }

        public string VisitDateText { get; set; }              //Thêm

        public string Times { get; set; }

        public string TimeBegin { get; set; }

        public string TimeEnd { get; set; }

        public decimal TimeBeginNum { get; set; }

        public decimal TimeEndNum { get; set; }

        public decimal Hours { get; set; }

        public decimal Minutes { get; set; }

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }

        public string IsInCommune { get; set; }

        public string IsInPos { get; set; }

        public string IsInterWard { get; set; }

        public string InterWardName { get; set; }

        public DateTime EffectiveDate { get; set; }
        
        public string EffectiveDateText { get; set; }     //Thêm

        public string TxnLocation { get; set; }

        public string AddressDetail { get; set; }

        public string AddressCode { get; set; }

        public string AddressFull { get; set; }

        public string PhoneSupport { get; set; }

        public string PhoneSupport01 { get; set; }

        public string PhoneSupport02 { get; set; }

        /// <summary>
        /// Trạng thái. Giá trị quy ước: 'A' - Mở; 'C' - Đóng;
        /// </summary>
        public string TxnStatus { get; set; }
        
        public string TxnStatusText { get; set; }           //Thêm

        public int Status { get; set; }

        public string StatusText { get; set; }                     //Thêm

        public string Remark { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ApproverBy { get; set; }

        public DateTime ApprovalDate { get; set; }

        public DateTime BusinessDate { get; set; }
        
        public string BusinessDateText { get; set; }      //Thêm
        
        public long DocumentId { get; set; }

        public int StatusUpdateCore { get; set; }

        public string CallApiTxnStatus { get; set; }

        public int CallApiResRecords { get; set; }

        public string CallApiResponseCode { get; set; }

        public string CallApiResponseMsg { get; set; }
    }
    #endregion

    #region ---Model ListOfTransPointWorkViewModel - Thông tin thêm mới/thay đổi thông tin điểm giao dịch ---
    public class ListOfTransPointWorkViewModel
    {
        public int OrderNo { get; set; }                        //Thêm

        public string OrderNoText { get; set; }                 //Thêm

        public string EventCode { get; set; }

        public string EventName { get; set; }                   //Thêm
        
        public long ParentId { get; set; }

        public string ProvinceCode { get; set; }

        public string ProvinceName { get; set; }

        public string PosCode { get; set; }

        public string PosName { get; set; }

        public string DistrictCode { get; set; }

        public string DistrictName { get; set; }

        public string CommuneCode { get; set; }

        public string CommuneName { get; set; }

        public string TxnPointCode { get; set; }

        public string TxnPointName { get; set; }

        public int VisitDate { get; set; }

        public string VisitDateText { get; set; }               //Thêm

        public string Times { get; set; }

        public string TimeBegin { get; set; }

        public string TimeEnd { get; set; }

        public decimal TimeBeginNum { get; set; }

        public decimal TimeEndNum { get; set; }

        public decimal Hours { get; set; }

        public decimal Minutes { get; set; }

        public decimal Longitude { get; set; }

        public decimal Latitude { get; set; }

        public string IsInCommune { get; set; }

        public string IsInPos { get; set; }

        public string IsInterWard { get; set; }

        public string InterWardName { get; set; }

        public DateTime EffectiveDate { get; set; }
        
        public string EffectiveDateText { get; set; }     //Thêm

        public string TxnLocation { get; set; }

        public string AddressDetail { get; set; }

        public string AddressCode { get; set; }

        public string AddressFull { get; set; }

        public string PhoneSupport { get; set; }

        public string PhoneSupport01 { get; set; }

        public string PhoneSupport02 { get; set; }

        /// <summary>
        /// Trạng thái. Giá trị quy ước: 'A' - Mở; 'C' - Đóng;
        /// </summary>
        public string TxnStatus { get; set; }

        public string TxnStatusText { get; set; }           //Thêm

        public int Status { get; set; }

        public string StatusText { get; set; }              //Thêm

        public string Remark { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string ApproverBy { get; set; }

        public DateTime ApprovalDate { get; set; }

        public DateTime BusinessDate { get; set; }
        
        public string BusinessDateText { get; set; }          //Thêm

        public long DocumentId { get; set; }

        public int StatusUpdateCore { get; set; }
        
        public string CallApiTxnStatus { get; set; }

        public int CallApiResRecords { get; set; }
        
        public string CallApiResponseCode { get; set; }
        
        public string CallApiResponseMsg { get; set; }

        //Thông tin trước đó (Trước khi thực hiện thêm/thay đổi thông tin)
        public string ProvinceCodeOldInfo { get; set; }

        public string ProvinceNameOldInfo { get; set; }

        public string PosCodeOldInfo { get; set; }

        public string PosNameOldInfo { get; set; }

        public string DistrictCodeOldInfo { get; set; }

        public string DistrictNameOldInfo { get; set; }

        public string CommuneCodeOldInfo { get; set; }

        public string CommuneNameOldInfo { get; set; }

        public string TxnPointCodeOldInfo { get; set; }

        public string TxnPointNameOldInfo { get; set; }

        public int VisitDateOldInfo { get; set; }

        public string VisitDateTextOldInfo { get; set; }               //Thêm

        public string TimesOldInfo { get; set; }

        public string TimeBeginOldInfo { get; set; }

        public string TimeEndOldInfo { get; set; }

        public decimal TimeBeginNumOldInfo { get; set; }

        public decimal TimeEndNumOldInfo { get; set; }

        public decimal HoursOldInfo { get; set; }

        public decimal MinutesOldInfo { get; set; }

        public decimal LongitudeOldInfo { get; set; }

        public decimal LatitudeOldInfo { get; set; }

        public string IsInCommuneOldInfo { get; set; }

        public string IsInPosOldInfo { get; set; }

        public string IsInterWardOldInfo { get; set; }

        public string InterWardNameOldInfo { get; set; }

        public DateTime EffectiveDateOldInfo { get; set; }

        public string TxnLocationOldInfo { get; set; }

        public string AddressDetailOldInfo { get; set; }

        public string AddressCodeOldInfo { get; set; }

        public string AddressFullOldInfo { get; set; }

        public string PhoneSupportOldInfo { get; set; }

        public string PhoneSupport01OldInfo { get; set; }

        public string PhoneSupport02OldInfo { get; set; }

        /// <summary>
        /// Trạng thái. Giá trị quy ước: 'A' - Mở; 'C' - Đóng;
        /// </summary>
        public string TxnStatusOldInfo { get; set; }

        public string TxnStatusTextOldInfo { get; set; }           //Thêm

        public int StatusOldInfo { get; set; }

        public string StatusTextOldInfo { get; set; }              //Thêm

        public string RemarkOldInfo { get; set; }

        public string CreatedByOldInfo { get; set; }

        public DateTime CreatedDateOldInfo { get; set; }

        public string ModifiedByOldInfo { get; set; }

        public DateTime ModifiedDateOldInfo { get; set; }

        public string ApproverByOldInfo { get; set; }

        public DateTime ApprovalDateOldInfo { get; set; }

        public DateTime BusinessDateOldInfo { get; set; }

        public string BusinessDateTextOldInfo { get; set; }          //Thêm

        public long DocumentIdOldInfo { get; set; }
    }
    #endregion
}
