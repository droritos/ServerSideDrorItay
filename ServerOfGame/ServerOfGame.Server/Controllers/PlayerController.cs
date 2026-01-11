using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;

[ApiController]
[Route("api/player")]
public class PlayerController : ControllerBase
{
    private static PlayerProfile testPlayerProfile = new PlayerProfile
    {
        username = "Moshiko",
        level = 1,
        xp = 0
    };

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(testPlayerProfile);
    }

    [HttpPut("profile")]
    public IActionResult UpdateProfile([FromBody] PlayerProfile updatedData)
    {
        Console.WriteLine($"Update requested for: {updatedData.username}");

        // Check if the user is allowed to update
        bool canUpdatePlayer = testPlayerProfile.username == updatedData.username;

        if (!canUpdatePlayer)
        {
            // Return 400 Bad Request so the Client knows it failed
            return BadRequest("Username does not match!");
        }

        // Apply updates
        testPlayerProfile.level = updatedData.level;
        testPlayerProfile.xp = updatedData.xp;

        Console.WriteLine($"Update Saved! New Lvl: {testPlayerProfile.level}");

        return Ok(testPlayerProfile);
    }
}


