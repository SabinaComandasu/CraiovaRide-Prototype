namespace Proiect_Implementare_Software.Models
{
    public class UserOrdersRide
    {
        public int UserID { get; set; }
        public Person User { get; set; }

        public int RideID { get; set; }
        public Ride Ride { get; set; }
    }

}
