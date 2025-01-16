using System;
using System.Collections.Generic;
using StudentManagement.Core.Enums;

namespace StudentManagement.Core.Entities
{
    public class User : BaseEntity
    {
        public User()
        {
            Username = string.Empty;
            Name = string.Empty;
            PasswordHash = string.Empty;
            Salt = string.Empty;
            StudentNumber = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            CanResetPassword = true;
            CreatedAt = DateTime.UtcNow;
            LastLoginTime = DateTime.UtcNow;
            UpdatedAt = null;
            Role = UserRole.Student;
            Status = UserStatus.Active;
            UserRoles = new List<UserRoleMapping>();
        }

        public string Username { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public string StudentNumber { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool CanResetPassword { get; set; }
        public virtual ICollection<UserRoleMapping> UserRoles { get; set; }
    }
}