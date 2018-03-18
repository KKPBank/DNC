﻿using System.Globalization;
using System.Web;
using System.Web.Mvc;
using CSM.Business;
using CSM.Common.Resources;
using CSM.Common.Utilities;
using CSM.Entity;
using CSM.Web.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSM.Web.Controllers.Common
{
    public class BaseController : Controller
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BaseController));

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.CallId = !string.IsNullOrEmpty(this.CallId) ? this.CallId : Constants.NotKnown;
            ViewBag.PhoneNo = !string.IsNullOrEmpty(this.PhoneNo) ? this.PhoneNo : Constants.NotKnown;
            base.OnActionExecuting(filterContext);
        }

        protected UserEntity UserInfo
        {
            get
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    UserIdentity id = HttpContext.User.Identity as UserIdentity;
                    return id.UserInfo;
                }

                return null;
            }
        }

        protected string CallId
        {
            get { return this.ControllerContext.RouteData.Values["callId"].ConvertToString(); }
        }

        protected string PhoneNo
        {
            get { return this.ControllerContext.RouteData.Values["phoneNo"].ConvertToString(); }
        }

        public Dictionary<string, object> GetModelValidationErrors(bool removePrefix = false)
        {
            var errors = new Dictionary<string, object>();
            if (!removePrefix)
            {
                foreach (var key in ModelState.Keys)
                {
                    if (ModelState[key].Errors.Count > 0)
                        errors[key] = string.IsNullOrWhiteSpace(ModelState[key].Errors[0].ErrorMessage) ? ModelState[key].Errors[0].Exception.InnerException.Message : ModelState[key].Errors[0].ErrorMessage;
                }
            }
            else
            {
                foreach (var key in ModelState.Keys)
                {
                    string newKey = key;
                    if (key.Contains("."))
                    {
                        newKey = key.Split('.').Last();
                    }

                    if (ModelState[key].Errors.Count > 0)
                        errors[newKey] = string.IsNullOrWhiteSpace(ModelState[key].Errors[0].ErrorMessage) ? ModelState[key].Errors[0].Exception.InnerException.Message : ModelState[key].Errors[0].ErrorMessage;
                }
            }

            return errors;
        }

        public ActionResult Error(HandleErrorInfo handleError)
        {
            if (handleError != null)
            {
                var controllerName = handleError.ControllerName;
                var actionName = handleError.ActionName;
                Logger.Error("Exception occur: Controller Name-" + controllerName + " Action Name-" + actionName + "\n", handleError.Exception);
            }

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    Valid = false,
                    Error = Resource.Error_System
                });
            }

            return View("~/Views/Shared/Error.cshtml", handleError);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            try
            {
                if (filterContext.ExceptionHandled)
                {
                    return;
                }

                filterContext.ExceptionHandled = true;
                Exception exception = filterContext.Exception;

                var controllerName = (string)filterContext.RouteData.Values["controller"];
                var actionName = (string)filterContext.RouteData.Values["action"];
                var model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);

                if (Request.IsAjaxRequest())
                {
                    filterContext.Result = Json(new
                    {
                        Valid = false,
                        Error = Resource.Error_System
                    });
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "~/Views/Shared/Error.cshtml",
                        ViewData = new ViewDataDictionary(model),
                        TempData = filterContext.Controller.TempData
                    };
                }

                Logger.Error("Exception occur:\n", exception);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
            }
        }

        protected void ClearSession()
        {
            Session["sessionid"] = null;

            // Reset routedata
            ClearCallId();

            // Clear cache
            var username = HttpContext.User != null ? HttpContext.User.Identity.Name : string.Empty;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_user_info", username);
                if (HttpRuntime.Cache[cacheKey] != null) HttpRuntime.Cache.Remove(cacheKey);
            }

            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
        }

        protected void ClearCallId()
        {
            if (RouteData.Values.ContainsKey("callId")) RouteData.Values.Remove("callId");
            if (RouteData.Values.ContainsKey("phoneNo")) RouteData.Values.Remove("phoneNo");
        }

        protected CustomerInfoViewModel MappingCustomerInfoView(int customerId)
        {
            using (var customerFacade = new CustomerFacade())
            {
                // CustomerInfo
                CustomerEntity customerEntity = customerFacade.GetCustomerByID(customerId);
                CustomerInfoViewModel custInfoVM = new CustomerInfoViewModel();
                custInfoVM.Account = customerEntity.Account;
                custInfoVM.AccountNo = customerEntity.AccountNo;
                custInfoVM.BirthDate = customerEntity.BirthDate;
                custInfoVM.CardNo = customerEntity.CardNo;
                custInfoVM.CreateUser = customerEntity.CreateUser;
                custInfoVM.CustomerId = customerEntity.CustomerId;
                custInfoVM.CustomerType = customerEntity.CustomerType;
                custInfoVM.Email = customerEntity.Email;
                custInfoVM.Fax = customerEntity.Fax;
                custInfoVM.FirstNameEnglish = customerEntity.FirstNameEnglish;
                custInfoVM.FirstNameThai = customerEntity.FirstNameThai;
                custInfoVM.LastNameEnglish = customerEntity.LastNameEnglish;
                custInfoVM.LastNameThai = customerEntity.LastNameThai;
                custInfoVM.FirstNameThaiEng = customerEntity.FirstNameThaiEng;
                custInfoVM.LastNameThaiEng = customerEntity.LastNameThaiEng;
                custInfoVM.PhoneList = customerEntity.PhoneList;
                custInfoVM.Registration = customerEntity.Registration;
                custInfoVM.SubscriptType = customerEntity.SubscriptType;
                custInfoVM.TitleEnglish = customerEntity.TitleEnglish;
                custInfoVM.TitleThai = customerEntity.TitleThai;
                custInfoVM.UpdateUser = customerEntity.UpdateUser;
                return custInfoVM;
            }
        }
    }
}
