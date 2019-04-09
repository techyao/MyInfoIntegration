using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyInfoIntegration.Models
{
    public class ResponseData
    {
        public string status { get; set; }
        public object data { get; set; }
        public string message { get; set; }
        
        public ResponseData(object obj)
        {
            status = "OK";
            data = obj;
        }

        public ResponseData(object obj, string msg)
        {
            status = "NOK";
            message = msg;
            data = obj;
        }
    }
}