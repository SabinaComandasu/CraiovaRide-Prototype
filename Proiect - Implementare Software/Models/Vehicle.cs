namespace Proiect_Implementare_Software.Models
{
    public class Vehicle
    {
        public int VehicleID { get; set; }
        public int DriverID { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public string RegistrationStatus { get; set; }

        public Person Driver { get; set; }
    }
}
