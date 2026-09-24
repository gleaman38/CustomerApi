using Microsoft.AspNetCore.Mvc;
using CustomerApi.Models;
using CustomerApi.DTOs;
using CustomerApi.Data;
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
    private readonly CustomerDbContext _context;
    public CustomersController(CustomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _context.Customers
            .Select(c=>new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                IsActive = c.IsActive
            }).ToListAsync();

        return Ok(customers);
    }

    
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

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

        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

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

        var foundRecord = await _context.Customers.FindAsync(id);

        if (foundRecord == null)
        {
            return NotFound();
        }

        foundRecord.FirstName = customerDto.FirstName;
        foundRecord.LastName = customerDto.LastName;
        foundRecord.Email = customerDto.Email;
        foundRecord.IsActive = customerDto.IsActive;

        await _context.SaveChangesAsync();

        var result = new CustomerDto
        {
            Id = foundRecord.Id,
            FirstName = foundRecord.FirstName,
            LastName = foundRecord.LastName,
            Email = foundRecord.Email,
            IsActive = foundRecord.IsActive
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<CustomerDto>> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        var result = new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            IsActive = customer.IsActive
        };

        return Ok(result);

    }
    
}
