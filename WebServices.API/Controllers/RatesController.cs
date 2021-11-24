using Microsoft.AspNetCore.Mvc;
using WebServices.Application.Contracts;

namespace WebServices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly IRateService _rate;
        public RatesController(IRateService rate)
        {
            _rate = rate;
        }
        
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_rate.GetAllRates());
        }
    }
}
