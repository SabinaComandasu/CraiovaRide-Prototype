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

        // GET /Products/Index[?q=...]
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

        // GET /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var all = await _context.Products
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            var product = all.FirstOrDefault(p => p.ProductID == id);
            if (product == null) return NotFound();

            // Compute similar products via TF-IDF + Cosine Similarity
            // Text = Name + Category + Description for richer vectorization
            var similar = TfIdfSimilarity.GetSimilar(
                all,
                p => p.ProductID,
                p => $"{p.Name} {p.Category} {p.Description}",
                targetId: id,
                topN: 4);

            ViewBag.SimilarProducts = similar.Select(x => x.Item).ToList();
            return View(product);
        }

        // GET /Products/Autocomplete?q=...  (returns JSON for predictive search)
        [HttpGet]
        public async Task<IActionResult> Autocomplete(string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 1)
                return Json(new List<object>());

            var all = await _context.Products
                .Select(p => new { p.ProductID, p.Name, p.Icon })
                .ToListAsync();

            var names = all.Select(p => p.Name).ToList();

            var suggestions = TrigramAutocomplete.GetSuggestions(names, q, maxResults: 6);

            // Return id + name + icon so JS can link directly to Details page
            var result = all
                .Where(p => suggestions.Contains(p.Name))
                .OrderBy(p => suggestions.IndexOf(p.Name))
                .Select(p => new { p.ProductID, p.Name, p.Icon });

            return Json(result);
        }

        // GET /Products/DownloadPdf?fileName=...
        public IActionResult DownloadPdf(string fileName)
        {
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/pdf", fileName);
        }
    }
}
