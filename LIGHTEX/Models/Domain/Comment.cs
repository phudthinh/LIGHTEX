using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models.Domain
{
    public class Comment
    {
        [Key]
        public int id_comment { get; set; }
        public int id_customer { get; set; }
        public int id_product { get; set; }
        public string content { get; set; }
        public int star { get; set; }
    }
}
