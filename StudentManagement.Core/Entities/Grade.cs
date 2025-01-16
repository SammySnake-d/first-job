using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement.Core.Entities
{
    public class Grade
    {
        public Grade()
        {
            GradeLevel = string.Empty;
            Comments = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "学生是必填项")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "选课记录是必填项")]
        public int CourseSelectionId { get; set; }

        [Required(ErrorMessage = "分数是必填项")]
        [Range(0, 100, ErrorMessage = "分数必须在0-100之间")]
        public decimal Score { get; set; }

        [Required(ErrorMessage = "等级是必填项")]
        public string GradeLevel { get; set; }

        [Required(ErrorMessage = "成绩日期是必填项")]
        public DateTime GradeDate { get; set; }

        public string Comments { get; set; }

        [Required]
        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

        [Required]
        [ForeignKey(nameof(CourseSelectionId))]
        public CourseSelection CourseSelection { get; set; }
    }
}