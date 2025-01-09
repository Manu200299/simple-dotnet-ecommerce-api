using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RESTful_API.Models;
using Stripe;
using System.Collections.Generic;

namespace RESTful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentStripeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("ISIWebAPI"); // Fetch the connection string from appsettings.json
        }

        public PaymentStripeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// POST Request to create a payment intent with Stripe
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("createPaymentIntentComplete")]
        public async Task<IActionResult> CreatePaymentIntentComplete([FromBody] CreatePaymentIntentRequest request)
        {
            var order = GetOrderFromDb(request.OrderID);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            var paymentMethod = GetPaymentMethodFromDb(request.PaymentMethodID);
            if (paymentMethod == null)
            {
                return BadRequest("Invalid PaymentMethodID");
            }

            if(string.IsNullOrEmpty(paymentMethod.MethodName))
            {
                return BadRequest("Payment method name is required");
            }

            try
            {
                // Create the payment intent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(order.Total * 100),
                    Currency = "eur",
                    PaymentMethodTypes = new List<string> { paymentMethod.MethodName },
                    Metadata = new Dictionary<string, string>
                    {
                        {"OrderID", order.OrderID.ToString() },
                        {"UserID", order.UserID.ToString() }
                    },
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                // Save the PaymentIntent ID in the payments table
                SavePaymentIntentToDb(order.OrderID, paymentIntent.Id, order.Total, paymentMethod.PaymentMethodID);

                //return Ok(new { ClientSecretCredential = paymentIntent.ClientSecret,  });
                return Ok(new { ClientSecretCredential = paymentIntent.ClientSecret , message = "Success", paymentMethod.MethodName });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST Request that confirms payment intent
        /// </summary>
        /// <param name="paymentIntentId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("confirmPaymentIntent")]
        public async Task<IActionResult> ConfirmPaymentIntent([FromBody] ConfirmPaymentIntentRequest request)
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(request.PaymentIntentId, new PaymentIntentConfirmOptions
            {
                PaymentMethod = request.PaymentMethodId
            });

            // Check if the payment requires further authentication (3D Secure, etc.)
            if (paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_source_action")
            {
                // Handle further authentication if necessary (redirect to 3D Secure)
                return Ok(new { requiresAction = true, clientSecret = paymentIntent.ClientSecret });
            }

            return Ok(new { success = true });
        }


        /// <summary>
        /// POST request for Stripe Webhook
        /// </summary>
        [HttpPost]
        [Route("[controller]/webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    "WebhookSigningSecret"  // Make sure you replace this with your actual webhook secret
                );

                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    UpdatePaymentStatus(paymentIntent.Id, "Succeeded");
                }
                else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    UpdatePaymentStatus(paymentIntent.Id, "Failed");
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        private OrderModel GetOrderFromDb(int orderId)
        {
            var connectionString = GetConnectionString(); // Get the actual connection string here

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("SELECT * FROM Orders WHERE OrderID = @OrderID", connection);
                command.Parameters.AddWithValue("@OrderID", orderId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new OrderModel
                        {
                            OrderID = (int)reader["OrderID"],
                            UserID = (int)reader["UserID"],
                            Total = (decimal)reader["Total"],
                            OrderDate = (DateTime)reader["OrderDate"],
                        };
                    }
                }
            }
            return null;
        }

        private void SavePaymentIntentToDb(int orderId, string paymentIntentId, decimal amount, int paymentMethodId)
        {
            var connectionString = GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(
                    "INSERT INTO Payments (OrderID, PaymentStatusID, PaymentDate, Amount, PaymentMethodID, PaymentIntentID) " +
                    "VALUES (@OrderID, @PaymentStatusID, @PaymentDate, @Amount, @PaymentMethodID, @PaymentIntentID)",
                    connection
                );

                command.Parameters.AddWithValue("@OrderID", orderId);
                command.Parameters.AddWithValue("@PaymentStatusID", 1); // Pending status
                command.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@PaymentMethodID", paymentMethodId);
                command.Parameters.AddWithValue("@PaymentIntentID", paymentIntentId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void UpdatePaymentStatus(string paymentIntentId, string status)
        {
            var connectionString = GetConnectionString(); // Get the actual connection string here

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(
                    "UPDATE Payments SET PaymentStatusID = @PaymentStatusID WHERE PaymentIntentID = @PaymentIntentID",
                    connection
                );

                command.Parameters.AddWithValue("@PaymentIntentID", paymentIntentId);
                command.Parameters.AddWithValue("@PaymentStatusID", status == "Succeeded" ? 2 : 3); // 2 = Succeeded, 3 = Failed

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private PaymentMethodModel GetPaymentMethodFromDb(int paymentMethodId)
        {
            var connectionString = GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand("SELECT * FROM PaymentMethods WHERE PaymentMethodID = @PaymentMethodID", connection);
                command.Parameters.AddWithValue("@PaymentMethodID", paymentMethodId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new PaymentMethodModel
                        {
                            PaymentMethodID = (int)reader["PaymentMethodID"],
                            MethodName = (string)reader["MethodName"] 
                        };
                    }
                }
            }
            return null;
        }
    }
}
