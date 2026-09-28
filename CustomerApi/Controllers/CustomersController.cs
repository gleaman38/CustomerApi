using Microsoft.AspNetCore.Mvc;
using CustomerApi.Models;
using CustomerApi.DTOs;
using CustomerApi.Data;
using CustomerApi.Repositories;
using System.Collections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;

namespace CustomerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    public CustomersController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _repository.GetAllAsync();

        var customerDtos = customers.Select(c=>new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                IsActive = c.IsActive
            });

        return Ok(customers);
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        if (id <= 0)
        {
            return NotFound("Invalid customer id");
        }

        var customer = await _repository.GetByIdAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            IsActive = customer.IsActive
        };

        return Ok(customerDto);
    }

    
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto customerDto)
    {
        var customer = new Customer
        {
            FirstName = customerDto.FirstName,
            LastName = customerDto.LastName,
            Email = customerDto.Email,
            IsActive = customerDto.IsActive
        };

        var createdCustomer = await _repository.AddAsync(customer);

        var result = new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            IsActive = customer.IsActive
        };

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id },
            result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> EditCustomer(int id, UpdateCustomerDto customerDto)
    {

        if (id <= 0)
        {
            return BadRequest("Invalid customer id");
        }

        var customer = new Customer
        {
            Id = id,
            FirstName = customerDto.FirstName,
            LastName = customerDto.LastName,
            Email = customerDto.Email,
            IsActive = customerDto.IsActive
        };

        var updatedCustomer = await _repository.UpdateAsync(customer);

        if (updatedCustomer == null)
        {
            return NotFound();
        }

        var result = new CustomerDto
        {
            Id = updatedCustomer.Id,
            FirstName = updatedCustomer.FirstName,
            LastName = updatedCustomer.LastName,
            Email = updatedCustomer.Email,
            IsActive = updatedCustomer.IsActive
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<CustomerDto>> DeleteCustomer(int id)
    {

        if (id <= 0)
        {
            return BadRequest("Invalid customer id");
        }

        var deletedCustomer = await _repository.DeleteAsync(id);

        if (deletedCustomer == null)
        {
            return NotFound();
        }

        var result = new CustomerDto
        {
            Id = deletedCustomer.Id,
            FirstName = deletedCustomer.FirstName,
            LastName = deletedCustomer.LastName,
            Email = deletedCustomer.Email,
            IsActive = deletedCustomer.IsActive
        };

        return Ok(result);

    }
    
}
