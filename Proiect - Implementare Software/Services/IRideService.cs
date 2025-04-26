using System.Collections.Generic;
using Proiect_Implementare_Software.Models;

namespace Proiect_Implementare_Software.Services
{
    public interface IRideService
    {
        IEnumerable<Ride> GetUpcomingRides();
        IEnumerable<Ride> GetCompletedRides();
    }
}