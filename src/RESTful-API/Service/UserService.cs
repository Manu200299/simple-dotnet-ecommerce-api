using Microsoft.Data.SqlClient;
using RESTful_API.Models;

namespace RESTful_API.Service
{
    public interface IUserService
    {
        UserModel ValidateUser(string username, string password);
    }

    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Validates the user credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserModel ValidateUser(string username, string password)
        {
            UserModel user = null;

            using (var connection = new SqlConnection(_configuration.GetConnectionString("ISIWebAPI")))
            {
                connection.Open();

                // Query to check if the user exists and password matches
                var query = @"SELECT UserID, Username, Password FROM Users 
                          WHERE Username = @Username AND Password = @Password";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new UserModel
                            {
                                UserID = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                // Optionally include the password or other fields if needed
                            };
                        }
                    }
                }
            }

            return user;
        }
    }
}
