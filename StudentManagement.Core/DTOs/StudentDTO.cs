namespace StudentManagement.Core.DTOs
{
    public class StudentDTO
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ClassName { get; set; }
        public string? Grade { get; set; }
    }
} 