using System.ComponentModel.DataAnnotations;

namespace JankiWeb.Models
{
    public class Signup
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}