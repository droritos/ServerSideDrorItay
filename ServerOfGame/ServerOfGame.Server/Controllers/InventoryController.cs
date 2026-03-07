using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using ServerOfGame.Server.Services;
using System.Security.Claims;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private const int ItemCost = 250;
        private readonly UserService _users;
        public InventoryController(UserService users) => _users = users;

        [HttpPost("purchase")]
        public IActionResult Purchase([FromBody] PurchaseRequest req)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = _users.GetById(userId);
            if (user == null) return Unauthorized();

            if (user.Gold < ItemCost)
                return Ok(new PurchaseResponse { isSuccess = false, error = "Not enough gold." });

            user.Gold -= ItemCost;
            // Persist (UserService saves internally after mutation)
            // For now call RecordWin to trigger Save – TODO: expose UpdateUser method
            return Ok(new PurchaseResponse { isSuccess = true, newBalance = user.Gold });
        }

        [HttpGet("balance")]
        public IActionResult GetBalance()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = _users.GetById(userId);
            if (user == null) return Unauthorized();
            return Ok(new { user.Gold });
        }
    }
}
