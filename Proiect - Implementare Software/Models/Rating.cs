namespace Proiect_Implementare_Software.Models
{
    public class Rating
    {
        public int RatingID { get; set; }
        public int UserID { get; set; }
        public int DriverID { get; set; }
        public int Stars { get; set; }
        public string Review { get; set; }
        public DateTime Date { get; set; }

        public Person User { get; set; }
        public Person Driver { get; set; }
    }
}
