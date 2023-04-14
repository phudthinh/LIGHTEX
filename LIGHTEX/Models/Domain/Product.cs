using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class Product
    {
        [Key]
        public int id_product { get; set; }
        public int id_category { get; set; }
        public int id_brand { get; set; }
        public string name { get; set; }
        public string information { get; set; }
        public double price { get; set; }
        public int effect { get; set; }
        public byte[] image { get; set; }
        public bool status { get; set; }
        public DateTime create_date { get; set; }
        public DateTime modified_date { get; set; }
    }
}
