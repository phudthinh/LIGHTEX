using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models.Domain
{
    public class Bill
    {
        [Key]
        public int id_bill { get; set; }
        public int id_customer { get; set; }
        public int id_product { get; set; }
        public int quantity { get; set; }
        public int status { get; set; }
        public int payments { get; set; }
        public DateTime ship_date { get; set; }
        public DateTime create_date { get; set; }
    }
}
