using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Models;

public class RideRepository : IRideRepository
{
    private readonly AppDbContext _context;

    public RideRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddRideAsync(Ride ride)
    {
        await _context.Rides.AddAsync(ride);
        await _context.SaveChangesAsync();
    }
}
