namespace Proiect_Implementare_Software.Utilities
{
    /// <summary>
    /// Simplified BM25 (Best Match 25) ranking algorithm.
    /// Referinta: Robertson, S.E. &amp; Zaragoza, H. (2009).
    /// "The Probabilistic Relevance Framework: BM25 and Beyond."
    /// Foundations and Trends in Information Retrieval, 3(4), 333-389.
    ///
    /// Formula:
    ///   score(D, Q) = Σ IDF(qi) * [ f(qi,D) * (k1+1) ] / [ f(qi,D) + k1*(1 - b + b*|D|/avgdl) ]
    ///
    /// Parametri standard:
    ///   k1 = 1.5  (saturatie frecventa termeni)
    ///   b  = 0.75 (normalizare lungime document)
    /// </summary>
    public static class BM25Scorer
    {
        private const double K1 = 1.5;
        private const double B  = 0.75;

        /// <summary>
        /// Rankeaza elementele din <paramref name="corpus"/> in functie de
        /// relevanta fata de <paramref name="query"/> folosind algoritmul BM25.
        /// Returneaza lista sortata descrescator dupa scor; elementele cu scor 0
        /// (fara niciun termen comun) sunt excluse cand <paramref name="query"/> nu e gol.
        /// </summary>
        public static List<(T Item, double Score)> Rank<T>(
            IReadOnlyList<T> corpus,
            Func<T, string> getText,
            string query)
        {
            // Tokenizeaza toate documentele
            var tokenizedDocs = corpus
                .Select(item => Tokenize(getText(item)))
                .ToList();

            var queryTerms = Tokenize(query).Distinct().ToArray();

            int N = corpus.Count;
            double avgdl = tokenizedDocs.Count == 0
                ? 1.0
                : tokenizedDocs.Average(d => (double)d.Length);

            var results = new List<(T Item, double Score)>(N);

            for (int i = 0; i < N; i++)
            {
                string[] docTokens = tokenizedDocs[i];
                int dl = docTokens.Length;
                double score = 0.0;

                foreach (string term in queryTerms)
                {
                    int tf = docTokens.Count(t => t == term);

                    int df = tokenizedDocs.Count(d => d.Contains(term));

                    if (df == 0) continue;

                    double idf = Math.Log((N - df + 0.5) / (df + 0.5) + 1.0);

                    double termScore = idf
                        * (tf * (K1 + 1.0))
                        / (tf + K1 * (1.0 - B + B * dl / avgdl));

                    score += termScore;
                }

                results.Add((corpus[i], score));
            }

            return results
                .OrderByDescending(r => r.Score)
                .ToList();
        }

        // ------------------------------------------------------------------ //

        private static string[] Tokenize(string text) =>
            NormalizeDiacritics(text)
                .ToLowerInvariant()
                .Split(new[] { ' ', '-', '_', '/', ',', '.', '(', ')', '–', '—', '+' },
                       StringSplitOptions.RemoveEmptyEntries);

        private static string NormalizeDiacritics(string s) =>
            s.Replace('ă', 'a').Replace('Ă', 'A')
             .Replace('â', 'a').Replace('Â', 'A')
             .Replace('î', 'i').Replace('Î', 'I')
             .Replace('ș', 's').Replace('Ș', 'S')
             .Replace('ş', 's').Replace('Ş', 'S')
             .Replace('ț', 't').Replace('Ț', 'T')
             .Replace('ţ', 't').Replace('Ţ', 'T');
    }
}
