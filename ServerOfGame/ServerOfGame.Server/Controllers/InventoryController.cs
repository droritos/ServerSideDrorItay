using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    [Route("api/Inventory")] // Matching like in Unity
    public class InventoryController : ControllerBase
    {
        private static int _currentGold = 500;
        private static int anyItemCost = 250;


        [HttpPost("purchase")]
        public IActionResult TryPurchase([FromBody] PurchaseRequest purchaseRequest)
        {
            PurchaseResponse purchaseResponse = new();

            if (anyItemCost <= _currentGold)
            {
                // Update Data Base
                _currentGold -= anyItemCost;

                purchaseResponse.isSuccess = true;

                // Updating the Player in Unity
                purchaseResponse.newBalance = _currentGold;
            }
            else
            { 
                purchaseResponse.isSuccess = false;
                purchaseResponse.error = "Not Enough Money";
            }

            return Ok(purchaseResponse);

        }
    }
}
