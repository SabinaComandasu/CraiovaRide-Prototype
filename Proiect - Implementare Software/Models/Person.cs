namespace Proiect_Implementare_Software.Models
{
    public class Person
    {
        public int PersonID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public float Rating { get; set; }
        public bool Availability { get; set; }
        public string DriverStatus { get; set; }
        public string Role { get; set; }

        public int? VehicleID { get; set; }
        public Vehicle Vehicle { get; set; }

        // Relații clare și corecte
        public ICollection<UserOrdersRide> OrderedRides { get; set; }  // asta e important
        public ICollection<Ride> Drives { get; set; }
        public ICollection<Rating> GivenRatings { get; set; }
        public ICollection<Rating> ReceivedRatings { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<PromoCode> PromoCodes { get; set; }
    }


}
