namespace WebServices.Entities.Models
{
    public class Rate : Entity
    {
        public string from { get; set; }
        public string to { get; set; }
        public decimal rate { get; set; }
    }
}
