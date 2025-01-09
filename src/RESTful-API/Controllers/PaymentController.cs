using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RESTful_API.Models;
using Stripe;
using System;
using System.Collections.Generic;

namespace RESTful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;


        string api = "ISIWebAPI";

        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GET Request to get payment details by PaymentID
        /// </summary>
        /// <param name="paymentId">The ID of the payment</param>
        /// <returns></returns>
        [HttpGet]
        [Route("get[controller]ById/{paymentId}")]
        public IActionResult GetPayment(int paymentId)
        {
            try
            {
                PaymentModel payment = null;

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Get payment details
                    var query = @"SELECT PaymentID, OrderID, PaymentStatusID, PaymentDate, Amount, PaymentMethodID
                                  FROM Payments WHERE PaymentID = @PaymentID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PaymentID", paymentId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                payment = new PaymentModel
                                {
                                    PaymentID = reader.GetInt32(0),
                                    OrderID = reader.GetInt32(1),
                                    PaymentStatusID = reader.GetInt32(2),
                                    PaymentDate = reader.GetDateTime(3),
                                    Amount = reader.GetDecimal(4),
                                    PaymentMethodID = reader.GetInt32(5)
                                };
                            }
                        }
                    }
                }

                if (payment == null)
                {
                    return NotFound(new { Message = "Payment not found" });
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST Request to create a payment
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <returns></returns>
        [HttpPost]
        [Route("create[controller]")]
        public IActionResult CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                int paymentId;

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Insert payment
                    var query = @"INSERT INTO Payments (OrderID, PaymentStatusID, PaymentDate, Amount, PaymentMethodID)
                                  OUTPUT INSERTED.PaymentID
                                  VALUES (@OrderID, @PaymentStatusID, GETDATE(), @Amount, @PaymentMethodID)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", request.OrderID);
                        command.Parameters.AddWithValue("@PaymentStatusID", request.PaymentStatusID);
                        command.Parameters.AddWithValue("@Amount", request.Amount);
                        command.Parameters.AddWithValue("@PaymentMethodID", request.PaymentMethodID);

                        paymentId = (int)command.ExecuteScalar();
                    }
                }

                return Ok(new { Message = "Payment created successfully", PaymentID = paymentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// PUT Request to update payment status
        /// </summary>
        /// <param name="paymentId">The ID of the payment</param>
        /// <param name="paymentStatusId">The new status ID for the payment</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update[controller]Status/{paymentId}")]
        public IActionResult UpdatePaymentStatus(int paymentId, [FromBody] int paymentStatusId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Update payment status
                    var query = @"UPDATE Payments SET PaymentStatusID = @PaymentStatusID WHERE PaymentID = @PaymentID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PaymentStatusID", paymentStatusId);
                        command.Parameters.AddWithValue("@PaymentID", paymentId);

                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { Message = "Payment not found" });
                        }
                    }
                }

                return Ok(new { Message = "Payment status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE Request to delete a payment
        /// </summary>
        /// <param name="paymentId">The ID of the payment</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete[controller]/{paymentId}")]
        public IActionResult DeletePayment(int paymentId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Delete payment
                    var query = @"DELETE FROM Payments WHERE PaymentID = @PaymentID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PaymentID", paymentId);

                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { Message = "Payment not found" });
                        }
                    }
                }

                return Ok(new { Message = "Payment deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }


        #region STRIPE

        [HttpPost]
        [Route("Stripe[controller]")]
        public async Task<IActionResult> ProcessStripePayment([FromBody] StripePaymentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = request.Amount,
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethodId,
                    Confirm = true, // Auto confirms the payment
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                        AllowRedirects = "never"
                    }
                };

                var service = new PaymentIntentService();
                PaymentIntent intent = await service.CreateAsync(options);

                return Ok(new
                {
                    PaymentIntentId = intent.Id,
                    Status = intent.Status,
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
        }


        #endregion
    }
}
