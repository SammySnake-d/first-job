using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagement.Core.Entities;

namespace StudentManagement.Core.Interfaces
{
    public interface IStudentService
    {
        Task<Student> CreateStudentAsync(Student student);
        Task<Student> GetStudentByIdAsync(int id);
        Task<IEnumerable<Student>> GetStudentsAsync(
            int page,
            int pageSize,
            string keyword = null,
            int? classId = null,
            string gender = null,
            string status = null);
        Task<int> GetTotalCountAsync(
            string keyword = null,
            int? classId = null,
            string gender = null,
            string status = null);
        Task UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(int id);
    }
} 