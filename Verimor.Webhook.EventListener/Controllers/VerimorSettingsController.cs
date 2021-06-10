using CallCenter.Operation;
using CallCenter.Operation.Models;
using RadiusR.Verimor.CallCenter.Caching;
using RezaB.Data.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class VerimorSettingsController : BaseController
    {
        // GET: VerimorSettings
        RadiusR_NetSpeed_5Entities db = new RadiusR_NetSpeed_5Entities();
        public ActionResult CreateFlowChart()
        {
            return View();
        }
        public ActionResult FlowChart()
        {
            var Operations = db.VerimorOperations.ToList();
            ViewBag.Operations = new SelectList(Operations.OrderBy(opr => opr.Title).Select(opr => new { Value = opr.ID, Text = opr.Title }).ToArray(), "Value", "Text");
            ViewBag.OperationDescriptions = new SelectList(Operations.Select(opr => new { Value = opr.ID, Text = opr.phrase }).ToArray(), "Value", "Text");
            var OperationResponse = db.VerimorOperationResponses.ToArray();
            List<ViewModels.VerimorFlowChartViewModel> flowChartList = new List<ViewModels.VerimorFlowChartViewModel>();
            foreach (var item in OperationResponse)
            {
                if (!flowChartList.Where(fc => fc.ParentId == item.ParentID).Any())
                {
                    flowChartList.Add(new ViewModels.VerimorFlowChartViewModel()
                    {
                        ParentId = item.ParentID,
                        FlowChartItemList = OperationResponse.Where(or => or.ParentID == item.ParentID).Select(or => new ViewModels.VerimorFlowChartViewModel.FlowChartItems()
                        {
                            Digit = or.digit,
                            OperationId = or.OperationID
                        }),
                    });
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult DrawDiagram()
        {
            return Json(new { OperationResponseList = db.VerimorOperationResponses.Select(v => new { operationType = v.VerimorOperation.operationType, parentId = v.ParentID, operationId = v.OperationID, digit = v.digit }).ToArray() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveDiagram(int? startedOperationId, int? parentId, string digit, int? operationId, string content)
        {

            if (startedOperationId != null)
            {
                db.VerimorOperationResponses.Add(new VerimorOperationResponse()
                { 
                    digit = null,
                    OperationID = startedOperationId.Value,
                    ParentID = null,
                });
                db.SaveChanges();
            }
            else
            {
                db.VerimorOperationResponses.Add(new VerimorOperationResponse()
                {
                    digit = string.IsNullOrEmpty(digit) ? null : digit,
                    OperationID = operationId.Value,
                    ParentID = parentId.Value
                });
                db.SaveChanges();
            }
            return Json(new { code = 0 }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetOperation(int operationId)
        {
            var operation = db.VerimorOperations.Find(operationId);
            return Json(new { operationType = operation.operationType }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ClearDiagram()
        {
            db.VerimorOperationResponses.RemoveRange(db.VerimorOperationResponses.ToArray());
            db.SaveChanges();
            return Json(new { code = 0 }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            var Operations = db.VerimorOperations.ToList();
            var OperationResponse = db.VerimorOperationResponses.ToList();
            List<ViewModels.VerimorSettings> settings = new List<ViewModels.VerimorSettings>();
            var OpList = Operations.OrderBy(m => m.phrase).Select(m => new SelectListItem
            {
                Text = m.phrase + (string.IsNullOrEmpty(m.target) ? "" : " (" + m.target + ")"),
                Value = m.ID.ToString()
            });
            var DigitList = new List<SelectListItem>();
            for (int i = 0; i < 12; i++)
            {
                var Text = i.ToString();
                if (i == 10)
                {
                    Text = "*";
                }
                if (i == 11)
                {
                    Text = "#";
                }
                DigitList.Add(new SelectListItem
                {
                    Text = Text,
                    Value = Text
                });
            }
            foreach (var item in OperationResponse)
            {
                ViewModels.VerimorSettings verimor = new ViewModels.VerimorSettings
                {
                    Digit = item.digit,
                    SelectedParent = item.ParentID,
                    SelectedChild = item.OperationID,
                    ID = item.ID
                };
                settings.Add(verimor);
            }

            ViewBag.List = OpList;
            ViewBag.DigitList = DigitList;
            return View(settings);
        }
        public ActionResult OperationList()
        {
            var operations = db.VerimorOperations.Select(v => new ViewModels.VerimorOperationsViewModel()
            {
                Title = v.Title,
                OperationType = v.operationType,
                ID = v.ID
            });
            return View(operations.ToList());
        }
        public ActionResult CreateDiagram()
        {
            var WebHookType = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.VerimorWebHookTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
            ViewBag.WebHookTypes = WebHookType;
            var OperationTypes = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.VerimorOperationTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
            ViewBag.OperationTypes = OperationTypes;
            var Parameters = new LocalizedList<PhraseTypes, RadiusR.Verimor.CallCenter.Common>().GenericList;
            ViewBag.Parameters = new SelectList(Parameters.Select(p => new { Value = (PhraseTypes)p.ID, Text = p.Name }).ToArray(), "Value", "Text");
            var ConditionTypes = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.ConditionTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
            ViewBag.ConditionTypes = ConditionTypes;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDiagram(ViewModels.VerimorDiagramVM verimorDiagramVM)
        {
            try
            {
                var WebHookType = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.VerimorWebHookTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
                ViewBag.WebHookTypes = WebHookType;
                var OperationTypes = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.VerimorOperationTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
                ViewBag.OperationTypes = OperationTypes;
                var Parameters = new LocalizedList<PhraseTypes, RadiusR.Verimor.CallCenter.Common>().GenericList;
                ViewBag.Parameters = new SelectList(Parameters.Select(p => new { Value = (PhraseTypes)p.ID, Text = p.Name }).ToArray(), "Value", "Text");
                var ConditionTypes = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.ConditionTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
                ViewBag.ConditionTypes = ConditionTypes;
                int? ID = null;
                var Count = db.VerimorOperations.Count();
                for (int i = 1; i < Count + 1; i++)
                {
                    var CurrentID = db.VerimorOperations.Find(i);
                    if (CurrentID == null)
                    {
                        ID = i;
                        break;
                    }
                    else
                    {
                        ID = Count + 1;
                    }
                }
                VerimorOperation verimorOperation = new VerimorOperation();
                if (string.IsNullOrEmpty(verimorDiagramVM.Title))
                {
                    verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                    return View(verimorDiagramVM);
                }
                switch ((RadiusR.Verimor.CallCenter.Enums.VerimorWebHookTypes)verimorDiagramVM.WebHookType)
                {
                    case RadiusR.Verimor.CallCenter.Enums.VerimorWebHookTypes.Function:
                        {
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase) || verimorDiagramVM.OperationType == (int)RadiusR.Verimor.CallCenter.Enums.VerimorOperationTypes.Basic)
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (verimorDiagramVM.ConditionType != null && string.IsNullOrEmpty(verimorDiagramVM.ConditionParameters))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                Title = verimorDiagramVM.Title,
                                phrase = $"*{verimorDiagramVM.Phrase}",
                                operationType = verimorDiagramVM.OperationType,
                                ID = ID.Value,
                                Condition = verimorDiagramVM.ConditionType == null ? null : $"{verimorDiagramVM.ConditionType},{verimorDiagramVM.ConditionParameters}"
                            };
                        }
                        break;
                    case RadiusR.Verimor.CallCenter.Enums.VerimorWebHookTypes.Prompt:
                        {
                            if (string.IsNullOrEmpty(verimorDiagramVM.Min_Digits) || string.IsNullOrEmpty(verimorDiagramVM.Max_Digits))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                Title = verimorDiagramVM.Title,
                                phrase = verimorDiagramVM.Phrase,
                                max_digits = verimorDiagramVM.Max_Digits,
                                min_digits = verimorDiagramVM.Min_Digits,
                                operationType = (int)RadiusR.Verimor.CallCenter.Enums.VerimorOperationTypes.Basic,
                                ID = ID.Value
                            };
                        }
                        break;
                    case RadiusR.Verimor.CallCenter.Enums.VerimorWebHookTypes.Transfer:
                        {
                            if (string.IsNullOrEmpty(verimorDiagramVM.Target))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                Title = verimorDiagramVM.Title,
                                ID = ID.Value,
                                target = verimorDiagramVM.Target,
                                phrase = verimorDiagramVM.Phrase,
                                operationType = (int)RadiusR.Verimor.CallCenter.Enums.VerimorOperationTypes.Basic
                            };
                        }
                        break;
                    case RadiusR.Verimor.CallCenter.Enums.VerimorWebHookTypes.Record:
                        {
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                Title = verimorDiagramVM.Title,
                                ID = ID.Value,
                                phrase = verimorDiagramVM.Phrase,
                                operationType = (int)RadiusR.Verimor.CallCenter.Enums.VerimorOperationTypes.Basic
                            };
                        }
                        break;
                    default:
                        break;
                }
                db.VerimorOperations.Add(verimorOperation);
                db.SaveChanges();
                verimorDiagramVM.ErrorMessage = "Yeni Diyagram eklendi";
                return View(verimorDiagramVM);
            }
            catch (Exception ex)
            {
                verimorDiagramVM.ErrorMessage = ex.Message;
                return View(verimorDiagramVM);
            }
        }
        public ActionResult Delete(int? Parent, string Digit, int? Child)
        {
            try
            {
                Digit = Digit == "" ? null : Digit;
                var OperResponse = db.VerimorOperationResponses.Where(m => m.digit == Digit && m.ParentID == Parent && m.OperationID == Child).FirstOrDefault();
                if (OperResponse != null)
                {
                    db.VerimorOperationResponses.Remove(OperResponse);
                    db.SaveChanges();
                    return Json(new { ErrorMessage = "Operasyon Silindi" });
                }
                else
                {
                    return Json(new { ErrorMessage = "Böyle bir operasyon kayıtlı değil" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { ErrorMessage = "Operasyonu silerken bir hata oluştu : " + ex.Message });
            }
        }
        public ActionResult Edit(long ID)
        {
            var operation = db.VerimorOperations.Find(ID);
            if (operation == null)
            {
                return View(new ViewModels.VerimorDiagramVM());
            }
            var conditionValue = string.IsNullOrEmpty(operation.Condition) ? null : operation.Condition.Split(',')[0];
            short? currentConditionType = null;
            if (short.TryParse(conditionValue, out short conditionType))
            {
                currentConditionType = conditionType;
            }
            var ConditionTypes = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.ConditionTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text", currentConditionType);
            ViewBag.ConditionTypes = ConditionTypes;
            return View(new ViewModels.VerimorDiagramVM()
            {
                ID = operation.ID,
                Max_Digits = operation.max_digits,
                Min_Digits = operation.min_digits,
                Phrase = operation.phrase.Replace("*", ""),
                OperationType = operation.operationType,
                Title = operation.Title,
                Target = operation.target,
                ConditionType = currentConditionType,
                ConditionParameters = string.IsNullOrEmpty(operation.Condition) == false ? operation.Condition.Split(',')[1] : null
            });
        }
        [HttpPost]
        public ActionResult Edit(ViewModels.VerimorDiagramVM verimorDiagramVM)
        {
            try
            {
                var ConditionTypes = new SelectList(EnumHelper.GetSelectList(typeof(RadiusR.Verimor.CallCenter.Enums.ConditionTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
                ViewBag.ConditionTypes = ConditionTypes;
                if (verimorDiagramVM.OperationType == (int)RadiusR.Verimor.CallCenter.Enums.VerimorOperationTypes.Basic)
                {
                    var Operation = db.VerimorOperations.Find(verimorDiagramVM.ID);
                    if (Operation != null)
                    {
                        if (string.IsNullOrEmpty(verimorDiagramVM.Phrase))
                        {
                            ViewBag.ErrorMessage = "Geçersiz işlem";
                            return View(verimorDiagramVM);
                        }
                        if (!string.IsNullOrEmpty(Operation.target) && string.IsNullOrEmpty(verimorDiagramVM.Target))
                        {
                            ViewBag.ErrorMessage = "Geçersiz işlem";
                            return View(verimorDiagramVM);
                        }
                        if (string.IsNullOrEmpty(Operation.target) && (string.IsNullOrEmpty(verimorDiagramVM.Max_Digits) || string.IsNullOrEmpty(verimorDiagramVM.Min_Digits)))
                        {
                            ViewBag.ErrorMessage = "Geçersiz işlem";
                            return View(verimorDiagramVM);
                        }
                        Operation.Title = verimorDiagramVM.Title;
                        Operation.phrase = Operation.Title.StartsWith("*") ? $"*{verimorDiagramVM.Phrase}" : verimorDiagramVM.Phrase;
                        Operation.max_digits = verimorDiagramVM.Max_Digits;
                        Operation.min_digits = verimorDiagramVM.Min_Digits;
                        Operation.target = verimorDiagramVM.Target;
                        db.SaveChanges();
                    }
                }
                else
                {
                    var Operation = db.VerimorOperations.Find(verimorDiagramVM.ID);
                    if (Operation != null)
                    {
                        Operation.Title = verimorDiagramVM.Title;
                        Operation.phrase = verimorDiagramVM.Phrase;
                        Operation.Condition = $"{verimorDiagramVM.ConditionType},{verimorDiagramVM.ConditionParameters}";
                        db.SaveChanges();
                    }
                }
                TempData["ErrorMessage"] = "Güncellendi";
                return RedirectToAction("OperationList", "VerimorSettings");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("OperationList", "VerimorSettings");
            }
        }
        public ActionResult DeleteOperation(int? ID)
        {
            try
            {
                var DeleteOperation = db.VerimorOperations.Find(ID);
                db.VerimorOperations.Remove(DeleteOperation);
                db.SaveChanges();
                TempData["ErrorMessage"] = "Operasyon Silindi";
                return RedirectToAction("OperationList", "VerimorSettings");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("OperationList", "VerimorSettings");
            }
        }
        public ActionResult UpdateOperations(int? parentID) // yeni menü tipi
        {
            var Digits = db.VerimorOperationResponses.Where(m => m.ParentID == parentID).Select(m => m.digit).ToArray();
            int[] ChildList = new int[Digits.Length];
            string[] DigitList = new string[Digits.Length];
            for (int i = 0; i < Digits.Length; i++)
            {
                var DigitIndex = Digits[i];
                ChildList[i] = db.VerimorOperationResponses.Where(m => m.digit == DigitIndex && m.ParentID == parentID).Select(m => m.OperationID).FirstOrDefault();
                DigitList[i] = Digits[i];
            }
            return Json(new { ChildList = ChildList, DigitList = DigitList }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditOperations(string Digit, int? Parent, int Child)
        {
            try
            {
                var ErrorMessage = "";
                if (Digit == "")
                {
                    Digit = null;
                }
                var Operationresponse = db.VerimorOperationResponses.Where(m => m.ParentID == Parent && m.digit == Digit).FirstOrDefault();
                if (Operationresponse == null)
                {
                    VerimorOperationResponse verimorOperationResponse = new VerimorOperationResponse
                    {
                        digit = Digit,
                        ParentID = Parent,
                        OperationID = Child
                    };
                    db.VerimorOperationResponses.Add(verimorOperationResponse);
                    db.SaveChanges();
                    ErrorMessage = "Yeni Kayıt Eklendi";
                }
                else
                {
                    Operationresponse.digit = Digit;
                    Operationresponse.ParentID = Parent;
                    Operationresponse.OperationID = Child;
                    db.SaveChanges();
                    ErrorMessage = "Kayıt Güncellendi";
                }
                return Json(new { ErrorMessage = ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { ErrorMessage = ex.Message });
            }

        }
        public ActionResult GeneralFaults()
        {
            //var credentials = RadiusR.DB.DomainsCache.DomainsCache.GetDomainByID(RadiusR.DB.CustomerWebsiteSettings.WebsiteServicesInfrastructureDomainID);
            //RezaB.TurkTelekom.WebServices.TTApplication.TTApplicationServiceClient client = new(credentials.TelekomCredential.XDSLWebServiceUsernameInt, credentials.TelekomCredential.XDSLWebServicePassword, credentials.TelekomCredential.XDSLWebServiceCustomerCodeInt);
            var generalFaults = db.GeneralFaults.ToArray();
            var generalFaultList = generalFaults.Select(gf => new ViewModels.GeneralFaultViewModel()
            {
                Description = gf.Description,
                EndTime = gf.EndTime,
                ID = gf.ID,
                ProvinceId = gf.Province,
                ProvinceName = gf.ProvinceName,
                StartTime = gf.StartTime,
                UpdateTime = gf.UpdateTime
            });
            return View(generalFaultList);
        }
        public ActionResult EditGeneralFault(long ID)
        {
            var currentFault = db.GeneralFaults.Find(ID);

            return View(new ViewModels.GeneralFaultViewModel()
            {
                Description = currentFault.Description,
                EndTime = currentFault.EndTime,
                StartTime = currentFault.StartTime,
                ProvinceId = currentFault.Province,
                ProvinceName = currentFault.ProvinceName,
                ID = currentFault.ID,
                UpdateTime = currentFault.UpdateTime
            });
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditGeneralFault(ViewModels.GeneralFaultViewModel model)
        {
            var currentFault = db.GeneralFaults.Find(model.ID);
            if (currentFault == null)
            {
                return View(model);
            }
            currentFault.Description = model.Description;
            currentFault.EndTime = model.EndTime;
            currentFault.ProvinceName = model.ProvinceName;
            currentFault.Province = (int)model.ProvinceId;
            currentFault.UpdateTime = DateTime.Now;
            currentFault.StartTime = model.StartTime;
            ModelState.AddModelError("", "Genel Arıza Güncellendi");
            db.SaveChanges();
            return View(model);
        }
    }
}