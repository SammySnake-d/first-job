using StudentManagement.Core.Enums;

namespace StudentManagement.API.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string StudentNumber { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
    }
} 