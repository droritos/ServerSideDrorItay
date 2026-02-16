using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using System.Linq;
using System.Text.Json;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/auth")] // Matching like in Unity
    public class AuthController : ControllerBase
    {
        private static string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");

        private List<User> _usersDB = LoadUsers();

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 1. Search for the user in our "DB"
            var foundUser = _usersDB.FirstOrDefault(u =>
                u.Username == request.Username &&
                u.Password == request.Password
            );

            // 2. Did we find them?
            if (foundUser != null)
            {
                return Ok(new
                {
                    // Send THEIR ID, not a fake one
                    token = foundUser.Id,
                    message = "Login Successful!"
                });
            }

            return Unauthorized("Wrong username or password");
        }
    
         [HttpPost("register")]
        public IActionResult Register([FromBody] LoginRequest request)
        {
            var userExists = _usersDB.FirstOrDefault(u => u.Username == request.Username);

            // Safe check empty
            if (userExists != null)
            {
                return BadRequest("User already exists");
            }

            // Add user than save it into the DB
            User newUser = new User(request.Username, request.Password);
            _usersDB.Add(newUser);
            SaveUsers();

            return Ok(new
            {
                message = "Registration successful!",
                token = "" // We send an empty token for now so Unity doesn't complain
            });
        }


        private static List<User> LoadUsers()
        {
            // Use "System.IO.File" instead of just "File"
            if (System.IO.File.Exists(_filePath))
            {
                string json = System.IO.File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            return new List<User>();
        }

        private void SaveUsers()
        {
            string json = JsonSerializer.Serialize(_usersDB, new JsonSerializerOptions { WriteIndented = true });

            // Use "System.IO.File" here too!
            System.IO.File.WriteAllText(_filePath, json);
        }
    }


    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}