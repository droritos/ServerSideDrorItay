using UnityEngine;
using Data; // Or wherever you put the classes

namespace Server
{
    public class InventoryService : MonoBehaviour
    {
        [SerializeField] ApiClient apiClient;

        // "Action" endpoints usually use POST
        private const string endPoint = "/api/inventory/purchase";

        public void PurchaseItem(int id)
        {
            // 1. Create the request object
            PurchaseRequest request = new PurchaseRequest();
            request.itemID = id;

            // 2. Send the POST request
            // Server needs to give me back a PurchaseResponse
            StartCoroutine(apiClient.SendRequest<PurchaseResponse>(
                endPoint,
                "POST", 
                request, 
                OnPurchaseComplete
            ));
        }

        private void OnPurchaseComplete(ApiResult<PurchaseResponse> result)
        {
            if (result.IsSuccess && result.Data.isSuccess)
            {
                Debug.Log($"<color=green>Bought Item!</color> Gold left: {result.Data.newBalance}");
            }
            else
            {
                string msg = result.Data.error ?? result.Error;
                Debug.LogError($"<color=red>Purchase Failed:</color> {msg}");
            }
        }
    }
}