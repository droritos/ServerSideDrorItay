using Microsoft.AspNetCore.Mvc;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/auth")] // Matching like in Unity
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Simple check: user "Moshe", password "1254!"
            if (request.Username == "Moshe" && request.Password == "1254!")
            {
                return Ok(new
                {
                    token = "fake-jwt-token-123",
                    message = "Login Successful!"
                });
            }
            return Unauthorized("Wrong username or password");
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}