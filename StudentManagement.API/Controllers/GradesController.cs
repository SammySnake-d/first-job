using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.DTOs;
using StudentManagement.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/grades")]
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetGrades(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? classId = null,
            [FromQuery] int? studentId = null,
            [FromQuery] int? courseId = null,
            [FromQuery] string? gradeLevel = null)
        {
            try
            {
                var query = _context.Grades
                    .Include(g => g.Student)
                        .ThenInclude(s => s.Class)
                    .Include(g => g.CourseSelection)
                        .ThenInclude(cs => cs.Course)
                    .AsQueryable();

                if (classId.HasValue)
                {
                    query = query.Where(g => g.Student.ClassId == classId);
                }

                if (studentId.HasValue)
                {
                    query = query.Where(g => g.StudentId == studentId);
                }

                if (courseId.HasValue)
                {
                    query = query.Where(g => g.CourseSelection.CourseId == courseId);
                }

                if (!string.IsNullOrEmpty(gradeLevel))
                {
                    query = query.Where(g => g.GradeLevel == gradeLevel);
                }

                var total = await query.CountAsync();

                var grades = await query
                    .OrderByDescending(g => g.GradeDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new
                    {
                        g.Id,
                        g.StudentId,
                        g.CourseSelectionId,
                        g.Score,
                        g.GradeLevel,
                        g.GradeDate,
                        g.Comments,
                        Student = new
                        {
                            g.Student.Id,
                            g.Student.StudentNumber,
                            g.Student.Name,
                            g.Student.ClassId,
                            Class = g.Student.Class != null ? new
                            {
                                g.Student.Class.Id,
                                g.Student.Class.Name,
                                g.Student.Class.Grade
                            } : null
                        },
                        CourseSelection = new
                        {
                            g.CourseSelection.Id,
                            g.CourseSelection.CourseId,
                            Course = new
                            {
                                g.CourseSelection.Course.Id,
                                g.CourseSelection.Course.Name,
                                g.CourseSelection.Course.Code
                            }
                        }
                    })
                    .ToListAsync();

                return Ok(new { data = grades, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取成绩列表失败", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Grade>> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.CourseSelection)
                .ThenInclude(cs => cs.Course)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                return NotFound(new { message = "未找到成绩记录" });
            }

            return grade;
        }

        [HttpPost]
        public async Task<ActionResult<Grade>> CreateGrade(CreateGradeDto gradeDto)
        {
            try
            {
                // 检查学生是否存在
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == gradeDto.StudentId);

                if (student == null)
                {
                    return BadRequest(new { message = "未找到对应的学生" });
                }

                // 检查选课记录是否存在
                var courseSelection = await _context.CourseSelections
                    .Include(cs => cs.Course)
                    .FirstOrDefaultAsync(cs => cs.Id == gradeDto.CourseSelectionId);

                if (courseSelection == null)
                {
                    return BadRequest(new { message = "未找到对应的选课记录" });
                }

                // 检查是否已经有成绩
                var existingGrade = await _context.Grades
                    .FirstOrDefaultAsync(g => g.CourseSelectionId == gradeDto.CourseSelectionId);

                if (existingGrade != null)
                {
                    return BadRequest(new { message = "该课程已有成绩记录" });
                }

                // 创建新的成绩记录
                var newGrade = new Grade
                {
                    StudentId = gradeDto.StudentId,
                    CourseSelectionId = gradeDto.CourseSelectionId,
                    Score = gradeDto.Score,
                    GradeLevel = gradeDto.GradeLevel,
                    GradeDate = gradeDto.GradeDate,
                    Comments = gradeDto.Comments ?? string.Empty
                };

                // 更新选课状态为已完成
                courseSelection.Status = "Completed";
                _context.Entry(courseSelection).State = EntityState.Modified;

                _context.Grades.Add(newGrade);
                await _context.SaveChangesAsync();

                // 返回包含关联数据的结果
                var result = await _context.Grades
                    .Include(g => g.Student)
                    .Include(g => g.CourseSelection)
                    .ThenInclude(cs => cs.Course)
                    .FirstAsync(g => g.Id == newGrade.Id);

                return CreatedAtAction(nameof(GetGrade), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "创建成绩记录失败", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGrade(int id, CreateGradeDto gradeDto)
        {
            try
            {
                // 检查成绩记录是否存在
                var existingGrade = await _context.Grades.FindAsync(id);
                if (existingGrade == null)
                {
                    return NotFound(new { message = "未找到成绩记录" });
                }

                // 检查学生是否存在
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == gradeDto.StudentId);

                if (student == null)
                {
                    return BadRequest(new { message = "未找到对应的学生" });
                }

                // 检查选课记录是否存在
                var courseSelection = await _context.CourseSelections
                    .Include(cs => cs.Course)
                    .FirstOrDefaultAsync(cs => cs.Id == gradeDto.CourseSelectionId);

                if (courseSelection == null)
                {
                    return BadRequest(new { message = "未找到对应的选课记录" });
                }

                // 更新成绩信息
                existingGrade.StudentId = gradeDto.StudentId;
                existingGrade.CourseSelectionId = gradeDto.CourseSelectionId;
                existingGrade.Score = gradeDto.Score;
                existingGrade.GradeLevel = gradeDto.GradeLevel;
                existingGrade.GradeDate = gradeDto.GradeDate;
                existingGrade.Comments = gradeDto.Comments ?? string.Empty;

                await _context.SaveChangesAsync();

                // 返回更新后的数据
                var result = await _context.Grades
                    .Include(g => g.Student)
                    .Include(g => g.CourseSelection)
                    .ThenInclude(cs => cs.Course)
                    .FirstAsync(g => g.Id == id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新成绩记录失败", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            try
            {
                var grade = await _context.Grades
                    .Include(g => g.CourseSelection)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (grade == null)
                {
                    return NotFound(new { message = "未找到成绩记录" });
                }

                // 更新选课状态为已选
                grade.CourseSelection.Status = "Selected";
                _context.Entry(grade.CourseSelection).State = EntityState.Modified;

                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除成绩记录失败", error = ex.Message });
            }
        }
    }
}