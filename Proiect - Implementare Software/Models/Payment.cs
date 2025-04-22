namespace Proiect_Implementare_Software.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int RideID { get; set; }
        public int UserID { get; set; }
        public float Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime Date { get; set; }

        public Ride Ride { get; set; }
        public Person User { get; set; }
    }

}
