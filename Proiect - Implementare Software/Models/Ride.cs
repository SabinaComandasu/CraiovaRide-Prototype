namespace Proiect_Implementare_Software.Models
{
    public class Ride
    {
        public int RideID { get; set; }
        public int UserID { get; set; }
        public int? DriverID { get; set; }
        public int? VehicleID { get; set; }
        public int? ProductID { get; set; }
        public string PickupLocation { get; set; }
        public string Destination { get; set; }
        public string RideStatus { get; set; }
        public float Fare { get; set; }
        public DateTime Date { get; set; }

        public Person User { get; set; }
        public Person Driver { get; set; }
        public Vehicle Vehicle { get; set; }
        public Product? Product { get; set; }

        public Payment Payment { get; set; }

        public ICollection<UserOrdersRide> UsersOrdered { get; set; }
    }

}
