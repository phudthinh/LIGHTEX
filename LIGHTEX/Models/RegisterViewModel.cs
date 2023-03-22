using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class RegisterViewModel
    {
        [Key]
        public string username { get; set; }
        public string password { get; set; }
        public string full_name { get; set; }
        public bool active { get; set; }
        public int permission { get; set;}
        public DateTime last_login { get; set;}
        public DateTime create_date { get; set; }

    }
}
