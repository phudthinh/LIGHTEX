using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class OrderDetailViewModel
    {
        [Key]
        public int id_bill { get; set; }
        public string username { get; set; }
        public string full_name { get; set; }
        public string product { get; set; }
        public string category { get; set; }
        public string brand { get; set; }
        public byte[] image { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public int status { get; set; }
        public int payments { get; set; }
        public DateTime ship_date { get; set; }
        public DateTime create_date { get; set; }
    }
}
