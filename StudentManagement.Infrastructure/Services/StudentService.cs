using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Core.Enums;
using System.Linq;
using StudentManagement.Infrastructure.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StudentManagement.Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpClient _httpClient;

        public StudentService(
            IRepository<Student> studentRepository,
            IUserService userService,
            ApplicationDbContext dbContext)
        {
            _studentRepository = studentRepository;
            _userService = userService;
            _dbContext = dbContext;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5284/")
            };
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 验证学号是否已存在
                var existingStudent = await _studentRepository.Query()
                    .FirstOrDefaultAsync(s => s.StudentNumber == student.StudentNumber);
                if (existingStudent != null)
                {
                    throw new Exception("学号已存在");
                }

                // 创建学生记录
                var createdStudent = await _studentRepository.AddAsync(student);
                await _dbContext.SaveChangesAsync();

                // 创建用户账号
                try
                {
                    var user = new User
                    {
                        Username = student.StudentNumber,
                        Name = student.Name,
                        StudentNumber = student.StudentNumber,
                        Role = UserRole.Student,
                        Status = UserStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _userService.CreateUserAsync(user, "123", new List<int> { 4 }); // 4 是学生角色ID
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"创建用户账号失败: {ex.Message}", ex);
                }

                // 提交事务
                await transaction.CommitAsync();
                return createdStudent;
            }
            catch (Exception ex)
            {
                // 回滚事务
                await transaction.RollbackAsync();
                throw new Exception($"创建学生失败: {ex.Message}", ex);
            }
        }

        public async Task<Student> GetStudentByIdAsync(int id)
        {
            return await _studentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Student>> GetStudentsAsync(
            int page,
            int pageSize,
            string keyword = null,
            int? classId = null,
            string gender = null,
            string status = null)
        {
            var query = _studentRepository.Query();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s =>
                    s.StudentNumber.Contains(keyword) ||
                    s.Name.Contains(keyword));
            }

            if (classId.HasValue)
            {
                query = query.Where(s => s.ClassId == classId.Value);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(s => s.Gender == gender);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(
            string keyword = null,
            int? classId = null,
            string gender = null,
            string status = null)
        {
            var query = _studentRepository.Query();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s =>
                    s.StudentNumber.Contains(keyword) ||
                    s.Name.Contains(keyword));
            }

            if (classId.HasValue)
            {
                query = query.Where(s => s.ClassId == classId.Value);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(s => s.Gender == gender);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            return await query.CountAsync();
        }

        public async Task UpdateStudentAsync(Student student)
        {
            await _studentRepository.UpdateAsync(student);
        }

        public async Task DeleteStudentAsync(int id)
        {
            await _studentRepository.DeleteAsync(id);
        }
    }
} 