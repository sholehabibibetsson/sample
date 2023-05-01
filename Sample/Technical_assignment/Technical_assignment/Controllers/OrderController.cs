using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Technical_assignment.Contracts;
using Technical_assignment.Services;

namespace Technical_assignment.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        public OrderController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpPost, Route("DeliveryDates/{postalCode}/")]
        public async Task<IActionResult> DeliveryDates(string postalCode, [FromBody, Required] List<Product> products)
        {
            try
            {
                var result = await _deliveryService.CheckDeliveryDates(postalCode, products);
                if (result.Any() == false)
                {
                    return Ok("Unfortunately there is no delivery dates available that all your products fit into.");
                }

                return Ok(result);
            }
            catch (ProductsDuplicateException ex)
            {
                return  BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [HttpGet, Route("/GetDofConnectionString")]
        public IActionResult GetDofConnectionString()
        {
            return Ok(Settings.SqlConnectionStrings.Dof);
        }
    }
}