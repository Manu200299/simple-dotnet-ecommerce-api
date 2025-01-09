using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTful_API.Models;
using RESTful_API.Service;

namespace RESTful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserService _userService;

        public LoginController(JwtService jwtService, UserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        /// <summary>
        /// POST login request that verifies if the credentials are valid and generates a token for that user
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[controller]")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var user = _userService.ValidateUser(login.Username, login.Password);

            if(user == null)
            {
                return Unauthorized(new { message = "Invalid credentials"});
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new { token, user.UserID });
        }
    }
}
