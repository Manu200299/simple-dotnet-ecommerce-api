using SOAP_API.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace SOAP_API.Service
{
    [WebService(Namespace = "http://isiwebapi.com/payments")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class PaymentServiceWS : WebService
    {
        // Connection string to the db
        private readonly string _connectionString = "Data Source=192.168.50.50,1433;Database=ISI_API;MultipleActiveResultSets=True;User Id=grupo11;Password=grupo11;TrustServerCertificate=True;";

        /// <summary>
        /// Method to make a payment
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="amount"></param>
        /// <param name="paymentMethodId"></param>
        /// <returns></returns>
        [WebMethod]
        public string MakePayment(int orderId, decimal amount, int paymentMethodId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Check if order exists
                    SqlCommand checkOrderCmd = new SqlCommand("SELECT COUNT(*) FROM Orders WHERE OrderID = @OrderId", connection);
                    checkOrderCmd.Parameters.AddWithValue("@OrderId", orderId);
                    int orderExists = (int)checkOrderCmd.ExecuteScalar();

                    if (orderExists == 0)
                    {
                        return "Order not found.";
                    }

                    // Insert payment
                    SqlCommand insertPaymentCmd = new SqlCommand(
                        @"INSERT INTO Payments (OrderID, PaymentStatusID, PaymentMethodID, Amount, PaymentDate)
                      VALUES (@OrderId, 1, @PaymentMethodId, @Amount, GETDATE());
                      SELECT SCOPE_IDENTITY();",
                        connection
                    );

                    insertPaymentCmd.Parameters.AddWithValue("@OrderId", orderId);
                    insertPaymentCmd.Parameters.AddWithValue("@PaymentMethodId", paymentMethodId);
                    insertPaymentCmd.Parameters.AddWithValue("@Amount", amount);

                    int paymentId = Convert.ToInt32(insertPaymentCmd.ExecuteScalar());
                    return $"Payment successful. Payment ID: {paymentId}";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Method to get all available payment methods
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public List<string> GetAllPaymentMethods()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT MethodName FROM PaymentMethods", connection);

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<string> paymentMethods = new List<string>();

                    while (reader.Read())
                    {
                        paymentMethods.Add(reader["MethodName"].ToString());
                    }

                    return paymentMethods;
                }
                catch (Exception ex)
                {
                    return new List<string> { $"Error: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Method to validate if a payment method exists
        /// </summary>
        /// <param name="paymentMethodId"></param>
        /// <returns></returns>
        [WebMethod]
        public string ValidatePaymentMethod(int paymentMethodId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM PaymentMethods WHERE PaymentMethodID = @PaymentMethodId", connection);
                    cmd.Parameters.AddWithValue("@PaymentMethodId", paymentMethodId);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0 ? "Valid payment method." : "Invalid payment method.";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Method to refund a payment
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [WebMethod]
        public string RefundPayment(int paymentId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Get the original payment details
                    SqlCommand getPaymentCmd = new SqlCommand(
                        @"SELECT OrderID, PaymentMethodID, Amount 
                  FROM Payments WHERE PaymentID = @PaymentId", connection);
                    getPaymentCmd.Parameters.AddWithValue("@PaymentId", paymentId);

                    SqlDataReader reader = getPaymentCmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        return "Payment not found.";
                    }

                    int orderId = Convert.ToInt32(reader["OrderID"]);
                    int paymentMethodId = Convert.ToInt32(reader["PaymentMethodID"]);
                    decimal amount = Convert.ToDecimal(reader["Amount"]);

                    reader.Close();

                    // Insert a negative payment record
                    SqlCommand refundCmd = new SqlCommand(
                        @"INSERT INTO Payments (OrderID, PaymentStatusID, PaymentMethodID, Amount, PaymentDate)
                  VALUES (@OrderId, 2, @PaymentMethodId, @RefundAmount, GETDATE());",
                        connection
                    );

                    refundCmd.Parameters.AddWithValue("@OrderId", orderId);
                    refundCmd.Parameters.AddWithValue("@PaymentMethodId", paymentMethodId);
                    refundCmd.Parameters.AddWithValue("@RefundAmount", -amount);

                    int rowsAffected = refundCmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? "Refund processed successfully." : "Refund failed.";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }


        /// <summary>
        /// Method to get payment details
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [WebMethod]
        public string GetPaymentDetails(int paymentId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT p.PaymentID, p.OrderID, pm.MethodName, p.Amount, ps.StatusName, p.PaymentDate
                  FROM Payments p
                  INNER JOIN PaymentMethods pm ON p.PaymentMethodID = pm.PaymentMethodID
                  INNER JOIN PaymentStatus ps ON p.PaymentStatusID = ps.PaymentStatusID
                  WHERE p.PaymentID = @PaymentId", connection);

                    cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        return $"Payment ID: {reader["PaymentID"]}, Order ID: {reader["OrderID"]}, " +
                               $"Method: {reader["MethodName"]}, Amount: {reader["Amount"]}, " +
                               $"Status: {reader["StatusName"]}, Date: {reader["PaymentDate"]}";
                    }
                    else
                    {
                        return "Payment not found.";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }


        /// <summary>
        /// Method to get payment status
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        [WebMethod]
        public string GetPaymentStatus(int paymentId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand(
                        @"SELECT ps.StatusName 
                      FROM Payments p
                      INNER JOIN PaymentStatus ps ON p.PaymentStatusID = ps.PaymentStatusID
                      WHERE p.PaymentID = @PaymentId",
                        connection
                    );

                    cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return $"Payment Status: {result}";
                    }
                    else
                    {
                        return "Payment not found.";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }
    }
}
