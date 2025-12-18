using Inventory.Api.Domain.Entities;

namespace Inventory.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string email, string password);
        Task<String> LoginAsync(string email, string password);
    }
}
