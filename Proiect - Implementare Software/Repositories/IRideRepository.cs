using System.Collections.Generic;
using System.Threading.Tasks;
using Proiect_Implementare_Software.Models;

namespace Proiect_Implementare_Software.Repositories
{
public interface IRideRepository
{
        Task<List<Ride>> GetRidesForUserAsync(int userId);
    Task AddRideAsync(Ride ride);
        Task<IEnumerable<Ride>> GetUpcomingRidesAsync();
        Task<IEnumerable<Ride>> GetCompletedRidesAsync();
    }
}
