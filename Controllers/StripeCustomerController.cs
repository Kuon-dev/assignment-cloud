using Microsoft.AspNetCore.Mvc;
using Cloud.Services;
using Cloud.Models;
using System;
using System.Threading.Tasks;

namespace Cloud.Controllers
{
    [ApiController]
    [Route("api/stripe-customers")]
    public class StripeCustomerController : ControllerBase
    {
        private readonly IStripeCustomerService _stripeCustomerService;

        public StripeCustomerController(IStripeCustomerService stripeCustomerService)
        {
            _stripeCustomerService = stripeCustomerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStripeCustomer([FromBody] StripeCustomerModel customer)
        {
            var createdCustomer = await _stripeCustomerService.CreateStripeCustomerAsync(customer);
            return CreatedAtAction(nameof(GetStripeCustomer), new { id = createdCustomer.Id }, createdCustomer);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStripeCustomer(Guid id)
        {
            var customer = await _stripeCustomerService.GetStripeCustomerAsync(id);
            if (customer == null)
                return NotFound();
            return Ok(customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStripeCustomer(Guid id, [FromBody] StripeCustomerModel customer)
        {
            if (id != customer.Id)
                return BadRequest();

            var updatedCustomer = await _stripeCustomerService.UpdateStripeCustomerAsync(customer);
            if (updatedCustomer == null)
                return NotFound();
            return Ok(updatedCustomer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStripeCustomer(Guid id)
        {
            var result = await _stripeCustomerService.DeleteStripeCustomerAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
