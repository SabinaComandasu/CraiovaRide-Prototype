using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Ride> GetUpcomingRides()
        {

            return _context.Rides.Where(r => r.RideStatus == "Scheduled").ToList();
        }

        public IEnumerable<Ride> GetCompletedRides()
        {
    
            return _context.Rides.Where(r => r.RideStatus == "Completed").ToList();
        }
    }
}