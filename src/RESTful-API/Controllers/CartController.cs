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
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        string api = "ISIWebAPI";

        public CartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GET Request to get the user's current cart
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getCartFromUser/{userId}")]
        public IActionResult GetCart(int userId)
        {
            try
            {
                var cartItems = new List<CartModel>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Get the user's cart items
                    var query = @"SELECT c.CartID, c.UserID, c.ProductID, c.Quantity, c.IsShared, p.Name AS ProductName, p.Price AS ProductPrice, c.SharedToken
                                  FROM Carts c
                                  JOIN Products p ON c.ProductID = p.ProductID
                                  WHERE c.UserID = @UserID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cartItems.Add(new CartModel
                                {
                                    CartID = reader.GetInt32(0),
                                    UserID = reader.GetInt32(1),
                                    ProductID = reader.GetInt32(2),
                                    Quantity = reader.GetInt32(3),
                                    IsShared = reader.GetBoolean(4),  // Handle 'bit' as boolean
                                    ProductName = reader.GetString(5),
                                    ProductPrice = reader.GetDecimal(6),
                                    SharedToken = reader.GetString(7),
                                });
                            }
                        }
                    }
                }

                if (cartItems.Count == 0)
                {
                    return NotFound(new { Message = "No items found in the cart." });
                }

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST Request to add a product to the user's cart
        /// </summary>
        /// <param name="request">Cart item details</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addToCart")]
        public IActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Check if the product is already in the cart
                    var checkQuery = @"SELECT COUNT(*) FROM Carts WHERE UserID = @UserID AND ProductID = @ProductID";

                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UserID", request.UserID);
                        checkCommand.Parameters.AddWithValue("@ProductID", request.ProductID);

                        var count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            // Update quantity if the product already exists in the cart
                            var updateQuery = @"UPDATE Carts 
                                                SET Quantity = Quantity + @Quantity 
                                                WHERE UserID = @UserID AND ProductID = @ProductID";

                            using (var updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@UserID", request.UserID);
                                updateCommand.Parameters.AddWithValue("@ProductID", request.ProductID);
                                updateCommand.Parameters.AddWithValue("@Quantity", request.Quantity);

                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Insert new product into the cart
                            var insertQuery = @"INSERT INTO Carts (UserID, ProductID, Quantity, IsShared, SharedToken)
                                                VALUES (@UserID, @ProductID, @Quantity, @IsShared, @SharedToken)";

                            using (var insertCommand = new SqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@UserID", request.UserID);
                                insertCommand.Parameters.AddWithValue("@ProductID", request.ProductID);
                                insertCommand.Parameters.AddWithValue("@Quantity", request.Quantity);
                                insertCommand.Parameters.AddWithValue("@IsShared", request.IsShared);  // Pass bool to SQL Server's 'bit'
                                insertCommand.Parameters.AddWithValue("@SharedToken", "notShared");
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }

                return Ok(new { Message = "Product added to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }


        /// <summary>
        /// PUT Request to generate and add a shared token to a specific cart
        /// </summary>
        /// <param name="cartId">The ID of the cart to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("addSharedCart/{cartId}")]
        public IActionResult AddSharedCart(int cartId, string sharedToken)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Update the SharedToken for the specific CartID
                    var query = @"UPDATE Carts 
                          SET SharedToken = @SharedToken 
                          WHERE CartID = @CartID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SharedToken", sharedToken);
                        command.Parameters.AddWithValue("@CartID", cartId);

                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { Message = "Cart not found with the specified ID." });
                        }
                    }

                    return Ok(new { Message = "Shared token added to cart successfully", cartId = cartId, SharedToken = sharedToken });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST Request to add a shared cart
        /// </summary>
        /// <param name="cartItems">List of cart items to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("addSharedCart")]
        public IActionResult AddSharedCart([FromBody] List<CartModel> cartItems)
        {
            try
            {
                if (cartItems == null || cartItems.Count == 0)
                {
                    return BadRequest(new { Message = "Cart items cannot be empty." });
                }

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    foreach (var item in cartItems)
                    {
                        var query = @"INSERT INTO Carts (UserID, ProductID, Quantity, IsShared, SharedToken)
                              VALUES (@UserID, @ProductID, @Quantity, @IsShared, @SharedToken)";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UserID", item.UserID);
                            command.Parameters.AddWithValue("@ProductID", item.ProductID);
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.Parameters.AddWithValue("@IsShared", true); // Set IsShared to true by default
                            command.Parameters.AddWithValue("@SharedToken", item.SharedToken);

                            command.ExecuteNonQuery();
                        }
                    }

                    return Ok(new { Message = "Shared cart added successfully",  });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }



        /// <summary>
        /// DELETE Request to remove a product from the user's cart
        /// </summary>
        /// <param name="cartId">The CartID of the item to remove</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("removeFromCart/{cartId}")]
        public IActionResult RemoveFromCart(int cartId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Remove the product from the cart
                    var query = @"DELETE FROM Carts WHERE CartID = @CartID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CartID", cartId);

                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { Message = "Product not found in the cart" });
                        }
                    }
                }

                return Ok(new { Message = "Product removed from cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

    }
}


