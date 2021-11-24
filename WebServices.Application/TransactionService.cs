using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using WebServices.Application.Contracts;
using WebServices.Entities.DTOs;
using WebServices.Entities.Models;
using WebServices.Repository.Contracts;

namespace WebServices.Application
{
    public class TransactionService : ITransactionService
    {
        private readonly IGenericRepository<Transaction> _transaction;
        private readonly IGenericRepository<Rate> _rate;
        private readonly IConfiguration _configuration;

        public TransactionService(IGenericRepository<Transaction> transaction, IGenericRepository<Rate> rate, IConfiguration configuration)
        {
            _transaction = transaction;
            _rate = rate;
            _configuration = configuration;
        }

        public IList<Transaction> GetAllTransactions()
        {
            try
            {
                WebRequest request = WebRequest.Create(_configuration["urlGetTransactions"]);
                request.Method = "GET";
                request.ContentType = "application/json; charset=utf-8";
                WebResponse result = request.GetResponse();
                Stream stream = result.GetResponseStream();
                var reader = new StreamReader(stream);
                string jsonresult = reader.ReadToEnd();
                var deserializeJsonResult = JsonConvert.DeserializeObject<List<Transaction>>(jsonresult);

                if (deserializeJsonResult.Count > 0)
                {
                    _transaction.DeleteAll();
                    deserializeJsonResult.ForEach(item => {_transaction.Save(item);});
                    Log.Information("TransactionService, Method: GetAllTransactions, Se inserta en la BBDD local el listado de tarifas obtenido desde el webService y se retorna el listado de tarifas.");
                    return deserializeJsonResult;
                }
                else
                {
                    var transactions = _transaction.GetAll();
                    Log.Warning("TransactionService, Method: GetAllTransactions, Se consulta el listado de tarifas desde la BBDD Local, ya que no se obtieron registros desde el WebService.");
                    return transactions.ToList();
                }
            }
            catch (Exception ex)
            {
                var transactions = _transaction.GetAll();
                Log.Error("TransactionService, Method: GetAllTransactions, Se consulta el listado de tarifas desde la BBDD Local, ya que el WebService esta fallando. " + ex.Message);
                return transactions.ToList();
            }
        }

        public IList<TransactionBySkuDto> GetTransactionBySKU(string sku)
        {
            var resultTransactionFilterSKU = new List<TransactionBySkuDto>();
            try
            {
                string to = _configuration["convertTo"];
                var filterSKUTransactions = _transaction.GetAll().Where(x => x.Sku == sku).ToList();

                if (filterSKUTransactions.Count > 0)
                {
                    var listTransactionByFilterSKU = new List<TransactionDto>();
                    var listRates = ValidatedRates(to, filterSKUTransactions);
                    filterSKUTransactions.ForEach(item =>
                    {
                        var rateEUR = listRates.FirstOrDefault(x => x.From == item.Currency && x.To == to);
                        if (item.Currency == to)
                            listTransactionByFilterSKU.Add(new TransactionDto { Sku = item.Sku, Amount = decimal.Round(item.Amount, 2), Currency = item.Currency });
                        else
                            listTransactionByFilterSKU.Add(new TransactionDto { Sku = item.Sku, Amount = decimal.Round(item.Amount * rateEUR.rate, 2), Currency = rateEUR.To });
                    });
                    resultTransactionFilterSKU.Add(new TransactionBySkuDto { ListTransactions = listTransactionByFilterSKU, TotalAmount = decimal.Round(listTransactionByFilterSKU.Sum(s => s.Amount), 2) });
                    Log.Warning("TransactionService, Method: GetTransactionBySKU, Se retorna el listado de transaciones filtradas por el SKU: " + sku + " y la suma total en EUR");
                    return resultTransactionFilterSKU;
                }
                Log.Information("TransactionService, Method: GetTransactionBySKU, No hay transaciones asociadas al sku: " + sku + ", enviado.");
                return resultTransactionFilterSKU;
            }
            catch (Exception ex)
            {
                Log.Error("TransactionService, Method: GetTransactionBySKU, Error: " + ex.Message);
                return resultTransactionFilterSKU;
            }
        }

        public IList<Rate> ValidatedRates(string to, IList<Transaction> ListTransactionByFilterSKU)
        {
            var listRates = _rate.GetAll();
            try
            {
                var rateToEUR = listRates.Where(x => x.To == to).ToList();
                if (rateToEUR.Count == 1)
                {
                    var listFirstConvertToEUR = listRates.Where(x => x.To == rateToEUR.FirstOrDefault().From && x.From != to).ToList();
                    var calculeRate = CalculeRate(listFirstConvertToEUR.FirstOrDefault().From, to, listRates);
                    listRates.Add(new Rate { From = calculeRate.From, To = calculeRate.To, rate = decimal.Round(calculeRate.rate, 2) });
                }

                var groupByListCurrency = ListTransactionByFilterSKU.Where(x => x.Currency != to).GroupBy(g => g.Currency).ToList();
                groupByListCurrency.ForEach(item =>
                {
                    var rateEUR = listRates.FirstOrDefault(x => x.From == item.Key && x.To == to);
                    if (rateEUR == null)
                    {
                        var calculeRate = CalculeRate(item.Key, to, listRates);
                        listRates.Add(new Rate { From = calculeRate.From, To = calculeRate.To, rate = decimal.Round(calculeRate.rate, 2) });
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Information("TransactionService, Method: ValidatedRates, Ingreso al metodo de ValidatedRates para verificar las tarifas que no existen. " + ex.Message);
                return listRates;
            }
            return listRates;
        }

        public Rate CalculeRate(string from, string to, IList<Rate> ListRates)
        {
            var listConversionFrom = ListRates.Where(x => x.From == from).ToList();
            var listConversionTo = ListRates.Where(x => listConversionFrom.Select(y => y.To).Contains(x.From) && x.To == to).ToList();
            decimal resultRate = 0;
            if (listConversionTo.Any())
                resultRate = listConversionFrom.FirstOrDefault(x => listConversionTo.Select(y => y.From).Contains(x.To)).rate * listConversionTo.FirstOrDefault(x => listConversionFrom.Select(y => y.To).Contains(x.From)).rate;

            Log.Information("TransactionService, Method: CalculeRate, Ingreso al metodo de CalculeRate para calcular de " + from + " a " + to);
            return (new Rate { From = from, To = to, rate = resultRate });
        }
    }
}
