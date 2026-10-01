using CustomerApi.DTOs;
using CustomerApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;
    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _service.GetAllAsync();

        return Ok(customers);
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        if (id <= 0)
        {
            return NotFound("Invalid customer id");
        }

        var customerDto = await _service.GetByIdAsync(id);

        if (customerDto == null)
        {
            return NotFound();
        }

        return Ok(customerDto);
    }

    
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto customerDto)
    {
        var createdCustomerDto = await _service.AddAsync(customerDto);

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = createdCustomerDto.Id },
            createdCustomerDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> EditCustomer(int id, UpdateCustomerDto customerDto)
    {

        if (id <= 0)
        {
            return BadRequest("Invalid customer id");
        }

        var updatedCustomerDto = await _service.UpdateAsync(id, customerDto);

        if (updatedCustomerDto == null)
        {
            return NotFound();
        }

        return Ok(updatedCustomerDto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<CustomerDto>> DeleteCustomer(int id)
    {

        if (id <= 0)
        {
            return BadRequest("Invalid customer id");
        }

        var deletedCustomerDto = await _service.DeleteAsync(id);

        if (deletedCustomerDto == null)
        {
            return NotFound();
        }

        return Ok(deletedCustomerDto);

    }
    
}
