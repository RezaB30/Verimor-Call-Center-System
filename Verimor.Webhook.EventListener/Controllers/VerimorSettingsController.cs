using RadiusR.Verimor.CallCenter.Caching;
using RezaB.Data.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Verimor.Webhook.EventListener.Models;

namespace Verimor.Webhook.EventListener.Controllers
{
    public class VerimorSettingsController : Controller
    {
        // GET: VerimorSettings
        RadiusR_NetSpeed_5Entities db = new RadiusR_NetSpeed_5Entities();
        public ActionResult Index()
        {

            var Operations = db.VerimorOperations.ToList();
            var OperationResponse = db.VerimorOperationResponses.ToList();
            List<ViewModels.VerimorSettings> settings = new List<ViewModels.VerimorSettings>();
            var OpList = Operations.Select(m => new SelectListItem
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
        public ActionResult CreateDiagram()
        {
            var WebHookType = new SelectList(EnumHelper.GetSelectList(typeof(Enums.VerimorWebHookTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
            ViewBag.WebHookTypes = WebHookType;
            var OperationTypes = new SelectList(EnumHelper.GetSelectList(typeof(Enums.VerimorOperationTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
            ViewBag.OperationTypes = OperationTypes;
            var Parameters = new LocalizedList<PhraseTypes, RadiusR.Verimor.CallCenter.Common>().GenericList;
            ViewBag.Parameters = new SelectList(Parameters.Select(p => new { Value = (PhraseTypes)p.ID, Text = p.Name }).ToArray(), "Value", "Text");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDiagram(ViewModels.VerimorDiagramVM verimorDiagramVM)
        {
            try
            {
                var WebHookType = new SelectList(EnumHelper.GetSelectList(typeof(Enums.VerimorWebHookTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
                ViewBag.WebHookTypes = WebHookType;
                var OperationTypes = new SelectList(EnumHelper.GetSelectList(typeof(Enums.VerimorOperationTypes)).Select(op => new { op.Value, op.Text }), "Value", "Text");
                ViewBag.OperationTypes = OperationTypes;
                var Parameters = new LocalizedList<PhraseTypes, RadiusR.Verimor.CallCenter.Common>().GenericList;
                ViewBag.Parameters = new SelectList(Parameters.Select(p => new { Value = (PhraseTypes)p.ID, Text = p.Name }).ToArray(), "Value", "Text");

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
                switch ((Enums.VerimorWebHookTypes)verimorDiagramVM.WebHookType)
                {
                    case Enums.VerimorWebHookTypes.Function:
                        {
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase) || verimorDiagramVM.OperationType == (int)Enums.VerimorOperationTypes.Basic)
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                phrase = verimorDiagramVM.Phrase,
                                operationType = verimorDiagramVM.OperationType,
                                ID = ID.Value
                            };
                        }
                        break;
                    case Enums.VerimorWebHookTypes.Prompt:
                        {
                            if ( string.IsNullOrEmpty(verimorDiagramVM.Min_Digits) || string.IsNullOrEmpty(verimorDiagramVM.Max_Digits) || string.IsNullOrEmpty(verimorDiagramVM.Retry_Count))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (!string.IsNullOrEmpty(verimorDiagramVM.Phrase) && !string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase) && string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                phrase = verimorDiagramVM.Phrase,
                                announcementID = verimorDiagramVM.AnnouncementID,
                                max_digits = verimorDiagramVM.Max_Digits,
                                min_digits = verimorDiagramVM.Min_Digits,
                                retry_count = verimorDiagramVM.Retry_Count,
                                operationType = (int)Enums.VerimorOperationTypes.Basic,
                                ID = ID.Value
                            };
                        }
                        break;
                    case Enums.VerimorWebHookTypes.Transfer:
                        {
                            if (string.IsNullOrEmpty(verimorDiagramVM.Target))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (!string.IsNullOrEmpty(verimorDiagramVM.Phrase) && !string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase) && string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                ID = ID.Value,
                                target = verimorDiagramVM.Target,
                                phrase = verimorDiagramVM.Phrase,
                                announcementID = verimorDiagramVM.AnnouncementID,
                                operationType = (int)Enums.VerimorOperationTypes.Basic
                            };
                        }
                        break;
                    case Enums.VerimorWebHookTypes.Record:
                        {
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase) && string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (!string.IsNullOrEmpty(verimorDiagramVM.Phrase) && !string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            if (string.IsNullOrEmpty(verimorDiagramVM.Phrase) && string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                            {
                                verimorDiagramVM.ErrorMessage = "Geçersiz işlem";
                                return View(verimorDiagramVM);
                            }
                            verimorOperation = new VerimorOperation()
                            {
                                ID = ID.Value,
                                phrase = verimorDiagramVM.Phrase,
                                announcementID = verimorDiagramVM.AnnouncementID,
                                operationType = (int)Enums.VerimorOperationTypes.Basic
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
        public ActionResult Edit(string ErrorMsg = null)
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            List<ViewModels.VerimorDiagramVM> verimorDiagramVMs = new List<ViewModels.VerimorDiagramVM>();
            var Operations = db.VerimorOperations.ToList();
            foreach (var item in Operations)
            {
                ViewModels.VerimorDiagramVM verimorDiagramVM = new ViewModels.VerimorDiagramVM
                {
                    ID = item.ID,
                    AnnouncementID = item.announcementID,
                    Max_Digits = item.max_digits,
                    Min_Digits = item.min_digits,
                    Phrase = item.phrase,
                    Retry_Count = item.retry_count,
                    Target = item.target,
                    ErrorMessage = ErrorMsg,
                    OperationType = (int)item.operationType,
                };
                verimorDiagramVMs.Add(verimorDiagramVM);
            }
            return View(verimorDiagramVMs.ToArray());
        }
        [HttpPost]
        public ActionResult Edit([Bind(Prefix ="item")]ViewModels.VerimorDiagramVM verimorDiagramVM)
        {
            try
            {
                if (verimorDiagramVM.OperationType == (int)Enums.VerimorOperationTypes.Basic)
                {
                    var Operation = db.VerimorOperations.Find(verimorDiagramVM.ID);
                    if (Operation != null)
                    {
                        if (!string.IsNullOrEmpty(verimorDiagramVM.Phrase) && !string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                        {
                            ViewBag.ErrorMessage = "Geçersiz işlem";
                            return View(verimorDiagramVM);
                        }
                        if (string.IsNullOrEmpty(verimorDiagramVM.Phrase) && string.IsNullOrEmpty(verimorDiagramVM.AnnouncementID))
                        {
                            ViewBag.ErrorMessage = "Geçersiz işlem";
                            return View(verimorDiagramVM);
                        }
                        if (!string.IsNullOrEmpty(Operation.target) && string.IsNullOrEmpty(verimorDiagramVM.Target))
                        {
                            ViewBag.ErrorMessage = "Geçersiz işlem";
                            return View(verimorDiagramVM);
                        }
                        if (!string.IsNullOrEmpty(Operation.retry_count) && (string.IsNullOrEmpty(verimorDiagramVM.Retry_Count) || string.IsNullOrEmpty(verimorDiagramVM.Max_Digits) || string.IsNullOrEmpty(verimorDiagramVM.Min_Digits)))
                        {
                            ViewBag.ErrorMessage = "Geçersiz işlem";
                            return View(verimorDiagramVM);
                        }
                        Operation.announcementID = verimorDiagramVM.AnnouncementID;
                        Operation.phrase = verimorDiagramVM.Phrase;
                        Operation.retry_count = verimorDiagramVM.Retry_Count;
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
                        Operation.phrase = verimorDiagramVM.Phrase;
                        Operation.announcementID = verimorDiagramVM.AnnouncementID;
                        db.SaveChanges();
                    }
                }
                TempData["ErrorMessage"] = "Güncellendi";
                return RedirectToAction("Edit", "VerimorSettings");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Edit", "VerimorSettings");
            }
        }
        public ActionResult DeleteOperation(int? ID)
        {
            try
            {
                var DeleteOperation = db.VerimorOperations.Find(ID);
                db.VerimorOperations.Remove(DeleteOperation);
                db.SaveChanges();
                return RedirectToAction("Edit", "VerimorSettings", new { ErrorMsg = "Operasyon Silindi" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Edit", "VerimorSettings", new { ErrorMsg = ex.Message });
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
    }
}