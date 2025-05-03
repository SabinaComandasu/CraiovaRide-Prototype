using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Services;

namespace Proiect_Implementare_Software.Controllers
{
    public class RidesController : Controller
    {
        private readonly IRideService _rideService;

        public RidesController(IRideService rideService)
        {
            _rideService = rideService;
        }

        public IActionResult Index()
        {
            // Vom folosi serviciul pentru a aduce ride-urile în viitor
            // Deocamdată întoarcem doar view-ul gol
            return View();
        }
    }
}