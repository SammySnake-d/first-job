using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentManagement.Core.Entities
{
    public class Class
    {
        public Class()
        {
            Students = new List<Student>();
            Name = string.Empty;
            Grade = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "班级名称是必填项")]
        public string Name { get; set; }

        [Required(ErrorMessage = "年级是必填项")]
        public string Grade { get; set; }

        [Required(ErrorMessage = "年份是必填项")]
        public int Year { get; set; }

        [JsonIgnore]
        public virtual ICollection<Student> Students { get; set; }
    }
}