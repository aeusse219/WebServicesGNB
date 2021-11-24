using Microsoft.AspNetCore.Mvc;
using WebServices.Application.Contracts;

namespace WebServices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transaction;
        public TransactionsController(ITransactionService transaction)
        {
            _transaction = transaction;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_transaction.GetAllTransactions());
        }

        [HttpGet]
        [Route("api/GetTransactionBySKU/{sku}")]
        public IActionResult GetTransactionBySKU(string sku)
        {
            return Ok(_transaction.GetTransactionBySKU(sku));
        }
    }
}
