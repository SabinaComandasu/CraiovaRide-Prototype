using Proiect_Implementare_Software.Models;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Services
{
    public interface IAccountService
    {
        Task<Person?> GetUserAsync(int id);
        Task<IEnumerable<Person>> GetAllUsersAsync();
    }
}
