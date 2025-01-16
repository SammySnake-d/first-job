using System;

namespace StudentManagement.Core.DTOs
{
    // 用于成绩查看的 DTO
    public class GradeDTO
    {
        public int id { get; set; }
        public string courseName { get; set; } = string.Empty;
        public string teacherName { get; set; } = string.Empty;
        public decimal score { get; set; }
        public string semester { get; set; } = string.Empty;
        public string? remark { get; set; }
    }

    // 用于创建成绩的 DTO
    public class CreateGradeDto
    {
        public int StudentId { get; set; }
        public int CourseSelectionId { get; set; }
        public decimal Score { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public DateTime GradeDate { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}