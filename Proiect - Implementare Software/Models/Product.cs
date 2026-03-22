namespace Proiect_Implementare_Software.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PdfPath { get; set; } = string.Empty;

        public ICollection<Ride> Rides { get; set; } = new List<Ride>();
    }
}
