using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.Entities
{
    public class UserRoleMapping
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int RoleId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
} 