using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RESTful_API.Models;
using System.Collections.Generic;

namespace RESTful_API.Controllers
{
    /// <summary>
    /// ProductController API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string api = "ISIWebAPI";

        /// <summary>
        /// GET Request to fetch all products
        /// </summary>
        [HttpGet]
        [Route("getAll[controller]")]
        public IActionResult GetAllProducts()
        {
            try
            {
                var productList = new List<ProductsModel>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = "SELECT ProductID, CategoryID, Name, Description, Color, Price, Stock, CreatedAt, UpdatedAt FROM Products";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productList.Add(new ProductsModel
                            {
                                ProductID = reader.GetInt32(0),
                                CategoryID = reader.GetInt32(1),
                                Name = reader.GetString(2),
                                Description = !reader.IsDBNull(3) ? reader.GetString(3) : null,
                                Color = !reader.IsDBNull(4) ? reader.GetString(4) : null,
                                Price = reader.GetDecimal(5),
                                Stock = reader.GetInt32(6),
                                CreatedAt = reader.GetDateTime(7),
                                UpdatedAt = reader.GetDateTime(8),
                            });
                        }
                    }
                }

                if (productList.Count > 0)
                {
                    return Ok(productList);
                }
                else
                {
                    return NotFound(new { Message = "No products found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET Request to fetch a product by ID
        /// </summary>
        [HttpGet]
        [Route("get[controller]byid/{productId}")]
        public IActionResult GetProductById(int productId)
        {
            try
            {
                var product = new ProductsModel();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = "SELECT ProductID, CategoryID, Name, Description, Color, Price, Stock, CreatedAt, UpdatedAt FROM Products WHERE ProductID = @ProductID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", productId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                product.ProductID = reader.GetInt32(0);
                                product.CategoryID = reader.GetInt32(1);
                                product.Name = reader.GetString(2);
                                product.Description = !reader.IsDBNull(3) ? reader.GetString(3) : null;
                                product.Color = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                                product.Price = reader.GetDecimal(5);
                                product.Stock = reader.GetInt32(6);
                                product.CreatedAt = reader.GetDateTime(7);
                                product.UpdatedAt = reader.GetDateTime(8);

                                return Ok(product);
                            }
                            else
                            {
                                return NotFound(new { Message = "Product not found" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST Request to create a new product
        /// </summary>
        [HttpPost]
        [Route("add[controller]")]
        public IActionResult CreateProduct([FromBody] ProductsModel request)
        {
            try
            {
                int productId;

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = @"INSERT INTO Products (CategoryID, Name, Description, Color, Price, Stock)
                                  OUTPUT INSERTED.ProductID
                                  VALUES (@CategoryID, @Name, @Description, @Color, @Price, @Stock)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryID", request.CategoryID);
                        command.Parameters.AddWithValue("@Name", request.Name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Color", request.Color ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Price", request.Price);
                        command.Parameters.AddWithValue("@Stock", request.Stock);

                        productId = (int)command.ExecuteScalar();
                    }
                }

                return Ok(new { Message = "Product created successfully", ProductID = productId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// PUT Request to update a product
        /// </summary>
        [HttpPut]
        [Route("update[controller]/{productId}")]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductsModel updatedProduct)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = @"UPDATE Products
                                  SET CategoryID = @CategoryID, Name = @Name, Description = @Description, Color = @Color, 
                                      Price = @Price, Stock = @Stock, UpdatedAt = GETDATE()
                                  WHERE ProductID = @ProductID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryID", updatedProduct.CategoryID);
                        command.Parameters.AddWithValue("@Name", updatedProduct.Name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Description", updatedProduct.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Color", updatedProduct.Color ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Price", updatedProduct.Price);
                        command.Parameters.AddWithValue("@Stock", updatedProduct.Stock);
                        command.Parameters.AddWithValue("@ProductID", productId);

                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = "Product updated successfully" });
                        }
                        else
                        {
                            return NotFound(new { Message = "Product not found" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE Request to delete a product
        /// </summary>
        [HttpDelete]
        [Route("delete[controller]/{productId}")]
        public IActionResult DeleteProduct(int productId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = "DELETE FROM Products WHERE ProductID = @ProductID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", productId);

                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = "Product deleted successfully" });
                        }
                        else
                        {
                            return NotFound(new { Message = "Product not found" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }
    }
}
