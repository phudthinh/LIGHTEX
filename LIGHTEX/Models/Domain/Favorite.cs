using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models.Domain
{
    public class Favorite
    {
        [Key]
        public int id_favorite { get; set; }
        public int id_customer { get; set; }
        public int id_product { get; set; }
        public bool active { get; set; }
    }
}
