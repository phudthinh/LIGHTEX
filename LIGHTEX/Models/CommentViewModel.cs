using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class CommentViewModel
    {
        [Key]
        public int id_comment { get; set; }
        public int id_bill { get; set; }

        public string product { get; set; }
        public string category { get; set; }
        public string brand { get; set; }
        public byte[] image { get; set; }
        public string content { get; set; }
        public int star { get; set; }


    }
}
