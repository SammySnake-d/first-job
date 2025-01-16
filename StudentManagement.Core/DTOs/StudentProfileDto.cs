using System;
using StudentManagement.Core.Enums;

namespace StudentManagement.Core.DTOs
{
    public class StudentProfileDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string? ClassName { get; set; }
        public string? Grade { get; set; }
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
    }
} 