using Microsoft.AspNetCore.Mvc;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult GetServerStatus()
        {
            return Ok(new
            {
                Status = "Online",
                Message = "Welcome to the Game Server!",
                ActiveProjectiles = 0 // We can hook this up to real data later!
            });
        }
    }
}