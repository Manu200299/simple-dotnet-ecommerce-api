using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOAP_API.Models
{
    public class PaymentRequest
    {
        public int OrderID { get; set; }
        public int PaymentMethodID { get; set; }
        public decimal Amount { get; set; }
    }
}