namespace WebServices.Entities.Models
{
    public class Transaction : Entity
    {
        public string sku { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
    }
}
