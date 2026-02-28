using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;

[ApiController]
[Route("api/player")]
public class PlayerController : ControllerBase
{
    private static PlayerSession testPlayerProfile = new PlayerSession
    {
        Username = "Moshiko",
        Level = 1,
        ExperiencePoints = 0
    };

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        return Ok(testPlayerProfile);
    }

    [HttpPut("profile")]
    public IActionResult UpdateProfile([FromBody] PlayerSession updatedData)
    {
        Console.WriteLine($"Update requested for: {updatedData.Username}");

        // Check if the user is allowed to update
        bool canUpdatePlayer = testPlayerProfile.Username == updatedData.Username;

        if (!canUpdatePlayer)
        {
            // Return 400 Bad Request so the Client knows it failed
            return BadRequest("Username does not match!");
        }

        // Apply updates
        testPlayerProfile.Level = updatedData.Level;
        testPlayerProfile.ExperiencePoints = updatedData.ExperiencePoints;

        Console.WriteLine($"Update Saved! New Lvl: {testPlayerProfile.Level}");

        return Ok(testPlayerProfile);
    }
}


