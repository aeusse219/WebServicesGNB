using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using WebServices.Entities.Models;
using WebServices.Repository.Contracts;

namespace WebServices.Application.Contracts
{
    public class RateAplication : IRateApplication
    {
        private readonly IGenericRepository<Rate> _rate;
        private readonly IConfiguration _configuration;
        public RateAplication(IGenericRepository<Rate> rate, IConfiguration configuration)
        {
            _rate = rate;
            _configuration = configuration;
        }

        //Method that allows get the list of rates
        public IList<Rate> GetAllRates()
        {
            try
            {
                WebRequest request = WebRequest.Create(_configuration["urlGetRates"]);
                request.Method = "GET";
                request.ContentType = "application/json; charset=utf-8";
                WebResponse result = request.GetResponse();
                Stream stream = result.GetResponseStream();

                var reader = new StreamReader(stream);
                string jsonresult = reader.ReadToEnd();
                var deserializeJsonResul = JsonConvert.DeserializeObject<List<Rate>>(jsonresult);

                if (deserializeJsonResul.Count > 0)
                {
                    _rate.DeleteAll();
                    foreach (var item in deserializeJsonResul)
                    {
                        _rate.Save(item);
                    }

                    Log.Information("RateAplication, Metodo: GetAllRates, Se inserta en la BBDD local el listado de tarifas obtenido desde el webService y se retorna el listado de tarifas.");
                    return deserializeJsonResul;
                }
                else
                {
                    var rates = _rate.GetAll();

                    Log.Warning("RateAplication, Metodo: GetAllRates, Se consulta el listado de tarifas desde la BBDD Local, ya que no se obtieron registros desde el WebService.");
                    return rates.ToList();
                }

            }
            catch (Exception ex)
            {
                var rates = _rate.GetAll();

                Log.Error("RateAplication, Metodo: GetAllRates, Se consulta el listado de tarifas desde la BBDD Local, ya que el WebService esta fallando. " + ex.Message);
                return rates.ToList();
            }
        }
    }
}
