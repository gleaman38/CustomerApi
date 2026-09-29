using CustomerApi.DTOs;
using CustomerApi.Models;
using CustomerApi.Repositories;

namespace CustomerApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository; 
        }

        public async Task<CustomerDto?> GetByIdAsync(int id)
        {
            var customer = await _repository.GetByIdAsync(id);

            if (customer == null)
            {
                return null;
            }

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                IsActive = customer.IsActive
            };

            return new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                IsActive = customer.IsActive
            };
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _repository.GetAllAsync();

            var customerDtos = customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                IsActive = c.IsActive
            });

            return customerDtos;
        }

        public async Task<CustomerDto> AddAsync(CreateCustomerDto customerDto)
        {
            // Map CreateCustomerDto to Customer entity
            var customer = new Customer
            {
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email,
                IsActive = customerDto.IsActive
            };

            // Add customer to the database
            var createdCustomer = await _repository.AddAsync(customer);

            // Map created customer to CustomerDto
            return new CustomerDto
            {
                Id = createdCustomer.Id,
                FirstName = createdCustomer.FirstName,
                LastName = createdCustomer.LastName,
                Email = createdCustomer.Email,
                IsActive = createdCustomer.IsActive
            };

        }

        public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto customerDto)
        {
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
                return null;
            }

            return new CustomerDto
            {
                Id = updatedCustomer.Id,
                FirstName = updatedCustomer.FirstName,
                LastName = updatedCustomer.LastName,
                Email = updatedCustomer.Email,
                IsActive = updatedCustomer.IsActive
            };
        }

        public async Task<CustomerDto?> DeleteAsync(int id)
        {
            var deletedCustomer = await _repository.DeleteAsync(id);

            if (deletedCustomer == null)
            {
                return null;
            }

            return new CustomerDto
            {
                Id = deletedCustomer.Id,
                FirstName = deletedCustomer.FirstName,
                LastName = deletedCustomer.LastName,
                Email = deletedCustomer.Email,
                IsActive = deletedCustomer.IsActive
            };
        }
    }
}
