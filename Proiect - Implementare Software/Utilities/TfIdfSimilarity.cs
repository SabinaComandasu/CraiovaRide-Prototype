namespace Proiect_Implementare_Software.Utilities
{
    /// <summary>
    /// Recomandări de produse similare folosind TF-IDF + Cosine Similarity.
    ///
    /// Referință: Salton, G. &amp; McGill, M.J. (1983).
    /// "Introduction to Modern Information Retrieval." McGraw-Hill.
    ///
    /// Algoritm:
    ///   1. Fiecare produs este reprezentat ca un vector TF-IDF în spațiul termenilor.
    ///      - TF augmentat: tf(t,d) = 0.5 + 0.5 * count(t,d) / max_count(d)
    ///        (evita dominarea termenilor frecventi)
    ///      - IDF smoothed: idf(t) = ln((N+1) / (df(t)+1)) + 1
    ///        (smoothing Laplace pentru termeni rari)
    ///   2. Similaritatea cosinus între doi vectori A și B:
    ///      cos(A,B) = (A · B) / (||A|| * ||B||)
    ///   3. Produsele sunt ordonate descrescător după similaritate.
    /// </summary>
    public static class TfIdfSimilarity
    {
        /// <summary>
        /// Returnează primele <paramref name="topN"/> produse similare cu cel identificat
        /// prin <paramref name="targetId"/>. Textul folosit pentru vectorizare combină
        /// Name + Category + Description pentru a captura mai bine semantica.
        /// </summary>
        public static List<(T Item, double Similarity)> GetSimilar<T>(
            IReadOnlyList<T> corpus,
            Func<T, int>    getId,
            Func<T, string> getText,
            int targetId,
            int topN = 4)
        {
            if (corpus.Count == 0)
                return new List<(T, double)>();

            // 1. Tokenize all documents
            var tokenized = corpus.Select(item => Tokenize(getText(item))).ToList();
            int N = corpus.Count;

            // 2. Compute IDF for every term in the vocabulary
            var vocab = tokenized.SelectMany(d => d).Distinct().ToArray();
            var idf   = new Dictionary<string, double>(vocab.Length);
            foreach (var term in vocab)
            {
                int df   = tokenized.Count(d => d.Contains(term));
                idf[term] = Math.Log((N + 1.0) / (df + 1.0)) + 1.0;  // smoothed
            }

            // 3. Build augmented TF-IDF vector for each document
            var vectors = tokenized.Select(doc =>
            {
                var vec      = new Dictionary<string, double>();
                var termFreq = doc.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
                int maxTf    = termFreq.Values.DefaultIfEmpty(1).Max();

                foreach (var (term, count) in termFreq)
                {
                    double tf  = 0.5 + 0.5 * count / maxTf;
                    vec[term]  = tf * idf.GetValueOrDefault(term, 0.0);
                }
                return vec;
            }).ToList();

            // 4. Find the target product's vector
            int targetIdx = corpus
                .Select((item, idx) => (item, idx))
                .FirstOrDefault(x => getId(x.item) == targetId).idx;

            // If targetId not found (default 0 might be valid), do explicit check
            bool found = corpus.Any(item => getId(item) == targetId);
            if (!found) return new List<(T, double)>();

            var targetVec = vectors[targetIdx];

            // 5. Compute cosine similarity with all other products
            return corpus
                .Select((item, idx) =>
                {
                    if (getId(item) == targetId) return (item, -1.0);   // exclude self
                    double sim = CosineSimilarity(targetVec, vectors[idx]);
                    return (item, sim);
                })
                .Where(x => x.Item2 >= 0)
                .OrderByDescending(x => x.Item2)
                .Take(topN)
                .ToList();
        }

        // ------------------------------------------------------------------ //

        private static double CosineSimilarity(
            Dictionary<string, double> a,
            Dictionary<string, double> b)
        {
            double dot = 0.0;
            foreach (var (term, valA) in a)
                if (b.TryGetValue(term, out double valB))
                    dot += valA * valB;

            double normA = Math.Sqrt(a.Values.Sum(v => v * v));
            double normB = Math.Sqrt(b.Values.Sum(v => v * v));
            if (normA == 0 || normB == 0) return 0.0;
            return dot / (normA * normB);
        }

        private static string[] Tokenize(string text) =>
            NormalizeDiacritics(text)
                .ToLowerInvariant()
                .Split(new[] { ' ', '-', '_', '/', ',', '.', '(', ')', '–', '—', '+' },
                       StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 1)
                .ToArray();

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
