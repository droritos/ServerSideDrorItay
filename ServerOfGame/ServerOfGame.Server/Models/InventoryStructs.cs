namespace ServerOfGame.Server.Models
{
    public struct PurchaseRequest
    {
        public int itemID { get; set; }
    }
    public struct PurchaseResponse
    {
        public bool isSuccess   { get; set; }
        public string error     { get; set; }
        public int newBalance { get; set; }
    }
}