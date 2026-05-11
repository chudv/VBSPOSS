using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{
    public class ScriptExecutionController : BaseController
    {
        public ScriptExecutionController(ILogger<BaseController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper, IScriptExecutionService scriptExecutionService) : base(logger, administrationService, sessionHelper)
        {
            _scriptExecutionService = scriptExecutionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        private readonly IScriptExecutionService _scriptExecutionService;               

        [HttpPost]
        public async Task<IActionResult> LoadScriptQueue(
            [DataSourceRequest] DataSourceRequest request,
            string moduleCode,
            int? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var data =
                await _scriptExecutionService
                    .LoadScriptQueue(
                        moduleCode,
                        status,
                        fromDate,
                        toDate);

            return Json(
                await data.ToDataSourceResultAsync(request));
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteScripts(List<long> ids)
        {
            var result =
                await _scriptExecutionService
                    .ExecuteScripts(
                        ids,
                        User.Identity.Name);

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> RetryScript(
            long id)
        {
            var result =
                await _scriptExecutionService
                    .RetryScript(
                        id,
                        User.Identity.Name);

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> CancelScript(
            long id)
        {
            var result =
                await _scriptExecutionService
                    .CancelScript(
                        id,
                        User.Identity.Name);

            return Json(result);
        }

        public async Task<IActionResult> ViewLog(
            long queueId)
        {
            var model =
                await _scriptExecutionService
                    .GetExecutionLogs(queueId);

            return PartialView(
                "_ViewLog",
                model);
        }


        [HttpGet]
        public async Task<IActionResult> ScriptDetail(
            long id)
        {
            var model = await _scriptExecutionService.GetScriptDetail(id);

            return PartialView(
                "_ScriptDetail",
                model);
        }
    }
}
