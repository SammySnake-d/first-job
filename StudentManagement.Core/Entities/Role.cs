using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.Entities
{
    public class Role
    {
        public Role()
        {
            Name = string.Empty;
            Description = string.Empty;
            UserRoles = new List<UserRoleMapping>();
            RolePermissions = new List<RolePermission>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public bool IsSystem { get; set; }

        public virtual ICollection<UserRoleMapping> UserRoles { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
} 