using Proiect_Implementare_Software.Repositories;

namespace Proiect_Implementare_Software.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _homeRepository;

        public HomeService(IHomeRepository homeRepository)
        {
            _homeRepository = homeRepository;
        }

        public string GetWelcomeMessage()
        {
            return _homeRepository.GetWelcomeMessage();
        }
    }
}
