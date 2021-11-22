using System.ComponentModel.DataAnnotations;

namespace JankiTransfer.DTO
{
    public class Signup
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}