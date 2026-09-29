using CustomerApi.DTOs;
using CustomerApi.Models;
namespace CustomerApi.Services
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto> AddAsync(CreateCustomerDto customer);
        Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto customerDto);
        Task<CustomerDto?> DeleteAsync(int id);
    }
}
 
