using System.Threading.Tasks;
using Data;
using UnityEngine;

namespace Server
{
    /// <summary>
    /// Talks to /api/inventory on the server.
    /// ApiClient automatically attaches the stored JWT.
    /// </summary>
    public class InventoryService : MonoBehaviour
    {
        [SerializeField] private ApiClient apiClient;

        private const string PurchaseEndpoint = "/api/inventory/purchase";
        private const string BalanceEndpoint  = "/api/inventory/balance";

        // ── Purchase ──────────────────────────────────────────

        public async void PurchaseItem(int itemId)
        {
            var request = new PurchaseRequest { itemID = itemId };
            var result  = await apiClient.SendRequestAsync<PurchaseResponse>(
                PurchaseEndpoint, GlobalData.POST, request);

            if (result.IsSuccess && result.Data.isSuccess)
            {
                Debug.Log($"<color=green>Bought item {itemId}! Gold left: {result.Data.newBalance}</color>");
                PopUpGUIHandler.Instance.HandlePopupRequest(
                    $"Purchased! Gold left: {result.Data.newBalance}", InfoPopupType.Log);
            }
            else
            {
                string err = result.Data.error ?? result.Error;
                Debug.LogError($"<color=red>Purchase failed: {err}</color>");
                PopUpGUIHandler.Instance.HandlePopupRequest(err, InfoPopupType.Error);
            }
        }

        // ── Get balance ───────────────────────────────────────

        public async Task<int> GetBalance()
        {
            var result = await apiClient.SendRequestAsync<BalanceResponse>(
                BalanceEndpoint, GlobalData.GET, null);

            if (result.IsSuccess)
            {
                Debug.Log($"[Inventory] Balance: {result.Data.gold}");
                return result.Data.gold;
            }
            return 0;
        }
    }

    [System.Serializable]
    public class BalanceResponse { public int gold; }
}
