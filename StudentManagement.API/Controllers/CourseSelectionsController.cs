using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using StudentManagement.Core.Enums;
using System.Security.Claims;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/course-selection")]
    public class CourseSelectionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseSelectionsController> _logger;

        public CourseSelectionsController(
            ApplicationDbContext context,
            ILogger<CourseSelectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetCourseSelections(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? studentKeyword = null,
            [FromQuery] string? courseKeyword = null,
            [FromQuery] string? semester = null,
            [FromQuery] string? status = null,
            [FromQuery] int? year = null)
        {
            try
            {
                // 获取当前用户角色和学号
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var studentNumber = User.Claims.FirstOrDefault(c => c.Type == "StudentNumber")?.Value;
                
                _logger.LogInformation($"Current user role: {userRole}, student number: {studentNumber}");

                var query = _context.CourseSelections
                    .Include(cs => cs.Student)
                    .Include(cs => cs.Course)
                    .Include(cs => cs.Grade)
                    .AsQueryable();

                // 使用枚举进行比较
                if (userRole == ((int)UserRole.Student).ToString())
                {
                    if (string.IsNullOrEmpty(studentNumber))
                    {
                        return Forbid();
                    }
                    query = query.Where(cs => cs.Student.StudentNumber == studentNumber);
                }

                if (!string.IsNullOrEmpty(studentKeyword))
                {
                    query = query.Where(cs => 
                        cs.Student.StudentNumber.Contains(studentKeyword) || 
                        cs.Student.Name.Contains(studentKeyword));
                }

                if (!string.IsNullOrEmpty(courseKeyword))
                {
                    query = query.Where(cs => 
                        cs.Course.Name.Contains(courseKeyword) || 
                        cs.Course.Code.Contains(courseKeyword));
                }

                if (!string.IsNullOrEmpty(semester))
                {
                    query = query.Where(cs => cs.Semester == semester);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(cs => cs.Status == status);
                }

                if (year.HasValue)
                {
                    query = query.Where(cs => cs.Year == year);
                }

                var total = await query.CountAsync();

                var courseSelections = await query
                    .OrderByDescending(cs => cs.SelectionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new { data = courseSelections, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取选课列表失败", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseSelection>> GetCourseSelection(int id)
        {
            var courseSelection = await _context.CourseSelections
                .Include(cs => cs.Student)
                .Include(cs => cs.Course)
                .Include(cs => cs.Grade)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (courseSelection == null)
            {
                return NotFound(new { message = "未找到选课记录" });
            }

            return courseSelection;
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentCourseSelections(
            int studentId,
            [FromQuery] string? courseName = null,
            [FromQuery] string? semester = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var studentNumber = User.Claims.FirstOrDefault(c => c.Type == "StudentNumber")?.Value;
                
                _logger.LogInformation($"GetStudentCourseSelections - Role: {userRole}, StudentNumber: {studentNumber}");

                if (userRole == ((int)UserRole.Student).ToString())
                {
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.Id == studentId);

                    if (student == null || student.StudentNumber != studentNumber)
                    {
                        return Forbid();
                    }
                }

                var query = _context.CourseSelections
                    .Include(cs => cs.Student)
                    .Include(cs => cs.Course)
                    .Where(cs => cs.StudentId == studentId);

                if (!string.IsNullOrEmpty(courseName))
                {
                    query = query.Where(cs => 
                        cs.Course.Name.Contains(courseName) || 
                        cs.Course.Code.Contains(courseName));
                }

                if (!string.IsNullOrEmpty(semester))
                {
                    query = query.Where(cs => cs.Semester == semester);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(cs => cs.Status == status);
                }

                var courseSelections = await query.ToListAsync();
                return Ok(courseSelections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取学生选课记录失败");
                return StatusCode(500, new { message = "获取选课记录失败", error = ex.Message });
            }
        }

        public class CourseSelectionRequest
        {
            [Required]
            public int StudentId { get; set; }

            [Required]
            public int CourseId { get; set; }

            [Required]
            public string Semester { get; set; }

            [Required]
            public int Year { get; set; }

            public string? Status { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult<CourseSelection>> CreateCourseSelection([FromBody] CourseSelectionRequest request)
        {
            try 
            {
                // 检查该学生是否已经选择了这门课程
                var existingSelection = await _context.CourseSelections
                    .FirstOrDefaultAsync(cs => 
                        cs.StudentId == request.StudentId && 
                        cs.CourseId == request.CourseId &&
                        cs.Status != "Dropped");  // 排除已退���的记录

                if (existingSelection != null)
                {
                    return BadRequest(new { message = "该学生已经选择了这门课程" });
                }

                var courseSelection = new CourseSelection
                {
                    StudentId = request.StudentId,
                    CourseId = request.CourseId,
                    Semester = request.Semester,
                    Year = request.Year,
                    Status = "Selected",
                    SelectionDate = DateTime.Now
                };

                _context.CourseSelections.Add(courseSelection);
                await _context.SaveChangesAsync();

                var result = await _context.CourseSelections
                    .Include(cs => cs.Student)
                    .Include(cs => cs.Course)
                    .FirstOrDefaultAsync(cs => cs.Id == courseSelection.Id);

                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "创建选课记录失败", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCourseSelection(int id, [FromBody] CourseSelectionRequest request)
        {
            try
            {
                var courseSelection = await _context.CourseSelections.FindAsync(id);
                if (courseSelection == null)
                {
                    return NotFound(new { message = "未找到选课记录" });
                }

                courseSelection.StudentId = request.StudentId;
                courseSelection.CourseId = request.CourseId;
                courseSelection.Semester = request.Semester;
                courseSelection.Year = request.Year;

                if (!string.IsNullOrEmpty(request.Status))
                {
                    courseSelection.Status = request.Status;
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "更新选课记录失败", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseSelection(int id)
        {
            try
            {
                var courseSelection = await _context.CourseSelections.FindAsync(id);
                if (courseSelection == null)
                {
                    return NotFound(new { message = "未找到选课记录" });
                }

                // 如果已经有成绩��不允许删除
                if (await _context.Grades.AnyAsync(g => g.CourseSelectionId == id))
                {
                    return BadRequest(new { message = "该选课记录已有成绩，无法删除" });
                }

                _context.CourseSelections.Remove(courseSelection);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除选课记录失败", error = ex.Message });
            }
        }
    }
} 