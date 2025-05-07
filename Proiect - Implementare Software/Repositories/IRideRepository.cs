using Proiect_Implementare_Software.Models;

public interface IRideRepository
{
    Task AddRideAsync(Ride ride);
}
