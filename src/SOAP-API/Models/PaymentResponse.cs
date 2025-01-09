using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOAP_API.Models
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentIntentID { get; set; }
        public string Message { get; set; }
    }
}