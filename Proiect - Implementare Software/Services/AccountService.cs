using Proiect_Implementare_Software.Models;
using Proiect_Implementare_Software.Data;

namespace Proiect_Implementare_Software.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;

        public AccountService(IAccountRepository repo)
        {
            _repo = repo;
        }

        public async Task<Person?> GetUserAsync(int id)
        {
            return await _repo.GetUserByIdAsync(id);
        }

        public async Task<IEnumerable<Person>> GetAllUsersAsync()
        {
            return await _repo.GetAllUsersAsync();
        }
    }
}
