using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;

namespace Proiect_Implementare_Software.Repositories
{
    public class RideRepository : IRideRepository
    {
        private readonly AppDbContext _context;

        public RideRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ride>> GetRidesForUserAsync(int userId)
        {
            return await _context.Rides
                .Where(r => r.UserID == userId)
                .OrderBy(r => r.Date)
                .ToListAsync();
        }

        public async Task AddRideAsync(Ride ride)
        {
            _context.Rides.Add(ride);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Ride>> GetUpcomingRidesAsync()
        {
            var now = DateTime.Now;
            return await _context.Rides
                .Where(r => r.RideStatus == "Scheduled" && r.Date > now)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ride>> GetCompletedRidesAsync()
        {
            return await _context.Rides
                .Where(r => r.RideStatus == "Completed")
                .ToListAsync();
        }
    }
}
