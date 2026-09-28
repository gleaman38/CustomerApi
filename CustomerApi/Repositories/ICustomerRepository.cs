using CustomerApi.Models;

namespace CustomerApi.Repositories;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> AddAsync(Customer customer);
    Task<Customer?> UpdateAsync(Customer customer);
    Task<Customer?> DeleteAsync(int id);
}
