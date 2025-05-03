using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Models;

namespace Proiect_Implementare_Software.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            var user = new Person
            {
                Username = "regele",
                FullName = "Sebastian-Valentin Sion",
                PhoneNumber = "0712345678",
                Avatar = "/images/default-avatar.png",
                Role = "Pasager",
                Rating = 4.8f
            };

            return View(user);
        }
    }
}
