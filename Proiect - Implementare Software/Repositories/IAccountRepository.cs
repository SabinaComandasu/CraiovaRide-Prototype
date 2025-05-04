using Proiect_Implementare_Software.Models;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Data
{
    public interface IAccountRepository
    {
        Task<Person?> GetUserByIdAsync(int id);
        Task<IEnumerable<Person>> GetAllUsersAsync();
        Task SaveChangesAsync();
    }
}
