using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RESTful_API.Models;
using System;
using System.Collections.Generic;

namespace RESTful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedCartController : Controller
    {
        private readonly IConfiguration _configuration;
        string api = "ISIWebAPI";

        public SharedCartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        /// <summary>
        /// POST Request to generate and share a cart
        /// </summary>
        /// <param name="userId">The ID of the user sharing their cart</param>
        /// <returns>A 5-digit share code</returns>
        [HttpPost]
        [Route("shareCart/{userId}")]
        public IActionResult ShareCart(int userId)
        {
            try
            {
                string shareCode;
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Generate a unique 5-digit code
                    do
                    {
                        shareCode = new Random().Next(10000, 99999).ToString("D5");
                        var checkQuery = "SELECT COUNT(*) FROM Shared_Carts WHERE ShareCode = @ShareCode";
                        using (var checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@ShareCode", shareCode);
                            var count = (int)checkCommand.ExecuteScalar();
                            if (count == 0) break;
                        }
                    } while (true);

                    // Insert the shared cart record
                    var insertQuery = @"INSERT INTO Shared_Carts (OwnerUserID, ShareCode, ExpiresAt) 
                                VALUES (@OwnerUserID, @ShareCode, DATEADD(hour, 24, GETDATE()))";
                    using (var insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@OwnerUserID", userId);
                        insertCommand.Parameters.AddWithValue("@ShareCode", shareCode);
                        insertCommand.ExecuteNonQuery();
                    }
                }

                return Ok(new { ShareCode = shareCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET Request to retrieve a shared cart
        /// </summary>
        /// <param name="shareCode">The 5-digit share code</param>
        /// <returns>The shared cart items</returns>
        [HttpGet]
        [Route("getSharedCart/{shareCode}")]
        public IActionResult GetSharedCart(string shareCode)
        {
            try
            {
                var sharedCartItems = new List<CartModel>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    var query = @"SELECT c.CartID, c.UserID, c.ProductID, c.Quantity, p.Name AS ProductName, p.Price AS ProductPrice
                          FROM Carts c
                          JOIN Products p ON c.ProductID = p.ProductID
                          JOIN Shared_Carts sc ON c.UserID = sc.OwnerUserID
                          WHERE sc.ShareCode = @ShareCode AND sc.ExpiresAt > GETDATE()";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ShareCode", shareCode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sharedCartItems.Add(new CartModel
                                {
                                    CartID = reader.GetInt32(0),
                                    UserID = reader.GetInt32(1),
                                    ProductID = reader.GetInt32(2),
                                    Quantity = reader.GetInt32(3),
                                    ProductName = reader.GetString(4),
                                    ProductPrice = reader.GetDecimal(5)
                                });
                            }
                        }
                    }
                }

                if (sharedCartItems.Count == 0)
                {
                    return NotFound(new { Message = "Shared cart not found or expired." });
                }

                return Ok(sharedCartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST Request to add a shared cart to the user's cart
        /// </summary>
        /// <param name="request">AddSharedCartRequest object containing UserID and ShareCode</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addSharedCart")]
        public IActionResult AddSharedCart([FromBody] AddSharedCartRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // First, get the shared cart items
                    var getSharedCartQuery = @"SELECT c.ProductID, c.Quantity
                                       FROM Carts c
                                       JOIN Shared_Carts sc ON c.UserID = sc.OwnerUserID
                                       WHERE sc.ShareCode = @ShareCode AND sc.ExpiresAt > GETDATE()";

                    var sharedCartItems = new List<(int ProductID, int Quantity)>();

                    using (var command = new SqlCommand(getSharedCartQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ShareCode", request.ShareCode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sharedCartItems.Add((reader.GetInt32(0), reader.GetInt32(1)));
                            }
                        }
                    }

                    if (sharedCartItems.Count == 0)
                    {
                        return NotFound(new { Message = "Shared cart not found or expired." });
                    }

                    // Now add or update items in the user's cart
                    foreach (var item in sharedCartItems)
                    {
                        var upsertQuery = @"MERGE Carts AS target
                                    USING (SELECT @UserID, @ProductID, @Quantity) AS source (UserID, ProductID, Quantity)
                                    ON (target.UserID = source.UserID AND target.ProductID = source.ProductID)
                                    WHEN MATCHED THEN
                                        UPDATE SET Quantity = target.Quantity + source.Quantity
                                    WHEN NOT MATCHED THEN
                                        INSERT (UserID, ProductID, Quantity)
                                        VALUES (source.UserID, source.ProductID, source.Quantity);";

                        using (var command = new SqlCommand(upsertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", request.UserID);
                            command.Parameters.AddWithValue("@ProductID", item.ProductID);
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                return Ok(new { Message = "Shared cart added to user's cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        public class AddSharedCartRequest
        {
            public int UserID { get; set; }
            public string ShareCode { get; set; }
        }
    }
}
