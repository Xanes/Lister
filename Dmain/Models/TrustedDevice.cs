using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class TrustedDevice
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Mac { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 