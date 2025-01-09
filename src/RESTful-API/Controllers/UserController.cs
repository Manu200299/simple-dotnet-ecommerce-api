using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using RESTful_API.Models;
using RESTful_API.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Server;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;

namespace RESTful_API.Controllers
{
    /// <summary>
    /// UserController API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Connection string name from appsettings.json
        string api = "ISIWebAPI";


        #region GET REQUESTS


        /// <summary>
        /// GET Request to fetch all Users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll[controller]")]
        public IActionResult GetAllUsers()
        {
            try
            {
                // Inits the UserModel as a List
                var usersList = new List<UserModel>();

                // Connection string
                using(var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    // Opens the connection and builds the query and executes it
                    connection.Open();
                    var query = "SELECT * FROM Users";
                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Adds fetched fields to the correspondent UserModel fields
                            usersList.Add(new UserModel
                            {
                                UserID = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.GetString(2),
                                Password = reader.GetString(3),
                                Token = reader.GetString(4),
                            });
                        }
                    }
                }

                // If it detects 1+ users on the list it returns Ok 200 status code else its empty
                if(usersList.Count > 0)
                {
                    return Ok(usersList);
                }
                else
                {
                    return NotFound(new { Message = "No data found" });
                }
            } 
            // Error occurred
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }


        /// <summary>
        /// GET Request for user by its specific ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get[controller]ById/{userId}")]
        public IActionResult GetUserById(int userId)
        {
            try
            {
                // Creates new UserModel object as the goal is to fetch a single user and not a list of users
                var user = new UserModel();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = ("SELECT * FROM Users WHERE UserID = @UserId");

                    using (var command = new SqlCommand(query, connection))
                    {
                        // UserID parameter the safe way to avoid SQL injection
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Maps user object created earlier to each correspondent field
                                user.UserID = reader.GetInt32(0);
                                user.Username = reader.GetString(1);
                                user.Email = reader.GetString(2);
                                user.Password = reader.GetString(3);
                                user.Token = reader.GetString(4);
                                return Ok(user);
                            }
                            else
                            {
                                return NotFound(new { Message = "User not found" });
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

        #endregion

        #region POST REQUESTS


        /// <summary>
        /// POST Request to post a new user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("add[controller]")]
        public IActionResult AddUser([FromBody] UserModel newUser)
        {
            // UserName and email are required and makes sure they are inputted
            if (newUser == null || string.IsNullOrWhiteSpace(newUser.Username) || string.IsNullOrWhiteSpace(newUser.Email))
            {
                return BadRequest(new { Message = "Invalid input. Username and Email are required." });
            }

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // SQL Query for inserting a new user
                    var query = @"INSERT INTO Users (Username, Email, Token, Password)
                                  VALUES (@Username, @Email, @Token, @Password);
                                  SELECT SCOPE_IDENTITY();";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add parameters to the query
                        command.Parameters.AddWithValue("@Username", newUser.Username);
                        command.Parameters.AddWithValue("@Email", newUser.Email);
                        command.Parameters.AddWithValue("@Token", newUser.Token);
                        command.Parameters.AddWithValue("@Password", newUser.Password);

                        // Execute query and get the new user ID
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int newUserId = Convert.ToInt32(result);
                            return CreatedAtAction(nameof(GetUserById), new { userId = newUserId }, new { UserId = newUserId });
                        }
                        else
                        {
                            return StatusCode(500, new { Message = "User not found. Please try a valid user" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding the user.", Error = ex.Message });
            }
        }

        #endregion

        #region DELETE REQUESTS


        /// <summary>
        /// DELETE Request to delete certain user by id from the database
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete[controller]/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = ("DELETE FROM Users WHERE UserID = @UserId");

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = "User deleted successfully!"});
                        }
                        else
                        {
                            return StatusCode(500, new { Message = "User not found. Please try a valid user" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the user.", Error = ex.Message });
            }
        }

        #endregion

        #region PUT REQUESTS

        /// <summary>
        /// PUT request to update an existing user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update[controller]/{userId}")]
        public IActionResult UpdateUser(int userId, UserModel user)
        {
            try
            {
                // Adds connection string to sql db
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    // Start connection and executes desired query
                    connection.Open();
                    var query = ("UPDATE Users SET Username = @Username, Email = @Email, Password = @Password, UpdatedAt = @UpdatedAt WHERE UserID = @UserId");
                    using (var command = new SqlCommand(query, connection))
                    {
                        // Adds parameters that can be updated and auto updates the UpdatedAt to current date
                        command.Parameters.AddWithValue("UserID", userId);
                        command.Parameters.AddWithValue("Username", user.Username);
                        command.Parameters.AddWithValue("Email", user.Email);
                        command.Parameters.AddWithValue("Password", user.Password);
                        command.Parameters.AddWithValue("UpdatedAt", DateTime.Now);

                        // ExecuteNonQuery returns rows affected, therefore, if more than 0 rows are affected, means the PUT was successful
                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = "User updated successfully!" });
                        }
                        // If there are no rows affected, means no user was found or no updates were submitted
                        else
                        {
                            return StatusCode(500, new { Message = "User not found or no updates were presented. Please try a valid user" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while trying to update the user.", Error = ex.Message });
            }
        }
        #endregion

    }
}
