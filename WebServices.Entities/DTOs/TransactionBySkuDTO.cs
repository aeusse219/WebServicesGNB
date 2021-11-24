using System.Collections.Generic;

namespace WebServices.Entities.DTOs
{
    public class TransactionBySkuDto
    {
        public decimal TotalAmount { get; set; }
        public List<TransactionDto> ListTransactions { get; set; }
    }
}
