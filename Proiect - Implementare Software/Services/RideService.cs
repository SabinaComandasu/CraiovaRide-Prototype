using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;

namespace Proiect_Implementare_Software.Services
{
    public class RideService : IRideService
    {
        private readonly AppDbContext _context;

        public RideService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Person?> GetPersonByIdentityUserIdAsync(string identityUserId)
        {
            return await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentityUserId == identityUserId);
        }

        public async Task CreateRideAsync(Ride ride)
        {
            _context.Rides.Add(ride);
            int affected = await _context.SaveChangesAsync();

            Console.WriteLine($"RideService: SaveChanges affected {affected} rows.");

            if (affected == 0)
            {
                throw new Exception("SaveChangesAsync returned 0. Ride not saved.");
            }
        }

        public async Task<List<Ride>> GetRidesForUserAsync(int userId)
        {
            return await _context.Rides
                .Where(r => r.UserID == userId)
                .Include(r => r.Product)
                .OrderBy(r => r.Date)
                .ToListAsync();
        }

        public IEnumerable<Ride> GetUpcomingRides()
        {
            return _context.Rides
                .Where(r => r.RideStatus == "Scheduled" && r.Date > DateTime.Now)
                .ToList();
        }
        public async Task UpdateRidesAsync(List<Ride> rides)
        {
            _context.Rides.UpdateRange(rides);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<Ride> GetCompletedRides()
        {
            return _context.Rides
                .Where(r => r.RideStatus == "Completed")
                .ToList();
        }

        public async Task CheckoutAsync(int userId)
        {
            var rides = await _context.Rides
                .Where(r => r.UserID == userId && r.RideStatus == "Scheduled")
                .ToListAsync();
            _context.Rides.RemoveRange(rides);
            await _context.SaveChangesAsync();
        }
    }
}
