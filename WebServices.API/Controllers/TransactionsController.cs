using Microsoft.AspNetCore.Mvc;
using WebServices.Application.Contracts;

namespace WebServices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionApplication _transaction;
        public TransactionsController(ITransactionApplication transaction)
        {
            _transaction = transaction;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_transaction.GetAllTransactions());
        }

        [HttpGet]
        [Route("api/GetListTransactionBySKU/{sku}")]
        public IActionResult GetListTransactionBySKU(string sku)
        {
            return Ok(_transaction.GetListTransactionBySKU(sku));
        }
    }
}
