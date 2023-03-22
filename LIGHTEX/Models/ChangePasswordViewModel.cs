using System.ComponentModel.DataAnnotations;

namespace LIGHTEX.Models
{
    public class ChangePasswordViewModel
    {
        [Key]
        public string username { get; set; }
        public string newpassword { get; set; }
        public string re_newpassword { get; set; }
    }
}
