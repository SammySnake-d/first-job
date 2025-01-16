using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StudentManagement.Core.Entities
{
    public class Student
    {
        public Student()
        {
            CourseSelections = new List<CourseSelection>();
            Grades = new List<Grade>();
            StudentNumber = string.Empty;
            Name = string.Empty;
            Gender = string.Empty;
            ContactNumber = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            Status = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "学号是必填项")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "学号长度必须在3-20个字符之间")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "学号只能包含字母和数字")]
        public string StudentNumber { get; set; }

        [Required(ErrorMessage = "姓名是必填项")]
        public string Name { get; set; }

        [Required(ErrorMessage = "性别是必填项")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "出生日期是必填项")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "联系电话是必填项")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "邮箱是必填项")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string Email { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "入学日期是必填项")]
        public DateTime EnrollmentDate { get; set; }

        [Required(ErrorMessage = "状态是必填项")]
        public string Status { get; set; }

        [Required(ErrorMessage = "班级是必填项")]
        [Range(1, int.MaxValue, ErrorMessage = "请选择有效的班级")]
        public int ClassId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ClassId))]
        public virtual Class? Class { get; set; }

        [JsonIgnore]
        public virtual ICollection<CourseSelection> CourseSelections { get; set; }

        [JsonIgnore]
        public virtual ICollection<Grade> Grades { get; set; }
    }
}