using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Services;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _users;
        public AuthController(UserService users) => _users = users;

        // POST api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] AuthRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Username and password are required.");

            var (ok, error) = _users.Register(req.Username, req.Password);
            if (!ok) return BadRequest(error);

            return Ok(new { message = "Registration successful! Please log in." });
        }

        // POST api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthRequest req)
        {
            var (ok, token, error) = _users.Login(req.Username, req.Password);
            if (!ok) return Unauthorized(error);

            return Ok(new { token, message = "Login successful!" });
        }

        // POST api/auth/ban  – admin use only (no full admin system wired, protect as needed)
        [HttpPost("ban")]
        public IActionResult Ban([FromBody] BanRequest req)
        {
            var (ok, error) = _users.BanUser(req.Username, req.Reason ?? "No reason given.");
            if (!ok) return NotFound(error);
            return Ok(new { message = $"{req.Username} has been banned." });
        }

        // POST api/auth/unban
        [HttpPost("unban")]
        public IActionResult Unban([FromBody] BanRequest req)
        {
            var (ok, error) = _users.UnbanUser(req.Username);
            if (!ok) return NotFound(error);
            return Ok(new { message = $"{req.Username} has been unbanned." });
        }

        // GET api/auth/leaderboard  – top 10 by wins
        [HttpGet("leaderboard")]
        public IActionResult Leaderboard()
        {
            var top = _users.GetAll()
                .OrderByDescending(u => u.Wins)
                .Take(10)
                .Select(u => new { u.Username, u.Wins, u.GamesPlayed })
                .ToList();
            return Ok(top);
        }
    }

    public class AuthRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class BanRequest
    {
        public string Username { get; set; } = string.Empty;
        public string? Reason  { get; set; }
    }
}
