using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.Entities
{
    public class RolePermission
    {
        public int Id { get; set; }
        
        [Required]
        public int RoleId { get; set; }
        
        [Required]
        public int PermissionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }
    }
} 