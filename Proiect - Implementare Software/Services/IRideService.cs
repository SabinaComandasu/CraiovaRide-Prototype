using Proiect_Implementare_Software.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proiect_Implementare_Software.Services
{
    public interface IRideService
    {
        IEnumerable<Ride> GetUpcomingRides();
        IEnumerable<Ride> GetCompletedRides();

        Task<List<Ride>> GetRidesForUserAsync(int userId);
        Task CreateRideAsync(Ride ride);
        Task UpdateRidesAsync(List<Ride> rides);

        Task<Person?> GetPersonByIdentityUserIdAsync(string identityUserId); // 👈 Add this
    }
}
