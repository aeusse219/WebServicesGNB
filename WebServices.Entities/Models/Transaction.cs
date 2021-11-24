namespace WebServices.Entities.Models
{
    public class Transaction : Entity
    {
        public string Sku { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
