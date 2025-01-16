using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/course")]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetCourses(
            [FromQuery] string? keyword,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Courses
                .Include(c => c.PrerequisiteCourse)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.Code.Contains(keyword) ||
                    c.Name.Contains(keyword));
            }

            var total = await query.CountAsync();
            var courses = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { data = courses, total });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.PrerequisiteCourse)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse(Course course)
        {
            if (await _context.Courses.AnyAsync(c => c.Code == course.Code))
            {
                return BadRequest(new { message = $"课程代码 '{course.Code}' 已存在" });
            }

            _context.Courses.Add(course);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Courses_Code") == true)
                {
                    return BadRequest(new { message = $"课程代码 '{course.Code}' 已存在" });
                }
                throw;
            }

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, Course course)
        {
            if (id != course.Id)
            {
                return BadRequest(new { message = "无效的请求" });
            }

            var existingCourse = await _context.Courses
                .FirstOrDefaultAsync(c => c.Code == course.Code && c.Id != id);
            if (existingCourse != null)
            {
                return BadRequest(new { message = $"课程代码 '{course.Code}' 已被其他课程使用" });
            }

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CourseExists(id))
                {
                    return NotFound(new { message = "未找到课程" });
                }
                throw;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Courses_Code") == true)
                {
                    return BadRequest(new { message = $"课程代码 '{course.Code}' 已被其他课程使用" });
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("available/{studentId}")]
        public async Task<ActionResult<IEnumerable<Course>>> GetAvailableCourses(int studentId)
        {
            try
            {
                // 获取学生已选的课程ID列表
                var selectedCourseIds = await _context.CourseSelections
                    .Where(cs => cs.StudentId == studentId)
                    .Select(cs => cs.CourseId)
                    .ToListAsync();

                // 获取所有未被该学生选择的课程
                var availableCourses = await _context.Courses
                    .Where(c => !selectedCourseIds.Contains(c.Id))
                    .ToListAsync();

                return Ok(availableCourses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取可选课程失败", error = ex.Message });
            }
        }

        private async Task<bool> CourseExists(int id)
        {
            return await _context.Courses.AnyAsync(e => e.Id == id);
        }
    }
}