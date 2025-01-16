using System;

namespace StudentManagement.Core.Models
{
    public class SystemLog
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public string Module { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string StackTrace { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 