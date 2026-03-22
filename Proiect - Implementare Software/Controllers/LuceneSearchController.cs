using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proiect_Implementare_Software.Services;

namespace Proiect_Implementare_Software.Controllers
{
    [Authorize]
    public class LuceneSearchController : Controller
    {
        private readonly LuceneIndexService _lucene;

        public LuceneSearchController(LuceneIndexService lucene)
        {
            _lucene = lucene;
        }

        // GET /LuceneSearch?q=...&sort=asc|desc
        public IActionResult Index(string? q, string sort = "desc")
        {
            var ascending = sort == "asc";
            var results = string.IsNullOrWhiteSpace(q)
                ? new List<LuceneSearchResult>()
                : _lucene.Search(q, ascending);

            ViewBag.Query       = q ?? "";
            ViewBag.Sort        = sort;
            ViewBag.ResultCount = results.Count;

            return View(results);
        }
    }
}
