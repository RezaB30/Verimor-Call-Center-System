using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebHook
{
    public class WebHookResponse
    {
        public Transfer transfer { get; set; }
        public Prompt prompt { get; set; }
        public Record record { get; set; }
    }
    public class Prompt
    {
        public string announcement_id { get; set; }
        public string min_digits { get; set; }
        public string max_digits { get; set; }
        public string retry_count { get; set; }
        public string service_url { get; set; }
        public string param_name { get; set; }
        public string phrase { get; set; }

    }
    public class Record
    {
        public string announcement_id { get; set; }
        public string phrase { get; set; }
    }
    public class Transfer
    {
        public string target { get; set; }
        public string greet_name { get; set; }
        public string greet_phrase { get; set; }
    }
}
