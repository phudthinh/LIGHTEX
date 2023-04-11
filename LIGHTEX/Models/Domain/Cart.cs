using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models.Domain
{
    public class Cart
    {
        [Key]
        public int id_cart { get; set; }
        public int id_customer { get; set; }
        public int id_product { get; set; }
        public int quantity { get; set; }
    }
}
