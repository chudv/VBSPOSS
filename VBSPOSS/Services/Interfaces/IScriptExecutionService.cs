using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Models;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Interfaces
{
    public interface IScriptExecutionService
    {
        Task<List<ScriptExecutionViewModel>>
            LoadScriptQueue(
                string moduleCode,
                int? status,
                DateTime? fromDate,
                DateTime? toDate);

        Task<ServiceResult>
            ExecuteScripts(
                List<long> ids,
                string executedBy);

        Task<ServiceResult>
            RetryScript(
                long id,
                string executedBy);

        Task<ServiceResult>
            CancelScript(
                long id,
                string executedBy);

        Task<List<ScriptExecutionLog>>
            GetExecutionLogs(long queueId);

        Task<ScriptExecutionQueue>
            GetScriptDetail(long id);


        /// <summary>
        /// Hàm để các module khác gọi khi muốn đẩy 1 script vào queue để thực thi. Hệ thống sẽ tự động chạy theo lịch hoặc chạy ngay nếu effective date đã tới.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ServiceResult>PushScriptToQueue(ScriptExecutionRequest request);
    }
}
