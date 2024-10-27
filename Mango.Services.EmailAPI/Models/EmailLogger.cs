using System.ComponentModel.DataAnnotations;

namespace Mango.Services.EmailAPI.Models
{
    public class EmailLogger
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime? EmailSent { get; set; }
    }
}
