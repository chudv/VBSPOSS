using VBSPOSS.Integration.Model;

namespace VBSPOSS.Integration.Interfaces
{
    public interface IApiReportGateway
    {
        /// <summary>
        /// Hàm gọi sang ReportGateway để in báo cáo
        /// </summary>
        /// <param name="inputModel"></param>
        /// <returns></returns>
        GenericResultReportGateway<ReportResultDto> GetReport(ReportInput inputModel);
    }
}
