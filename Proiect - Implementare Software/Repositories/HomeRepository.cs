namespace Proiect_Implementare_Software.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        public string GetWelcomeMessage()
        {
            return "Welcome to CraiovaRide!";
        }
    }
}
