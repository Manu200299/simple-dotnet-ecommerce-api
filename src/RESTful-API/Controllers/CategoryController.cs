using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RESTful_API.Models;
using System.Collections.Generic;

namespace RESTful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CategoryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string api = "ISIWebAPI";

        /// <summary>
        /// GET all categories
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll[controller]")]
        public IActionResult GetCategories()
        {
            try
            {
                var categories = new List<CategoryModel>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    var query = @"SELECT CategoryID, CategoryName, Description FROM CategoryController";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new CategoryModel
                                {
                                    CategoryID = reader.GetInt32(0),
                                    CategoryName = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                                });
                            }
                        }
                    }
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET a specific category by CategoryID
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get[controller]ById/{categoryId}")]
        public IActionResult GetCategory(int categoryId)
        {
            try
            {
                var category = new CategoryModel();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    var query = @"SELECT CategoryID, CategoryName, Description FROM CategoryController WHERE CategoryID = @CategoryID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryID", categoryId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                category.CategoryID = reader.GetInt32(0);
                                category.CategoryName = reader.GetString(1);
                                category.Description = reader.IsDBNull(2) ? null : reader.GetString(2);
                            }
                            else
                            {
                                return NotFound(new { Message = "CategoryController not found" });
                            }
                        }
                    }
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// POST a new category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("add[controller]")]
        public IActionResult CreateCategory([FromBody] CategoryModel category)
        {
            try
            {
                int categoryId;

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    var query = @"INSERT INTO CategoryController (CategoryName, Description)
                                  OUTPUT INSERTED.CategoryID
                                  VALUES (@CategoryName, @Description)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                        command.Parameters.AddWithValue("@Description", category.Description);

                        categoryId = (int)command.ExecuteScalar();
                    }
                }

                return Ok(new { Message = "CategoryController created successfully", CategoryID = categoryId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// PUT (Update) a category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update[controller]/{categoryId}")]
        public IActionResult UpdateCategory(int categoryId, [FromBody] CategoryModel category)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    var query = @"UPDATE CategoryController
                                  SET CategoryName = @CategoryName, Description = @Description
                                  WHERE CategoryID = @CategoryID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryID", categoryId);
                        command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                        command.Parameters.AddWithValue("@Description", category.Description);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { Message = "CategoryController not found" });
                        }
                    }
                }

                return Ok(new { Message = "CategoryController updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE a category by CategoryID
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete[controller]/{categoryId}")]
        public IActionResult DeleteCategory(int categoryId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    var query = @"DELETE FROM CategoryController WHERE CategoryID = @CategoryID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryID", categoryId);

                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = "CategoryController deleted successfully!" });
                        }
                        else
                        {
                            return StatusCode(500, new { Message = "CategoryController not found" });
                        }
                    }
                }
                //return NoContent(); // No content is returned when deletion is successful
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }
    }
}
