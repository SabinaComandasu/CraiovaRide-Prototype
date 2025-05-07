using Proiect_Implementare_Software.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Data
{
    public interface IAccountRepository
    {
        Task<Person?> GetUserByIdAsync(int id);
        Task<Person?> GetUserByIdentityUserIdAsync(string identityUserId);
        Task<IEnumerable<Person>> GetAllUsersAsync();
        Task SaveChangesAsync();
        void Delete(Person person);

    }
}
