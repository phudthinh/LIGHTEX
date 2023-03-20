using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models.Domain
{
    public class Customer
    {
        [Key]
        public int id_customer { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string ward { get; set; }
        public string city { get; set; }
        public byte[] avatar { get; set; }
        public double money { get; set; }
    }
}
