using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOAP_API.Models
{
    public class PaymentModel
    {
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        public int PaymentStatusID { get; set; }
        public int PaymentMethodID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}