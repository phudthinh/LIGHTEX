using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class Category
    {
        [Key]
        public int id_category { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public byte[] icon { get; set; }
    }
}
