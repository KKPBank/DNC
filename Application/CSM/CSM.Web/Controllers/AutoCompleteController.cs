﻿using CSM.Business;
using CSM.Common.Utilities;
using CSM.Entity;
using CSM.Web.Controllers.Common;
using CSM.Web.Filters;
using CSM.Web.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CSM.Web.Controllers
{
    [CheckUserSession]
    public class AutoCompleteController : BaseController
    {
        private const int AutoCompleteMaxResult = 10;

        private IUserFacade _userFacade;
        private IBranchFacade _branchFacade;
        private IServiceRequestFacade _srFacade;
        private LogMessageBuilder _logMsg = new LogMessageBuilder();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AutoCompleteController));

        [HttpPost]
        public ActionResult AutoCompleteSearchBranch(string keyword)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Branch").Add("Keyword", keyword).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<BranchEntity> result = _srFacade.AutoCompleteSearchBranch(keyword, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.BranchId,
                    r.BranchName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Branch").ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchBranchByBranchIds(string keyword, string branchIds)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Branch by BranchIds").Add("Keyword", keyword)
                .Add("BranchIds", branchIds).ToInputLogString());

            try
            {
                _branchFacade = new BranchFacade();
                List<BranchEntity> result = _branchFacade.GetBranchByBranchIds(keyword, branchIds.Split(',').Select(Int32.Parse).ToList(), AutoCompleteMaxResult);

                return Json(result.Select(r => new
                {
                    r.BranchId,
                    r.BranchName
                }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Branch by BranchIds").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchUserByUserIds(string keyword, int branchId, string userIds)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search User by UserIds").Add("Keyword", keyword)
                .Add("BranchId", branchId).Add("UserIds", userIds).ToInputLogString());

            try
            {
                _userFacade = new UserFacade();
                List<UserEntity> result = _userFacade.AutoCompleteSearchUserByUserIds(keyword, branchId, userIds.Split(',').Select(Int32.Parse).ToList(), AutoCompleteMaxResult);

                return Json(result.Select(r => new
                {
                    r.UserId,
                    r.FullName
                }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search User by UserIds").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchUser(string keyword, int? branchId)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search User").Add("Keyword", keyword).Add("BranchId", branchId).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<UserEntity> result = _srFacade.AutoCompleteSearchUser(keyword, branchId, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.UserId,
                    UserDisplayName = r.FullName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search User").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchUserWithJobOnHand(string keyword, int branchId)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search User with Job On Hand").Add("Keyword", keyword)
                .Add("BranchId", branchId).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<UserEntity> result = _srFacade.AutoCompleteSearchUserWithJobOnHand(keyword, branchId, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.UserId,
                    UserDisplayName = r.FullName + " (" + (r.JobOnHand ?? 0) + ")"
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search User with Job On Hand").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchProductGroup(string keyword, int? productId, int? campaignServiceId, bool? isAllStatus)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search ProductGroup").Add("Keyword", keyword)
                .Add("ProductId", productId).Add("CampaignServiceId", campaignServiceId).Add("IsAllStatus", isAllStatus).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<ProductGroupEntity> result = _srFacade.AutoCompleteSearchProductGroup(keyword, AutoCompleteMaxResult, productId, isAllStatus);
                return Json(result.Select(r => new
                {
                    r.ProductGroupId,
                    r.ProductGroupName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search ProductGroup").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchProductWithExceptions(AutoCompleteSearchProductInputModel model)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Product")
                                        .Add("Keyword", model.Keyword)
                                        .Add("ExceptionIds", string.Join(", ", model.ExceptProductIds)));
            try
            {
                _srFacade = new ServiceRequestFacade();
                List<ProductEntity> result = _srFacade.AutoCompleteSearchProduct(model.Keyword, model.ExceptProductIds, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.ProductId,
                    r.ProductName,
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Product").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchProduct(string keyword, int? productGroupId, int? campaignServiceId, bool? isAllStatus)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Product").Add("Keyword", keyword)
                .Add("ProductGroupId", productGroupId).Add("CampaignServiceId", campaignServiceId).Add("IsAllStatus", isAllStatus).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<ProductEntity> result = _srFacade.AutoCompleteSearchProduct(keyword, productGroupId, AutoCompleteMaxResult, campaignServiceId, isAllStatus);
                return Json(result.Select(r => new
                {
                    r.ProductId,
                    r.ProductName,
                    r.ProductGroupId,
                    r.ProductGroupName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Product").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchProductForQuestionGroup(string keyword, int? productGroupId, int? campaignServiceId, bool? isAllStatus)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Product for QuestionGroup").Add("Keyword", keyword)
                .Add("ProductGroupId", productGroupId).Add("CampaignServiceId", campaignServiceId).Add("IsAllStatus", isAllStatus).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<ProductEntity> result = _srFacade.AutoCompleteSearchProductForQuestionGroup(keyword, productGroupId, AutoCompleteMaxResult, campaignServiceId);
                return Json(result.Select(r => new
                {
                    r.ProductId,
                    r.ProductName,
                    r.ProductGroupName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Product for QuestionGroup").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchCampaignService(string keyword, int? productGroupId, int? productId, bool? isAllStatus)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Campaign/Service").Add("Keyword", keyword)
                .Add("ProductGroupId", productGroupId).Add("ProductId", productId).Add("IsAllStatus", isAllStatus).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<CampaignServiceEntity> result = _srFacade.AutoCompleteSearchCampaignService(keyword, productGroupId, productId, AutoCompleteMaxResult, isAllStatus);
                return Json(result.Select(r => new
                {
                    r.CampaignServiceId,
                    r.CampaignServiceName
                }));
            }
            catch (Exception ex)
            {
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Campaign/Service").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchCampaignServiceOnMapping(string keyword, int? areaId, int? subAreaId, int? typeId)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Campaign/Service on Mapping").Add("Keyword", keyword)
                .Add("AreaId", areaId).Add("SubAreaId", subAreaId).Add("TypeId", typeId).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<CampaignServiceEntity> result = _srFacade.AutoCompleteSearchCampaignServiceOnMapping(keyword, areaId, subAreaId, typeId);
                return Json(result.Select(r => new
                {
                    r.CampaignServiceId,
                    r.CampaignServiceName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Campaign/Service on Mapping").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchArea(string keyword, int? subAreaId, bool? isAllStatus)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Area").Add("Keyword", keyword).Add("SubAreaId", subAreaId)
                .Add("IsAllStatus", isAllStatus).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<AreaItemEntity> result = _srFacade.AutoCompleteSearchArea(keyword, subAreaId, AutoCompleteMaxResult, isAllStatus);
                return Json(result.Select(r => new
                {
                    r.AreaId,
                    r.AreaName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Area").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchAreaOnMapping(string keyword, int? campaignServiceId, int? subAreaId, int? typeId)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Area (on Mapping)").Add("Keyword", keyword)
                .Add("CampaignServiceId", campaignServiceId).Add("SubAreaId", subAreaId).Add("TypeId", typeId).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<AreaItemEntity> result = _srFacade.AutoCompleteSearchAreaOnMapping(keyword, campaignServiceId, subAreaId, typeId, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.AreaId,
                    r.AreaName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Area (on Mapping)").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchSubArea(string keyword, int? areaId, bool? isAllStatus)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search SubArea").Add("Keyword", keyword)
                .Add("AreaId", areaId).Add("IsAllStatus", isAllStatus).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<SubAreaItemEntity> result = _srFacade.AutoCompleteSearchSubArea(keyword, areaId, AutoCompleteMaxResult, isAllStatus);
                return Json(result.Select(r => new
                {
                    r.SubAreaId,
                    r.SubAreaName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search SubArea").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchSubAreaOnMapping(string keyword, int? campaignServiceId, int? areaId, int? typeId)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search SubArea (on Mapping)").Add("Keyword", keyword)
                .Add("CampaignServiceId", campaignServiceId).Add("AreaId", areaId).Add("TypeId", typeId).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<SubAreaItemEntity> result = _srFacade.AutoCompleteSearchSubAreaOnMapping(keyword, campaignServiceId, areaId, typeId, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.SubAreaId,
                    r.SubAreaName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search SubArea (on Mapping)").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchType(string keyword, bool? isAllStatus)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Type").Add("Keyword", keyword).Add("IsAllStatus", isAllStatus).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<TypeItemEntity> result = _srFacade.AutoCompleteSearchType(keyword, AutoCompleteMaxResult, isAllStatus);
                return Json(result.Select(r => new
                {
                    r.TypeId,
                    r.TypeName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Type").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        public ActionResult AutoCompleteSearchTypeOnMapping(string keyword, int? campaignServiceId, int? areaId, int? subAreaId)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Type (on Mapping)").Add("Keyword", keyword)
                .Add("CampaignServiceId", campaignServiceId).Add("AreaId", areaId).Add("SubAreaId", subAreaId).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<TypeItemEntity> result = _srFacade.AutoCompleteSearchTypeOnMapping(keyword, campaignServiceId, areaId, subAreaId, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.TypeId,
                    r.TypeName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Type (on Mapping)").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        [OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult AutoCompleteSearchChannel()
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Channel").ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<ChannelEntity> result = _srFacade.AutoCompleteSearchChannel();
                return Json(result.Select(r => new 
                {
                    r.ChannelId,
                    r.Name
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search Channel").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }

        [HttpPost]
        [OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult AutoCompleteSearchSrStatus()
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search SR Status").ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<SRStatusEntity> result = _srFacade.AutoCompleteSearchSrStatus();
                return Json(result.Select(r => new 
                {
                    r.SRStatusId,
                    r.SRStatusName
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search SR Status").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }
        [HttpPost]
        public ActionResult AutoCompleteSearchAfsAsset(string keyword)
        {
            Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search AFSAsset").Add("Keyword", keyword).ToInputLogString());

            try
            {
                _srFacade = new ServiceRequestFacade();
                List<AfsAssetEntity> result = _srFacade.AutoCompleteSearchAfsAsset(keyword, AutoCompleteMaxResult);
                return Json(result.Select(r => new
                {
                    r.AssetId,
                    r.AssetNo
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occur:\n", ex);
                Logger.Info(_logMsg.Clear().SetPrefixMsg("Auto Complete :: Search AFSAsset").Add("Error Message", ex.Message).ToFailLogString());
                return Error(new HandleErrorInfo(ex, this.ControllerContext.RouteData.Values["controller"].ToString(),
                    this.ControllerContext.RouteData.Values["action"].ToString()));
            }
        }
    }
}