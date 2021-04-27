using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenter.Operation
{
    public static class Utilities
    {

    }

    public static class CustomVPOSSettings
    {
        public static string MerchantID { get { return Properties.Settings.Default.MerchantID; } }
        public static string StoreKey { get { return Properties.Settings.Default.StoreKey; } }
        public static string UserID { get { return Properties.Settings.Default.UserID; } }
        public static string UserPassword { get { return Properties.Settings.Default.UserPass; } }
        public static string MerchantSalt { get { return Properties.Settings.Default.MerchantSalt; } }
        public static int CurrentVPOSID { get { return Properties.Settings.Default.CurrentVPOSID; } }
    }
}
