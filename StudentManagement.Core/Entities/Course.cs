using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentManagement.Core.Entities
{
    public class Course
    {
        public Course()
        {
            CourseSelections = new List<CourseSelection>();
            Code = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            TeacherName = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "课程代码是必填项")]
        public string Code { get; set; }

        [Required(ErrorMessage = "课程名称是必填项")]
        public string Name { get; set; }

        [Required(ErrorMessage = "学分是必填项")]
        [Range(1, 10, ErrorMessage = "学分必须在1-10之间")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "课程描述是必填项")]
        public string Description { get; set; }

        [Required(ErrorMessage = "任课教师是必填项")]
        [StringLength(50, ErrorMessage = "任课教师姓名不能超过50个字符")]
        public string TeacherName { get; set; }

        public int? PrerequisiteCourseId { get; set; }

        [JsonIgnore]
        public virtual Course? PrerequisiteCourse { get; set; }

        [JsonIgnore]
        public virtual ICollection<CourseSelection> CourseSelections { get; set; }
    }
}