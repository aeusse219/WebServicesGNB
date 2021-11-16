using System.Collections.Generic;
using WebServices.Entities.DTOs;
using WebServices.Entities.Models;

namespace WebServices.Application.Contracts
{
    public interface ITransactionApplication
    {
        IList<Transaction> GetAllTransactions();
        IList<TransactionBySkuDTO> GetListTransactionBySKU(string sku);
        IList<Rate> ValidatedRates(string to, IList<Transaction> ListTransactionByFilterSKU);
        Rate CalculeRate(string from, string to, IList<Rate> ListRates);
    }
}
