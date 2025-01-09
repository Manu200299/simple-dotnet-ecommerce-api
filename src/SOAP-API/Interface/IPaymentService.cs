using SOAP_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SOAP_API.Interface
{
    [ServiceContract]
    public interface IPaymentService
    {
        [OperationContract]
        PaymentResponse CreatePayment(PaymentRequest request);

        [OperationContract]
        PaymentStatusResponse GetPaymentStatus(int paymentId);
    }
}
