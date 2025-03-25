using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class PasswordConfig
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Password { get; set; }
        
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
} 