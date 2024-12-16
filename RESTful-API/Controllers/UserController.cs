using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using RESTful_API.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Server;
using Newtonsoft.Json;

namespace RESTful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string api = "ISIWebAPI";

        /// <summary>
        /// GET Request (All Users)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Get[controller]")]
        public string GetUsers()
        {
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString(api).ToString());
            SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM Users", connection);
            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            List<UserModel> usersList = new List<UserModel>();
            ResponseModel response = new ResponseModel();


            if (dataTable.Rows.Count > 0)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    UserModel user = new UserModel();
                    user.UserID = Convert.ToInt32(dataTable.Rows[i]["UserID"]); 
                    user.Username = Convert.ToString(dataTable.Rows[i]["Username"]); 
                    user.Email = Convert.ToString(dataTable.Rows[i]["Email"]); 
                    user.PasswordHash = Convert.ToString(dataTable.Rows[i]["PasswordHash"]);
                    user.Salt = Convert.ToString(dataTable.Rows[i]["Salt"]); 
                    //user.Token = Convert.ToInt32(dataTable.Rows[i]["Token"]); 
                    //user.CreatedAt = Convert.ToDateTime(dataTable.Rows[i]["CreatedAt"]); 
                    //user.UpdatedAt = Convert.ToDateTime(dataTable.Rows[i]["UpdatedAt"]); 
                    usersList.Add(user);
                }
            }
            if (usersList.Count > 0)
            {
                return JsonConvert.SerializeObject(usersList);
            }
            else {
                response.StatusCode = 100;
                response.ErrorMessage = "No data found";
                return  JsonConvert.SerializeObject(response);
            }
        }

        /// <summary>
        /// POST Request (Add User)
        /// </summary>
        [HttpPost]
        [Route("api/Post[controller]")]
        public string PostUser(UserModel user)
        {
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString(api).ToString());
            SqlDataAdapter dataAdapter = new SqlDataAdapter("INSERT INTO Users(Username, Email, PasswordHash, Salt, Token, CreatedAt, UpdatedAt) VALUES("+user.Username")", connection);
        }
    }
}
