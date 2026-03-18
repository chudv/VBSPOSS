using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VBSPOSS.Constants;
using VBSPOSS.Data.Models;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{
    
    public class RoleController : BaseController
    {
        //private IAdministrationService _administrationService;

        public RoleController(ILogger<BaseController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper)
        {
            //_administrationService = administrationService;
        }

        [Authorize]
        [AuthorizeFilter]
        public IActionResult Index()
        {
            List<ValueConstModel> lstStatus = new List<ValueConstModel>();
            lstStatus.Add(StatusValue.CLOSED);
            lstStatus.Add(StatusValue.ACTIVE);
            ViewData["lstStatus"] = lstStatus;
            return View();
        }

        [Authorize]
        public ActionResult LoadData([DataSourceRequest] DataSourceRequest request)
        {
            var list = _administrationService.GetRoles(0, UserGrade);
            return Json(list.ToDataSourceResult(request));
        }

        [Authorize]
        [AcceptVerbs("Post")]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, Role model)
        {
            if (model != null && ModelState.IsValid)
            {
                try
                {
                    if (model.Id == 0)
                    {
                        model.Status = StatusValue.ACTIVE.Value;
                        model.Id = _administrationService.CreateUserGroup(model);
                    }
                    else
                    {
                        model.Status = StatusValue.CLOSED.Value;
                        model.Id = _administrationService.UpdateUserGroup(model);
                    }
                }
                catch (Exception e)
                {
                    WriteLog(LogType.ERROR, e.Message);
                    ModelState.AddModelError("ERROR", e.Message);
                }

            }
            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }


        [Authorize]
        [AcceptVerbs("Post")]
        public ActionResult Edit([DataSourceRequest] DataSourceRequest request, Role model)
        {
            try
            {
                if (model != null && ModelState.IsValid)
                {
                    var result = _administrationService.UpdateUserGroup(model);
                    if (result == 0)
                    {
                        ModelState.AddModelError("ERROR", "Cập nhật dữ liệu thất bại.");
                    }
                }
                return Json(new[] { model }.ToDataSourceResult(request, ModelState));
            }
            catch (Exception e)
            {
                WriteLog(LogType.ERROR, e.Message);
                return View();
            }
        }


        [Authorize]
        [AcceptVerbs("Post")]
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, Role model)
        {
            if (model != null && ModelState.IsValid)
            {
                try
                {
                    var result = _administrationService.DeleteUserGroup(model);
                    if (result == 0)
                    {
                        ModelState.AddModelError("ERROR", "Xóa dữ liệu thất bại.");
                    }
                }
                catch (Exception e)
                {
                    WriteLog(LogType.ERROR, e.Message);
                    ModelState.AddModelError("ERROR", e.Message);
                }

            }
            return Json(new[] { model }.ToDataSourceResult(request, ModelState));
        }
    }
}
