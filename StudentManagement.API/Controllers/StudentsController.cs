using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagement.API.DTOs;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/student")]
    public class StudentsController : ControllerBase
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly IRepository<Class> _classRepository;
        private readonly IRepository<User> _userRepository;

        public StudentsController(IRepository<Student> studentRepository, IRepository<Class> classRepository, IRepository<User> userRepository)
        {
            _studentRepository = studentRepository;
            _classRepository = classRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents(
            [FromQuery] string? keyword,
            [FromQuery] int? classId,
            [FromQuery] string? gender,
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _studentRepository.Query()
                    .Include(s => s.Class)
                    .AsQueryable();

                // 应用搜索条件
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(s =>
                        s.StudentNumber.Contains(keyword) ||
                        s.Name.Contains(keyword));
                }

                if (classId.HasValue)
                {
                    query = query.Where(s => s.ClassId == classId.Value);
                }

                if (!string.IsNullOrWhiteSpace(gender))
                {
                    query = query.Where(s => s.Gender == gender);
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(s => s.Status == status);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(s => s.EnrollmentDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(s => s.EnrollmentDate <= endDate.Value);
                }

                // 获取总记录数
                var totalCount = await query.CountAsync();

                // 应用分页
                var students = await query
                    .OrderByDescending(s => s.Id)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        StudentNumber = s.StudentNumber,
                        Name = s.Name,
                        Gender = s.Gender,
                        DateOfBirth = s.DateOfBirth,
                        ContactNumber = s.ContactNumber,
                        Email = s.Email,
                        Address = s.Address,
                        EnrollmentDate = s.EnrollmentDate,
                        Status = s.Status,
                        ClassId = s.ClassId,
                        Class = new ClassDto
                        {
                            Id = s.Class.Id,
                            Name = s.Class.Name,
                            Grade = s.Class.Grade,
                            Year = s.Class.Year
                        }
                    })
                    .ToListAsync();

                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取学生列表失败", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _studentRepository.Query()
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student student)
        {
            try 
            {
                // 验证班级是否存在
                var existingClass = await _classRepository.GetByIdAsync(student.ClassId);
                if (existingClass == null)
                {
                    return BadRequest(new { success = false, message = "指定的班级不存在" });
                }

                // 验证学号是否已存在
                var existingStudent = await _studentRepository.Query()
                    .FirstOrDefaultAsync(s => s.StudentNumber == student.StudentNumber);
                if (existingStudent != null)
                {
                    return BadRequest(new { success = false, message = "该学号已存在" });
                }

                // 创建学生记录
                var createdStudent = await _studentRepository.AddAsync(student);

                // 获取包含班级信息的完整学生数据
                var result = await _studentRepository.Query()
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.Id == createdStudent.Id);

                if (result == null)
                {
                    return StatusCode(500, new { 
                        success = false, 
                        message = "创建学生后无法获取详细信息" 
                    });
                }

                // 返回成功响应
                return Ok(new { 
                    success = true, 
                    message = "创建成功",
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "创建学生失败",
                    error = ex.Message,
                    details = ex.InnerException?.Message 
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Student>> UpdateStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest(new { success = false, message = "学生ID不匹配" });
            }

            try
            {
                // 验证班级是否存在
                var existingClass = await _classRepository.GetByIdAsync(student.ClassId);
                if (existingClass == null)
                {
                    return BadRequest(new { success = false, message = "指定的班级不存在" });
                }

                // 验证学生是否存在
                var existingStudent = await _studentRepository.GetByIdAsync(id);
                if (existingStudent == null)
                {
                    return NotFound(new { success = false, message = "未找到要更新的学生" });
                }

                // 检查学号是否被其他学生使用
                var studentWithSameNumber = await _studentRepository.Query()
                    .FirstOrDefaultAsync(s => s.StudentNumber == student.StudentNumber && s.Id != id);
                if (studentWithSameNumber != null)
                {
                    return BadRequest(new { success = false, message = "该学号已被其他学生使用" });
                }

                // 更新现有学生的属性
                existingStudent.StudentNumber = student.StudentNumber;
                existingStudent.Name = student.Name;
                existingStudent.Gender = student.Gender;
                existingStudent.DateOfBirth = student.DateOfBirth;
                existingStudent.ContactNumber = student.ContactNumber;
                existingStudent.Email = student.Email;
                existingStudent.Address = student.Address;
                existingStudent.EnrollmentDate = student.EnrollmentDate;
                existingStudent.Status = student.Status;
                existingStudent.ClassId = student.ClassId;

                // 更新学生信息
                await _studentRepository.UpdateAsync(existingStudent);

                // 返回更新后的学生信息
                var updatedStudent = await _studentRepository.Query()
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.Id == id);

                return Ok(new { 
                    success = true, 
                    message = "更新成功",
                    data = updatedStudent 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "更新学生信息失败", 
                    error = ex.Message,
                    details = ex.InnerException?.Message 
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound(new { message = "未找到要删除的学生" });
                }

                // 查找并删除关联的用户账号
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Username == student.StudentNumber);
                
                if (user != null)
                {
                    await _userRepository.DeleteAsync(user.Id);
                }

                // 删除学生记录
                await _studentRepository.DeleteAsync(id);
                
                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "删除学生失败", 
                    error = ex.Message 
                });
            }
        }

        [HttpGet("{id}/grades")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentGrades(int id)
        {
            try
            {
                var student = await _studentRepository.Query()
                    .Include(s => s.CourseSelections)
                    .ThenInclude(cs => cs.Course)
                    .Include(s => s.CourseSelections)
                    .ThenInclude(cs => cs.Grade)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                {
                    return NotFound(new { message = "未找到学生" });
                }

                var grades = student.CourseSelections
                    .Where(cs => cs.Grade != null)
                    .Select(cs => new
                    {
                        courseName = cs.Course.Name,
                        teacherName = cs.Course.TeacherName,
                        score = cs.Grade.Score,
                        gradeLevel = cs.Grade.GradeLevel,
                        semester = cs.Semester,
                        remark = cs.Grade.Comments
                    })
                    .ToList();

                return Ok(grades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取学生成绩失败", error = ex.Message });
            }
        }

        private async Task<bool> StudentExists(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            return student != null;
        }
    }
}