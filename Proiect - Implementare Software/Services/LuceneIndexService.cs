using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Proiect_Implementare_Software.Models;
using UglyToad.PdfPig;

namespace Proiect_Implementare_Software.Services
{
    /// <summary>
    /// Result returned by a Lucene search query.
    /// </summary>
    public class LuceneSearchResult
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Category { get; set; } = "";
        /// <summary>First ~250 chars extracted from the specification PDF.</summary>
        public string Snippet { get; set; } = "";
        /// <summary>Raw Lucene TF-IDF relevance score for this document.</summary>
        public float Score { get; set; }
    }

    /// <summary>
    /// Singleton service that builds and queries an in-memory Lucene index
    /// over product specification PDFs (fișe tehnice).
    ///
    /// Scoring model: Lucene Classic Similarity (TF-IDF).
    ///   score(q,d) = coord(q,d) · queryNorm(q) · Σ[ tf(t,d) · idf(t)² · norm(t,d) ]
    ///   tf(t,d)   = √freq           (term frequency in doc)
    ///   idf(t)    = 1 + log( N / (df+1) )  (inverse document frequency)
    ///   norm(t,d) = 1 / √numTerms   (shorter fields score higher)
    /// </summary>
    public class LuceneIndexService
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        private RAMDirectory? _directory;
        private readonly IWebHostEnvironment _env;
        private readonly object _lock = new();

        public LuceneIndexService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ── Index construction ───────────────────────────────────────────────

        /// <summary>
        /// Indexes all products. Each document contains:
        ///   id, icon, name, category, description — stored fields
        ///   pdfContent — indexed only (full text extracted from the PDF)
        ///   snippet    — first 250 chars of PDF text, stored for display
        /// </summary>
        public void BuildIndex(IEnumerable<Product> products)
        {
            var dir = new RAMDirectory();
            var analyzer = new StandardAnalyzer(AppLuceneVersion);
            var config = new IndexWriterConfig(AppLuceneVersion, analyzer)
            {
                OpenMode = OpenMode.CREATE
            };

            using var writer = new IndexWriter(dir, config);

            foreach (var product in products)
            {
                var pdfText = ExtractPdfText(product.PdfPath);

                var doc = new Document
                {
                    new StringField("id",          product.ProductID.ToString(), Field.Store.YES),
                    new StoredField ("icon",        product.Icon),
                    new TextField   ("name",        product.Name,        Field.Store.YES),
                    new TextField   ("category",    product.Category,    Field.Store.YES),
                    new TextField   ("description", product.Description, Field.Store.YES),
                    new StoredField ("snippet",     Truncate(pdfText, 250)),
                    new TextField   ("pdfContent",  pdfText,             Field.Store.NO),
                };

                writer.AddDocument(doc);
            }

            writer.Flush(triggerMerge: false, applyAllDeletes: false);

            lock (_lock)
            {
                _directory?.Dispose();
                _directory = dir;
            }
        }

        // ── PDF text extraction ──────────────────────────────────────────────

        private string ExtractPdfText(string pdfPath)
        {
            if (string.IsNullOrEmpty(pdfPath)) return "";

            var fullPath = Path.Combine(_env.WebRootPath, pdfPath.TrimStart('/'));
            if (!File.Exists(fullPath)) return "";

            try
            {
                var sb = new System.Text.StringBuilder();
                using var doc = PdfDocument.Open(fullPath);
                foreach (var page in doc.GetPages())
                    sb.AppendLine(page.Text);
                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        // ── Search ───────────────────────────────────────────────────────────

        /// <summary>
        /// Searches the index across name, category, description and pdfContent.
        /// Results are sorted by Lucene score (descending by default).
        /// </summary>
        public List<LuceneSearchResult> Search(string query, bool ascending = false)
        {
            if (_directory == null || string.IsNullOrWhiteSpace(query))
                return [];

            try
            {
                using var reader = DirectoryReader.Open(_directory);
                var searcher = new IndexSearcher(reader);
                var analyzer = new StandardAnalyzer(AppLuceneVersion);

                var parser = new MultiFieldQueryParser(
                    AppLuceneVersion,
                    new[] { "name", "category", "description", "pdfContent" },
                    analyzer)
                {
                    DefaultOperator = QueryParserBase.OR_OPERATOR
                };

                Query luceneQuery;
                try   { luceneQuery = parser.Parse(query); }
                catch { luceneQuery = parser.Parse(QueryParserBase.Escape(query)); }

                var hits = searcher.Search(luceneQuery, 100);

                var scoreDocs = ascending
                    ? hits.ScoreDocs.OrderBy(x => x.Score).ToArray()
                    : hits.ScoreDocs.OrderByDescending(x => x.Score).ToArray();

                var results = new List<LuceneSearchResult>();
                foreach (var hit in scoreDocs)
                {
                    var doc = searcher.Doc(hit.Doc);
                    results.Add(new LuceneSearchResult
                    {
                        ProductId = int.Parse(doc.Get("id")),
                        Icon      = doc.Get("icon") ?? "",
                        Name      = doc.Get("name") ?? "",
                        Category  = doc.Get("category") ?? "",
                        Snippet   = doc.Get("snippet") ?? "",
                        Score     = hit.Score
                    });
                }

                return results;
            }
            catch
            {
                return [];
            }
        }

        public bool IsIndexBuilt => _directory != null;

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string Truncate(string text, int max)
        {
            var clean = text.Replace('\n', ' ').Replace('\r', ' ');
            return clean.Length > max ? clean[..max] + "…" : clean;
        }
    }
}
