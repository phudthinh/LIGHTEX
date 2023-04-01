using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class CustomerViewModel
    {
        [Key]
        public string username { get; set; }
        public string password { get; set; }
        public string full_name { get; set; }
        public bool active { get; set; }
        public int permission { get; set;}
        public DateTime last_login { get; set;}
        public DateTime create_date { get; set; }
        [Key]
        public int id_customer { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string ward { get; set; }
        public string city { get; set; }
        public byte[] avatar { get; set; }
        public double money { get; set; }

    }
}
