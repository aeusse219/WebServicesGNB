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
    public class TransactionApplication : ITransactionApplication
    {
        private readonly IGenericRepository<Transaction> _transaction;
        private readonly IGenericRepository<Rate> _rate;
        private readonly IConfiguration _configuration;

        public TransactionApplication(IGenericRepository<Transaction> transaction, IGenericRepository<Rate> rate, IConfiguration configuration)
        {
            _transaction = transaction;
            _rate = rate;
            _configuration = configuration;
        }

        //Method that allows get the list of Transactions
        public IList<Transaction> GetAllTransactions()
        {
            try
            {
                WebRequest request = WebRequest.Create(_configuration["urlGetTransactions"]);
                request.Method = "GET";
                request.ContentType = "application/json; charset=utf-8";
                WebResponse result = request.GetResponse();
                using (Stream stream = result.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    string jsonresult = reader.ReadToEnd();
                    var deserializeJsonResul = JsonConvert.DeserializeObject<IList<Transaction>>(jsonresult);

                    if (deserializeJsonResul.Count > 0)
                    {
                        _transaction.DeleteAll();

                        foreach (var item in deserializeJsonResul)
                        {
                            _transaction.Save(item);
                        }

                        Log.Information("TransactionApplication, Metodo: GetAllTransactions, Se inserta en la BBDD local el listado de tarifas obtenido desde el webService y se retorna el listado de tarifas.");
                        return deserializeJsonResul;
                    }
                    else
                    {
                        var transactions = _transaction.GetAll();

                        Log.Warning("TransactionApplication, Metodo: GetAllTransactions, Se consulta el listado de tarifas desde la BBDD Local, ya que no se obtieron registros desde el WebService.");
                        return transactions.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                var transactions = _transaction.GetAll();

                Log.Error("TransactionApplication, Metodo: GetAllTransactions, Se consulta el listado de tarifas desde la BBDD Local, ya que el WebService esta fallando. " + ex.Message);
                return transactions.ToList();
            }
        }

        //Method that allows get the list of transactions by filter SKU
        public IList<TransactionBySkuDTO> GetListTransactionBySKU(string sku)
        {
            var resultTransactionFilterSKU = new List<TransactionBySkuDTO>();
            try
            {
                string to = "EUR";
                var filterSKUtTansactions = _transaction.GetAll().Where(x => x.sku == sku).ToList();

                if (filterSKUtTansactions.Count > 0)
                {
                    var listTransactionByFilterSKU = new List<TransactionDTO>();
                    var listRates = ValidatedRates(to, filterSKUtTansactions);

                    foreach (var item in filterSKUtTansactions)
                    {
                        var rateEUR = listRates.FirstOrDefault(x => x.from == item.currency && x.to == to);

                        if (item.currency == to)
                        {
                            listTransactionByFilterSKU.Add(new TransactionDTO { sku = item.sku, amount = (Math.Round(item.amount, 2)), currency = item.currency });
                        }
                        else
                        {
                            listTransactionByFilterSKU.Add(new TransactionDTO { sku = item.sku, amount = (Math.Round((item.amount * rateEUR.rate), 2)), currency = rateEUR.to });
                        }
                    }
                    resultTransactionFilterSKU.Add(new TransactionBySkuDTO { ListTransactions = listTransactionByFilterSKU, TotalAmount = listTransactionByFilterSKU.Sum(s => s.amount) });

                    Log.Warning("TransactionApplication, Metodo: GetListTransactionBySKU, Se retorna el listado de transaciones filtradas por el SKU: " + sku + " y la suma total en EUR");
                    return resultTransactionFilterSKU;
                }

                Log.Information("TransactionApplication, Metodo: GetListTransactionBySKU, No hay transaciones asociadas al sku: " + sku + ", enviado.");
                return resultTransactionFilterSKU;

            }
            catch (Exception ex)
            {
                Log.Error("TransactionApplication, Metodo: GetListTransactionBySKU, Error: " + ex.Message);
                return resultTransactionFilterSKU;
            }
        }

        //Method to validated rates
        public IList<Rate> ValidatedRates(string to, IList<Transaction> ListTransactionByFilterSKU)
        {
            var listRates = _rate.GetAll().ToList();
            try
            {
                var groupByListCurrency = ListTransactionByFilterSKU.Where(x => x.currency != to).GroupBy(g => g.currency).ToList();

                foreach (var item in groupByListCurrency)
                {
                    var rateEUR = listRates.FirstOrDefault(x => x.from == item.Key && x.to == to);
                    if (rateEUR == null)
                    {
                        var calculeRate = CalculeRate(item.Key, to, listRates);
                        listRates.Add(new Rate { from = calculeRate.from, to = calculeRate.to, rate = Math.Round((calculeRate.rate), 2) });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Information("TransactionApplication, Metodo: ValidatedRates, Ingreso al metodo de ValidatedRates para verificar las tarifas que no existen. " + ex.Message);
                return listRates;
            }
            return listRates;
        }

        //Method to calculate auto currency conversion EUR
        public Rate CalculeRate(string from, string to, IList<Rate> ListRates)
        {
            var listConversionFrom = ListRates.Where(x => x.from == from).ToList();
            var listConversionTo = ListRates.Where(x => listConversionFrom.Select(y => y.to).Contains(x.from) && x.to == to).ToList();
            decimal resultRate = 0;
            if (listConversionTo.Any())
            {
                resultRate = listConversionFrom.FirstOrDefault(x => listConversionTo.Select(y => y.from).Contains(x.to)).rate * listConversionTo.FirstOrDefault(x => listConversionFrom.Select(y => y.to).Contains(x.from)).rate;
            }

            Log.Information("TransactionApplication, Metodo: CalculeRate, Ingreso al metodo de CalculeRate para calcular de " + from + " a " + to);
            return (new Rate { from = from, to = to, rate = resultRate });
        }
    }
}
