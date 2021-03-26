﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Verimor.Webhook.EventListener.ViewModels
{
    public class VerimorDiagramVM
    {
        [DisplayName("İşlem Tipi")]
        public int WebHookType { get; set; }
        [DisplayName("ID")]
        public int ID { get; set; }
        [DisplayName("Operasyon Tipi")]
        public int OperationType { get; set; }
        [DisplayName("Diyalog Metni")]
        public string Phrase { get; set; }
        [DisplayName("Anons ID'si")]
        public string AnnouncementID { get; set; }
        [DisplayName("Çağrı Hedefi")]
        public string Target { get; set; }
        [DisplayName("Maks Sayı Uzunluğu")]
        public string Max_Digits { get; set; }
        [DisplayName("Min Sayı Uzunluğu")]
        public string Min_Digits { get; set; }
        [DisplayName("Tekrar Sayısı")]
        public string Retry_Count { get; set; }
        [DisplayName("Hata Mesajı")]
        public string ErrorMessage { get; set; }
    }
}