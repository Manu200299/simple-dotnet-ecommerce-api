using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RESTful_API.Models;
using System.Collections.Generic;

namespace RESTful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string api = "ISIWebAPI";

        /// <summary>
        /// GET Request to fetch all orders for a specific User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get[controller]FromUser/{userId}")]
        public IActionResult GetOrdersByUser(int userId)
        {
            try
            {
                var orders = new List<object>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Get Orders for the User
                    var query = @"SELECT OrderID, UserID, Total, OrderDate, StatusID 
                          FROM Orders WHERE UserID = @UserID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            // Read all orders from the first DataReader
                            while (reader.Read())
                            {
                                var order = new OrderModel
                                {
                                    OrderID = reader.GetInt32(0),
                                    UserID = reader.GetInt32(1),
                                    Total = reader.GetDecimal(2),
                                    OrderDate = reader.GetDateTime(3),
                                    StatusID = reader.GetInt32(4)
                                };

                                // After reading the order, now query and read OrderDetails
                                var orderDetails = new List<OrderDetailModel>();
                                var detailQuery = @"SELECT OrderDetailID, OrderID, ProductID, Quantity, ProductPrice 
                                            FROM OrderDetails WHERE OrderID = @OrderID";

                                using (var detailCommand = new SqlCommand(detailQuery, connection))
                                {
                                    detailCommand.Parameters.AddWithValue("@OrderID", order.OrderID);

                                    using (var detailReader = detailCommand.ExecuteReader())
                                    {
                                        while (detailReader.Read())
                                        {
                                            orderDetails.Add(new OrderDetailModel
                                            {
                                                OrderDetailID = detailReader.GetInt32(0),
                                                OrderID = detailReader.GetInt32(1),
                                                ProductID = detailReader.GetInt32(2),
                                                Quantity = detailReader.GetInt32(3),
                                                ProductPrice = detailReader.GetDecimal(4)
                                            });
                                        }
                                    }
                                }

                                // Add the order and its details to the response list
                                orders.Add(new
                                {
                                    Order = order,
                                    OrderDetails = orderDetails
                                });
                            }
                        }
                    }
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET Request for a specific Order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get[controller]ById/{orderId}")]
        public IActionResult GetOrder(int orderId)
        {
            try
            {
                var order = new OrderModel();
                var orderDetails = new List<OrderDetailModel>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Get Order
                    var query = @"SELECT OrderID, UserID, Total, OrderDate, StatusID FROM Orders WHERE OrderID = @OrderID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderID", orderId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                order.OrderID = reader.GetInt32(0);
                                order.UserID = reader.GetInt32(1);
                                order.Total = reader.GetDecimal(2);
                                order.OrderDate = reader.GetDateTime(3);
                                order.StatusID = reader.GetInt32(4);
                            }
                            else
                            {
                                return NotFound(new { Message = "Order not found" });
                            }
                        }
                    }

                    // Get OrderDetails
                    var detailQuery = @"SELECT OrderDetailID, OrderID, ProductID, Quantity, ProductPrice 
                                        FROM OrderDetails WHERE OrderID = @OrderID";

                    using (var detailCommand = new SqlCommand(detailQuery, connection))
                    {
                        detailCommand.Parameters.AddWithValue("@OrderID", orderId);

                        using (var reader = detailCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orderDetails.Add(new OrderDetailModel
                                {
                                    OrderDetailID = reader.GetInt32(0),
                                    OrderID = reader.GetInt32(1),
                                    ProductID = reader.GetInt32(2),
                                    Quantity = reader.GetInt32(3),
                                    ProductPrice = reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }

                var orderWithDetails = new
                {
                    Order = order,
                    OrderDetails = orderDetails
                };

                return Ok(orderWithDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST Request that creates a whole order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize] // Now, to create an order, a user needs to be logged in and have a jwt token
        [HttpPost]
        [Route("create[controller]")]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                int orderId;

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Insert Order
                    var query = @"INSERT INTO Orders (UserID, Total, StatusID, OrderDate)
                                  OUTPUT INSERTED.OrderID
                                  VALUES (@UserID, @Total, @StatusID, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", request.UserID);
                        command.Parameters.AddWithValue("@Total", request.Total);
                        command.Parameters.AddWithValue("@StatusID", request.StatusID);

                        orderId = (int)command.ExecuteScalar();
                    }

                    // Insert OrderDetails for each product
                    foreach (var item in request.OrderDetails)
                    {
                        var detailQuery = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, ProductPrice)
                                            VALUES (@OrderID, @ProductID, @Quantity, @ProductPrice)";

                        using (var detailCommand = new SqlCommand(detailQuery, connection))
                        {
                            detailCommand.Parameters.AddWithValue("@OrderID", orderId);
                            detailCommand.Parameters.AddWithValue("@ProductID", item.ProductID);
                            detailCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                            detailCommand.Parameters.AddWithValue("@ProductPrice", item.ProductPrice);

                            detailCommand.ExecuteNonQuery();
                        }
                    }
                }

                return Ok(new { Message = "Order created successfully", OrderID = orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// Delete order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpDelete("delete[controller]/{orderId}")]
        public IActionResult DeleteOrder(int orderId)
        {
            // Delete the order details first (cascade delete is possible)
            string deleteDetailsQuery = "DELETE FROM OrderDetails WHERE OrderID = @OrderID";
            string deleteOrderQuery = "DELETE FROM Orders WHERE OrderID = @OrderID";

            using (SqlConnection connection = new SqlConnection(api))
            {
                connection.Open();

                // Delete order details first
                SqlCommand deleteDetailsCommand = new SqlCommand(deleteDetailsQuery, connection);
                deleteDetailsCommand.Parameters.AddWithValue("@OrderID", orderId);
                int detailsRowsAffected = deleteDetailsCommand.ExecuteNonQuery();

                if (detailsRowsAffected == 0)
                {
                    return NotFound($"No details found for Order ID {orderId}.");
                }

                // Delete the order after details are removed
                SqlCommand deleteOrderCommand = new SqlCommand(deleteOrderQuery, connection);
                deleteOrderCommand.Parameters.AddWithValue("@OrderID", orderId);
                int rowsAffected = deleteOrderCommand.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound($"Order with ID {orderId} not found.");
                }

                return NoContent(); // No content is returned when deletion is successful
            }
        }

    }
}
