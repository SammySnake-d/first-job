using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StudentManagement.Core.Entities
{
    public class CourseSelection
    {
        public CourseSelection()
        {
            Semester = string.Empty;
            Status = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "学生是必填项")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "课程是必填项")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "学期是必填项")]
        public string Semester { get; set; }

        [Required(ErrorMessage = "年份是必填项")]
        public int Year { get; set; }

        public DateTime SelectionDate { get; set; }

        [Required(ErrorMessage = "状态是必填项")]
        public string Status { get; set; }

        [Required]
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [Required]
        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; }

        [JsonIgnore]
        public virtual Grade? Grade { get; set; }
    }
}