using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VBSPOSS.Constants;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Controllers
{

    
    public class MenuController : BaseController
    {
        //private IAdministrationService _administrationService;
        public MenuController(ILogger<MenuController> logger, IAdministrationService administrationService, ISessionHelper sessionHelper) : base(logger, administrationService, sessionHelper)
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
        /// <summary>
        /// Tải dữ liệu lên lưới
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult LoadData([DataSourceRequest] DataSourceRequest request)
        {
            var list = _administrationService.GetMenus();
            return Json(list.ToDataSourceResult(request));
        }

        [Authorize]
        [AcceptVerbs("Post")]
        public ActionResult Create([DataSourceRequest] DataSourceRequest request, Data.Models.Menu model)
        {
            try
            {
                if (model != null && ModelState.IsValid)
                {
                    if (model.Id == 0)
                    {
                        model.Id = _administrationService.CreateMenu(model);
                    }
                    else
                    {
                        model.Id = _administrationService.UpdateMenu(model);
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
        public ActionResult Edit([DataSourceRequest] DataSourceRequest request, Data.Models.Menu model)
        {
            try
            {
                if (model != null && ModelState.IsValid)
                {
                    int result = _administrationService.UpdateMenu(model);
                    if (result == 0)
                    {
                        ModelState.AddModelError("ERROR", "Cập nhật chức năng thất bại.");
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
        public ActionResult Delete([DataSourceRequest] DataSourceRequest request, Data.Models.Menu model)
        {
            try
            {
                if (model != null && ModelState.IsValid)
                {
                    int result = _administrationService.DeleteMenu(model);
                    if (result == 0)
                    {
                        ModelState.AddModelError("ERROR", "Xóa chức năng thất bại.");
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

    }
}
