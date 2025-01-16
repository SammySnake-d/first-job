using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Enums;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
namespace StudentManagement.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            try
            {
                // 检查是否需要执行迁移
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    try
                    {
                        await context.Database.MigrateAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Migration failed: {ex.Message}");
                        await context.Database.EnsureDeletedAsync();
                        await context.Database.MigrateAsync();
                    }
                }
                else
                {
                    await context.Database.EnsureCreatedAsync();
                }

                // 初始化角色
                if (!context.Roles.Any())
                {
                    var roles = new List<Role>
                    {
                        new Role { Id = (int)UserRole.SuperAdmin, Name = "超级管理员" },
                        new Role { Id = (int)UserRole.Admin, Name = "管理员" },
                        new Role { Id = (int)UserRole.Teacher, Name = "教师" },
                        new Role { Id = (int)UserRole.Student, Name = "学生" }
                    };

                    await context.Roles.AddRangeAsync(roles);
                    await context.SaveChangesAsync();
                }

                // 创建默认的超级管理员账户
                if (!context.Users.Any())
                {
                    var superAdmin = new User
                    {
                        Username = "superadmin",
                        Name = "超级管理员",
                        Role = UserRole.SuperAdmin,
                        Status = UserStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                var saltBytes = new byte[] { 
                1, 2, 3, 4, 5, 6, 7, 8, 
                9, 10, 11, 12, 13, 14, 15, 16 
            };  // 固定的16字节salt
            var salt = Convert.ToBase64String(saltBytes);
                     var passwordHash = HashPassword("123456",salt);
                    superAdmin.PasswordHash = passwordHash;
                    superAdmin.Salt = salt;

                    // 添加超级管理员角色映射
                    var userRole = new UserRoleMapping
                    {
                        UserId = superAdmin.Id,
                        RoleId = (int)UserRole.SuperAdmin,
                        CreatedAt = DateTime.UtcNow
                    };

                    await context.Users.AddAsync(superAdmin);
                    await context.SaveChangesAsync();  // 先保存用户以获取ID
                    await context.UserRoles.AddAsync(userRole);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
        }

       private static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Concat(password, salt);
                var bytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}