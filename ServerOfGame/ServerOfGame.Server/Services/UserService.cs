using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using ServerOfGame.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ServerOfGame.Server.Services
{
    /// <summary>
    /// Handles user persistence, password hashing, JWT minting, and ban management.
    /// Data is stored in users.json (swap for a real DB in production).
    /// </summary>
    public class UserService
    {
        private readonly string _filePath;
        private readonly IConfiguration _config;
        private List<User> _users;
        private readonly object _lock = new();

        public UserService(IConfiguration config)
        {
            _config   = config;
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
            _users    = Load();
        }

        // ── Public API ─────────────────────────────────────────

        public (bool ok, string error) Register(string username, string password)
        {
            lock (_lock)
            {
                if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    return (false, "Username already taken.");

                string hash = BCrypt.Net.BCrypt.HashPassword(password);
                _users.Add(new User(username, hash));
                Save();
                return (true, string.Empty);
            }
        }

        public (bool ok, string token, string error) Login(string username, string password)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return (false, string.Empty, "Invalid username or password.");

                if (user.IsBanned)
                    return (false, string.Empty, $"Account banned: {user.BanReason}");

                return (true, MintJwt(user), string.Empty);
            }
        }

        public User? GetById(string id)
        {
            lock (_lock) { return _users.FirstOrDefault(u => u.Id == id); }
        }

        public User? GetByUsername(string username)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }

        public List<User> GetAll()
        {
            lock (_lock) { return _users.ToList(); }
        }

        public (bool ok, string error) BanUser(string username, string reason)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null) return (false, "User not found.");
                user.IsBanned  = true;
                user.BanReason = reason;
                Save();
                return (true, string.Empty);
            }
        }

        public (bool ok, string error) UnbanUser(string username)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null) return (false, "User not found.");
                user.IsBanned  = false;
                user.BanReason = string.Empty;
                Save();
                return (true, string.Empty);
            }
        }

        public void RecordWin(string username)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null) return;
                user.Wins++;
                user.GamesPlayed++;
                Save();
            }
        }

        public void RecordLoss(string username)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null) return;
                user.GamesPlayed++;
                Save();
            }
        }

        // ── JWT ────────────────────────────────────────────────

        private string MintJwt(User user)
        {
            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var hours = int.Parse(_config["Jwt:ExpiresInHours"] ?? "24");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim("isBanned",                user.IsBanned.ToString())
            };

            var token = new JwtSecurityToken(
                issuer:   _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:   claims,
                expires:  DateTime.UtcNow.AddHours(hours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ── Persistence ────────────────────────────────────────

        private List<User> Load()
        {
            if (!File.Exists(_filePath)) return new List<User>();
            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch { return new List<User>(); }
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
