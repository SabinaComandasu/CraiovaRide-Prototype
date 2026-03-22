using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace Proiect_Implementare_Software.Utilities
{
    /// <summary>
    /// Generates minimal valid PDF files without external PDF libraries.
    /// Body lines prefixed with "B:" are rendered in bold (Helvetica-Bold).
    /// </summary>
    public static class SimplePdfWriter
    {
        private const float PageW = 595f;
        private const float PageH = 842f;
        private const float MarginL = 50f;
        private const float MarginR = 50f;
        private const float LineH = 17f;
        private const int LogoDisplaySize = 90; // points

        public static byte[] CreatePdf(string title, string[] bodyLines, string? logoPath = null)
        {
            // --- Load logo ---
            byte[]? logoRgb = null;
            int logoW = 0, logoH = 0;
            if (logoPath != null && File.Exists(logoPath))
            {
                try { logoRgb = LoadLogoAsRgb(logoPath, out logoW, out logoH); }
                catch { /* skip logo on failure */ }
            }
            bool hasLogo = logoRgb != null;

            // --- Build content stream ---
            var cs = new StringBuilder();

            // 2. Title (centered, bold, 18 pt) — computed first so we know sepY for logo placement
            float titleY = PageH - 28f;
            float charW = 10.8f; // Helvetica-Bold 18 pt average char width (estimate)
            float titleW = title.Length * charW;
            float availW = PageW - MarginL - MarginR;
            float titleX = Math.Max(MarginL, (availW - titleW) / 2f + MarginL);

            // Separator position
            float sepY = titleY - 10f;

            // 1. Draw logo image below the separator (outside BT/ET — graphics state)
            if (hasLogo)
            {
                float lx = PageW - MarginR - LogoDisplaySize;
                float ly = sepY - LogoDisplaySize - 1f; // 6 pt gap below separator
                cs.Append($"q\n{LogoDisplaySize} 0 0 {LogoDisplaySize} {lx:F1} {ly:F1} cm\n/Logo Do\nQ\n");
            }

            cs.Append("BT\n");
            cs.Append("/F2 18 Tf\n");
            cs.Append($"1 0 0 1 {titleX:F1} {titleY:F1} Tm\n");
            cs.Append($"({EscapePdf(title)}) Tj\n");
            cs.Append("ET\n");

            // 3. Separator line (graphics)
            cs.Append($"0.5 w\n{MarginL:F1} {sepY:F1} m\n{PageW - MarginR:F1} {sepY:F1} l\nS\n");

            // 4. Body text
            float bodyStartY = sepY - 10f;
            cs.Append("BT\n");
            cs.Append("/F1 11 Tf\n");
            cs.Append($"1 0 0 1 {MarginL:F1} {bodyStartY:F1} Tm\n");

            bool currentBold = false;
            float currentY = bodyStartY;

            foreach (var rawLine in bodyLines)
            {
                if (currentY < 55f) break;

                bool bold = rawLine.StartsWith("B:");
                string text = bold ? rawLine[2..] : rawLine;

                if (string.IsNullOrEmpty(text))
                {
                    cs.Append($"0 -8 Td\n");
                    currentY -= 8f;
                    continue;
                }

                if (bold != currentBold)
                {
                    cs.Append(bold ? "/F2 11 Tf\n" : "/F1 11 Tf\n");
                    currentBold = bold;
                }

                cs.Append($"0 -{LineH:F1} Td\n");
                cs.Append($"({EscapePdf(text)}) Tj\n");
                currentY -= LineH;
            }

            // 5. Footer (absolute position)
            cs.Append("/F1 9 Tf\n");
            cs.Append($"1 0 0 1 {MarginL:F1} 25 Tm\n");
            cs.Append("(CraiovaRide - Partenerul tau de incredere pentru transport in Craiova) Tj\n");
            cs.Append("ET\n");

            // 6. Footer separator
            cs.Append($"0.3 w\n{MarginL:F1} 38 m\n{PageW - MarginR:F1} 38 l\nS\n");

            var contentBytes = Encoding.Latin1.GetBytes(cs.ToString());

            // --- Assemble PDF objects ---
            // Objects: 1=Catalog, 2=Pages, 3=Page, 4=Content, 5=F1, 6=F2, [7=Logo]
            int objCount = hasLogo ? 7 : 6;
            var offsets = new long[objCount];
            var parts = new List<byte[]>();
            long pos = 0;

            void Append(byte[] d) { parts.Add(d); pos += d.Length; }
            byte[] A(string s) => Encoding.ASCII.GetBytes(s);

            Append(A("%PDF-1.4\n"));

            string fontRes = "/Font <</F1 5 0 R /F2 6 0 R>>";
            string xobjRes = hasLogo ? " /XObject <</Logo 7 0 R>>" : "";
            string resources = $"<< {fontRes}{xobjRes} >>";

            offsets[0] = pos;
            Append(A("1 0 obj\n<</Type /Catalog /Pages 2 0 R>>\nendobj\n\n"));

            offsets[1] = pos;
            Append(A("2 0 obj\n<</Type /Pages /Kids [3 0 R] /Count 1>>\nendobj\n\n"));

            offsets[2] = pos;
            Append(A($"3 0 obj\n<</Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources {resources}>>\nendobj\n\n"));

            offsets[3] = pos;
            Append(A($"4 0 obj\n<</Length {contentBytes.Length}>>\nstream\n"));
            Append(contentBytes);
            Append(A("\nendstream\nendobj\n\n"));

            offsets[4] = pos;
            Append(A("5 0 obj\n<</Type /Font /Subtype /Type1 /BaseFont /Helvetica>>\nendobj\n\n"));

            offsets[5] = pos;
            Append(A("6 0 obj\n<</Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold>>\nendobj\n\n"));

            if (hasLogo)
            {
                offsets[6] = pos;
                Append(A($"7 0 obj\n<</Type /XObject /Subtype /Image /Width {logoW} /Height {logoH} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Length {logoRgb!.Length}>>\nstream\n"));
                Append(logoRgb!);
                Append(A("\nendstream\nendobj\n\n"));
            }

            // xref
            long xrefPos = pos;
            int xrefSize = objCount + 1;
            var xref = new StringBuilder();
            xref.Append("xref\n");
            xref.Append($"0 {xrefSize}\n");
            xref.Append("0000000000 65535 f \n");
            foreach (var o in offsets)
                xref.Append($"{o:D10} 00000 n \n");
            xref.Append($"trailer\n<</Size {xrefSize} /Root 1 0 R>>\nstartxref\n{xrefPos}\n%%EOF\n");
            Append(A(xref.ToString()));

            int total = parts.Sum(b => b.Length);
            var result = new byte[total];
            int p = 0;
            foreach (var b in parts) { Buffer.BlockCopy(b, 0, result, p, b.Length); p += b.Length; }
            return result;
        }

        private static byte[] LoadLogoAsRgb(string path, out int width, out int height)
        {
            const int MaxSize = 110; // embed at higher res for the bigger display size
            using var src = new Bitmap(path);
            float scale = Math.Min((float)MaxSize / src.Width, (float)MaxSize / src.Height);
            int w = Math.Max(1, (int)(src.Width * scale));
            int h = Math.Max(1, (int)(src.Height * scale));

            // Composite on white to flatten transparency
            using var dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(dst))
            {
                g.Clear(Color.White);
                g.DrawImage(src, 0, 0, w, h);
            }

            var data = dst.LockBits(new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            var raw = new byte[stride * h];
            Marshal.Copy(data.Scan0, raw, 0, raw.Length);
            dst.UnlockBits(data);

            // GDI+ Format24bppRgb is stored BGR — swap to RGB for PDF
            var rgb = new byte[w * h * 3];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int s = y * stride + x * 3;
                    int d = (y * w + x) * 3;
                    rgb[d] = raw[s + 2];     // R
                    rgb[d + 1] = raw[s + 1]; // G
                    rgb[d + 2] = raw[s];     // B
                }

            width = w; height = h;
            return rgb;
        }

        private static string EscapePdf(string text)
        {
            var sb = new StringBuilder(text.Length + 4);
            foreach (char c in text)
            {
                if (c == '\\') sb.Append("\\\\");
                else if (c == '(') sb.Append("\\(");
                else if (c == ')') sb.Append("\\)");
                else if (c < 128) sb.Append(c);
                else sb.Append(NormalizeDiacritic(c));
            }
            return sb.ToString();
        }

        private static char NormalizeDiacritic(char c) => c switch
        {
            'ă' => 'a', 'Ă' => 'A',
            'â' => 'a', 'Â' => 'A',
            'î' => 'i', 'Î' => 'I',
            'ș' => 's', 'Ș' => 'S',
            'ş' => 's', 'Ş' => 'S',
            'ț' => 't', 'Ț' => 'T',
            'ţ' => 't', 'Ţ' => 'T',
            '\u2013' => '-', // en dash
            '\u2014' => '-', // em dash
            '\u2019' => '\'', '\u2018' => '\'',
            '\u201C' => '"', '\u201D' => '"',
            _ => '-'
        };
    }
}
