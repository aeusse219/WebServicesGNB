using System.Collections.Generic;

namespace WebServices.Entities.DTOs
{
    public class TransactionBySkuDTO
    {
        public decimal TotalAmount { get; set; }
        public List<TransactionDTO> ListTransactions { get; set; }
    }
}
