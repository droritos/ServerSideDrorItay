using UnityEngine;
using Data; 

namespace Server
{
    public class InventoryService : MonoBehaviour
    {
        [SerializeField] ApiClient apiClient;

      
        private const string endPoint = "/api/inventory/purchase";

        public void PurchaseItem(int id)
        {
           
            PurchaseRequest request = new PurchaseRequest();
            request.itemID = id;

            
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