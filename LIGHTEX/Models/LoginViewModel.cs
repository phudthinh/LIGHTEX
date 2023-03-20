using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class LoginViewModel
    {
        [Key]
        public string username { get; set; }
        public string password { get; set; }
    }
}
