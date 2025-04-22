namespace Proiect_Implementare_Software.Models
{
    public class PromoCode
    {
        public int PromoCodeID { get; set; }
        public string Code { get; set; }
        public float Discount { get; set; }
        public int PersonID { get; set; }

        public Person Person { get; set; }
    }
}
