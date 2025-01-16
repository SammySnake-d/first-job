using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Core.Entities
{
    public class Permission
    {
        public Permission()
        {
            Name = string.Empty;
            Code = string.Empty;
            Description = string.Empty;
            Module = string.Empty;
            Category = string.Empty;
            RolePermissions = new List<RolePermission>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public string Module { get; set; }

        public string Category { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
} 