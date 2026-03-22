using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_Implementare_Software.Data;
using Proiect_Implementare_Software.Utilities;

namespace Proiect_Implementare_Software.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var all = await _context.Products
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            List<Models.Product> display;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var ranked = BM25Scorer.Rank(all, p => p.Name, q);
                display = ranked
                    .Where(r => r.Score > 0)
                    .Select(r => r.Item)
                    .ToList();
            }
            else
            {
                display = all;
            }

            ViewBag.SearchQuery = q ?? string.Empty;
            ViewBag.IsFiltered  = !string.IsNullOrWhiteSpace(q);
            return View(display);
        }

        public IActionResult DownloadPdf(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/pdf", fileName);
        }
    }
}
