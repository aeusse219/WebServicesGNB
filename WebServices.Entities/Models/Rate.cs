namespace WebServices.Entities.Models
{
    public class Rate : Entity
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal rate { get; set; }
    }
}
