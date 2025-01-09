using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Owin.BuilderProperties;
using RESTful_API.Models;

namespace RESTful_API.Controllers
{
    /// <summary>
    /// AddressController API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IConfiguration _configuration; 

        public AddressController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string api = "ISIWebAPI";

        /// <summary>
        /// GET Request to fetch all Addresses on the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll[controller]")]
        public IActionResult GetAllAdresses()
        {
            try
            {
                var addressList = new List<AddressModel>();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = ("SELECT * FROM Address");
                    using(var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            addressList.Add(new AddressModel
                            {
                                AddressID = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                PhoneNumber = reader.GetString(3),
                                StreetName = reader.GetString(4),
                                StreetAdditional = !reader.IsDBNull(5) ? reader.GetString(5) : null, // can be null
                                PostalCode = reader.GetString(6),
                                District = reader.GetString(7),
                                City = reader.GetString(8),
                                Country = reader.GetString(9),
                                AdditionalNote = !reader.IsDBNull(10) ? reader.GetString(10) : null // can be null
                            });
                        }
                    }
                }
                if(addressList.Count > 0)
                {
                    return Ok(addressList);
                }
                else
                {
                    return NotFound(new { Message = "No data found"});
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// GET Request to fetch a specific address by id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get[controller]ById/{userId}")]
        public IActionResult GetAddressById(int userId)
        {
            try
            {
                var address = new AddressModel();

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();
                    var query = ("SELECT UsersAddress.UserID, FirstName, LastName, PhoneNumber, StreetName, StreetAdditional, " +
                        "PostalCode, District, City, Country, AdditionalNote " +
                        "FROM [Address] INNER JOIN UsersAddress " +
                        "ON  UsersAddress.AddressID = [Address].AddressID " +
                        "WHERE UsersAddress.UserID = @UserId");

                    using (var command = new SqlCommand(query, connection))
                    {
                        // UserID parameter
                        command.Parameters.AddWithValue("@UserId", userId);

                        using(var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                address.AddressID = reader.GetInt32(0);
                                address.FirstName = reader.GetString(1);
                                address.LastName = reader.GetString(2);
                                address.PhoneNumber = reader.GetString(3);
                                address.StreetName = reader.GetString(4);
                                address.StreetAdditional = !reader.IsDBNull(5) ? reader.GetString(5) : null; // can be null
                                address.PostalCode = reader.GetString(6);
                                address.District = reader.GetString(7);
                                address.City = reader.GetString(8);
                                address.Country = reader.GetString(9);
                                address.AdditionalNote = !reader.IsDBNull(10) ? reader.GetString(10) : null; // can be null
                                return Ok(address);
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


        /// <summary>
        /// POST Request to post an address to an user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("add[controller]ToUser/{userId}")]
        public IActionResult CreateAddress(int userId, [FromBody] AddressModel request)
        {

            try
            {
                int addressId;

                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Insert into the Address table
                    var insertAddressQuery = "INSERT INTO [Address] " +
                                              "(FirstName, LastName, PhoneNumber, StreetName, StreetAdditional, PostalCode, District, City, Country, AdditionalNote) " +
                                              "OUTPUT INSERTED.AddressID " +
                                              "VALUES (@FirstName, @LastName, @PhoneNumber, @StreetName, @StreetAdditional, @PostalCode, @District, @City, @Country, @AdditionalNote)";

                    using (var addressCommand = new SqlCommand(insertAddressQuery, connection))
                    {
                        addressCommand.Parameters.AddWithValue("@FirstName", request.FirstName);
                        addressCommand.Parameters.AddWithValue("@LastName", request.LastName);
                        addressCommand.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
                        addressCommand.Parameters.AddWithValue("@StreetName", request.StreetName);
                        addressCommand.Parameters.AddWithValue("@StreetAdditional", (object)request.StreetAdditional ?? DBNull.Value);
                        addressCommand.Parameters.AddWithValue("@PostalCode", request.PostalCode);
                        addressCommand.Parameters.AddWithValue("@District", request.District);
                        addressCommand.Parameters.AddWithValue("@City", request.City);
                        addressCommand.Parameters.AddWithValue("@Country", request.Country);
                        addressCommand.Parameters.AddWithValue("@AdditionalNote", (object)request.AdditionalNote ?? DBNull.Value);

                        addressId = (int)addressCommand.ExecuteScalar();
                    }

                    // Link the address to the user in UsersAddress table
                    var insertUserAddressQuery = "INSERT INTO UsersAddress (UserID, AddressID) VALUES (@UserID, @AddressID)";
                    using (var userAddressCommand = new SqlCommand(insertUserAddressQuery, connection))
                    {
                        userAddressCommand.Parameters.AddWithValue("@UserID", userId);
                        userAddressCommand.Parameters.AddWithValue("@AddressID", addressId);
                        userAddressCommand.ExecuteNonQuery();
                    }
                }

                return Ok(new { Message = "Address created successfully", AddressID = addressId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred", Error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE Request to delete an address
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="addressId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("delete[controller]FromUser/{userId}/{addressId}")]
        public IActionResult DeleteAddress(int userId, int addressId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // Delete the record from UsersAddress to unlink the address from the user
                    var deleteUsersAddressQuery = "DELETE FROM UsersAddress WHERE UserID = @UserID AND AddressID = @AddressID";
                    using (var command = new SqlCommand(deleteUsersAddressQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@AddressID", addressId);
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { Message = "Address not found or user is not associated with this address" });
                        }
                    }

                    // Optionally, delete the address from the Address table if no other users are associated with it
                    var deleteAddressQuery = "DELETE FROM Address WHERE AddressID = @AddressID AND NOT EXISTS (SELECT 1 FROM UsersAddress WHERE AddressID = @AddressID)";
                    using (var command = new SqlCommand(deleteAddressQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AddressID", addressId);
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = "Address deleted successfully" });
                        }
                        else
                        {
                            return Ok(new { Message = "Address deleted from user but still exists for other users" });
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
        /// PUT Request to update an address
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="addressId"></param>
        /// <param name="updatedAddress"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update[controller]FromUser/{userId}/{addressId}")]
        public IActionResult UpdateAddress(int userId, int addressId, [FromBody] AddressModel updatedAddress)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString(api)))
                {
                    connection.Open();

                    // SQL query to update the address details
                    var query = @"
                UPDATE Address
                SET 
                    FirstName = @FirstName,
                    LastName = @LastName,
                    PhoneNumber = @PhoneNumber,
                    StreetName = @StreetName,
                    StreetAdditional = @StreetAdditional,
                    PostalCode = @PostalCode,
                    District = @District,
                    City = @City,
                    Country = @Country,
                    AdditionalNote = @AdditionalNote
                WHERE AddressID = @AddressID 
                AND EXISTS (SELECT 1 FROM UsersAddress WHERE UserID = @UserID AND AddressID = @AddressID)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Parameters for the update
                        command.Parameters.AddWithValue("@FirstName", updatedAddress.FirstName);
                        command.Parameters.AddWithValue("@LastName", updatedAddress.LastName);
                        command.Parameters.AddWithValue("@PhoneNumber", updatedAddress.PhoneNumber);
                        command.Parameters.AddWithValue("@StreetName", updatedAddress.StreetName);
                        command.Parameters.AddWithValue("@StreetAdditional", updatedAddress.StreetAdditional ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PostalCode", updatedAddress.PostalCode);
                        command.Parameters.AddWithValue("@District", updatedAddress.District);
                        command.Parameters.AddWithValue("@City", updatedAddress.City);
                        command.Parameters.AddWithValue("@Country", updatedAddress.Country);
                        command.Parameters.AddWithValue("@AdditionalNote", updatedAddress.AdditionalNote ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@AddressID", addressId);
                        command.Parameters.AddWithValue("@UserID", userId);

                        var rowsAffected = command.ExecuteNonQuery();

                        // Check if any row was updated
                        if (rowsAffected > 0)
                        {
                            return Ok(new { Message = "Address updated successfully" });
                        }
                        else
                        {
                            return NotFound(new { Message = "Address not found or user is not associated with this address" });
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
