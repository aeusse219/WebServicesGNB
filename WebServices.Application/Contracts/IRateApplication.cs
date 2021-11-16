using System.Collections.Generic;
using WebServices.Entities.Models;

namespace WebServices.Application.Contracts
{
    public interface IRateApplication
    {
        IList<Rate> GetAllRates();
    }
}
