using Proiect_Implementare_Software.Models;
using Microsoft.EntityFrameworkCore;

namespace Proiect_Implementare_Software.Data
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Person?> GetUserByIdAsync(int id)
        {
            return await _context.Persons.FindAsync(id);
        }

        public async Task<IEnumerable<Person>> GetAllUsersAsync()
        {
            return await _context.Persons.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
