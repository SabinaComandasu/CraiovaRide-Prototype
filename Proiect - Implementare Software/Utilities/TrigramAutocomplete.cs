namespace Proiect_Implementare_Software.Utilities
{
    /// <summary>
    /// Autocomplete predictiv bazat pe trigrame și distanța Levenshtein.
    ///
    /// Referință: Manning, C., Raghavan, P., Schütze, H. (2008).
    /// "Introduction to Information Retrieval", Cambridge University Press, Cap. 3.
    ///
    /// Algoritm:
    ///   1. Se construiesc trigrame din query (fereastra de 3 caractere pe textul padded cu spații).
    ///   2. Fiecare candidat primește un scor compus din:
    ///      a) Prefix match  (+10 dacă candidatul începe cu query-ul)
    ///      b) Substring match (+5 dacă query-ul apare în candidat)
    ///      c) Dice coefficient pe trigrame * 3
    ///         Dice(A,B) = 2 * |A ∩ B| / (|A| + |B|)
    ///      d) (1 - Levenshtein normalizat) — tiebreaker pentru distanță minimă
    ///   3. Rezultatele cu scor > 0.1 sunt returnate sortate descrescător.
    /// </summary>
    public static class TrigramAutocomplete
    {
        /// <summary>
        /// Returnează primele <paramref name="maxResults"/> sugestii de autocomplete
        /// pentru <paramref name="query"/> din lista <paramref name="candidates"/>.
        /// </summary>
        public static List<string> GetSuggestions(
            IEnumerable<string> candidates,
            string query,
            int maxResults = 6)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            string normQuery = Normalize(query.Trim());
            var queryTrigrams = GetTrigrams(normQuery);

            return candidates
                .Select(candidate =>
                {
                    string normCand = Normalize(candidate);

                    bool isPrefix   = normCand.StartsWith(normQuery, StringComparison.Ordinal);
                    bool hasTerm    = normCand.Contains(normQuery,   StringComparison.Ordinal);

                    var   candTrigrams   = GetTrigrams(normCand);
                    double trigramScore  = DiceCoefficient(queryTrigrams, candTrigrams);

                    double levenNorm = normQuery.Length == 0 && normCand.Length == 0
                        ? 0.0
                        : LevenshteinDistance(normQuery, normCand)
                          / (double)Math.Max(normQuery.Length, normCand.Length);

                    double score = (isPrefix ? 10.0 : 0.0)
                                 + (hasTerm  ?  5.0 : 0.0)
                                 + trigramScore * 3.0
                                 + (1.0 - levenNorm);

                    return (Candidate: candidate, Score: score);
                })
                .Where(x => x.Score > 0.1)
                .OrderByDescending(x => x.Score)
                .Take(maxResults)
                .Select(x => x.Candidate)
                .ToList();
        }

        // ------------------------------------------------------------------ //

        /// <summary>Construieste multimea trigramelor pentru un text (padded cu spatii).</summary>
        private static HashSet<string> GetTrigrams(string text)
        {
            var set = new HashSet<string>();
            string p = " " + text + " ";
            for (int i = 0; i + 2 < p.Length; i++)
                set.Add(p.Substring(i, 3));
            return set;
        }

        /// <summary>Dice coefficient: 2|A∩B| / (|A|+|B|)</summary>
        private static double DiceCoefficient(HashSet<string> a, HashSet<string> b)
        {
            if (a.Count == 0 || b.Count == 0) return 0.0;
            int common = a.Count(t => b.Contains(t));
            return 2.0 * common / (a.Count + b.Count);
        }

        /// <summary>Distanța Levenshtein clasică (DP O(mn)).</summary>
        private static int LevenshteinDistance(string s, string t)
        {
            int m = s.Length, n = t.Length;
            var d = new int[m + 1, n + 1];
            for (int i = 0; i <= m; i++) d[i, 0] = i;
            for (int j = 0; j <= n; j++) d[0, j] = j;
            for (int i = 1; i <= m; i++)
                for (int j = 1; j <= n; j++)
                    d[i, j] = s[i - 1] == t[j - 1]
                        ? d[i - 1, j - 1]
                        : 1 + Math.Min(d[i - 1, j - 1],
                              Math.Min(d[i - 1, j], d[i, j - 1]));
            return d[m, n];
        }

        private static string Normalize(string s) =>
            s.Replace('ă', 'a').Replace('Ă', 'A')
             .Replace('â', 'a').Replace('Â', 'A')
             .Replace('î', 'i').Replace('Î', 'I')
             .Replace('ș', 's').Replace('Ș', 'S')
             .Replace('ş', 's').Replace('Ş', 'S')
             .Replace('ț', 't').Replace('Ț', 'T')
             .Replace('ţ', 't').Replace('Ţ', 'T')
             .ToLowerInvariant();
    }
}
