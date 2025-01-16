using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Core.Enums;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace StudentManagement.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRoleMapping> _userRoleRepository;
        private readonly IRepository<Student> _studentRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRoleMapping> userRoleRepository,
            IRepository<Student> studentRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _studentRepository = studentRepository;
            _logger = logger;
        }

        public async Task<User> CreateUserAsync(User user, string password, IEnumerable<int> roleIds)
        {
            try
            {
                if (user == null) throw new ArgumentNullException(nameof(user));
                if (string.IsNullOrEmpty(password)) throw new ArgumentException("密码不能为空", nameof(password));

                // 检查用户名是否已存在
                var existingUser = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    throw new Exception("用户名已存在");
                }

                // 如果是学生角色，检查学号
                if (user.Role == UserRole.Student)
                {
                    if (string.IsNullOrEmpty(user.StudentNumber))
                    {
                        throw new Exception("学生用户必须填写学号");
                    }

                    // 检查学号是否已被关联
                    var existingStudentUser = await _userRepository.Query()
                        .FirstOrDefaultAsync(u => u.StudentNumber == user.StudentNumber);
                    if (existingStudentUser != null)
                    {
                        throw new Exception("该学号已关联其他用户");
                    }

                    // 检查学号是否存在于学生表
                    var student = await _studentRepository.Query()
                        .FirstOrDefaultAsync(s => s.StudentNumber == user.StudentNumber);
                    if (student == null)
                    {
                        throw new Exception("学号不存在");
                    }
                }
                else
                {
                    // 非学生角色，清空学号
                    user.StudentNumber = string.Empty;
                }

                // 生成密码盐和哈希
                user.Salt = GenerateSalt();
                user.PasswordHash = HashPassword(password, user.Salt);
                user.CreatedAt = DateTime.UtcNow;
                user.Status = UserStatus.Active;

                // 创建用户
                var createdUser = await _userRepository.AddAsync(user);

                // 分配角色
                if (roleIds?.Any() == true)
                {
                    foreach (var roleId in roleIds)
                    {
                        await _userRoleRepository.AddAsync(new UserRoleMapping
                        {
                            UserId = createdUser.Id,
                            RoleId = roleId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户失败: {@User}", user);
                throw;
            }
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }
            return user;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("用户名不能为空", nameof(username));
            }

            // 只返回查询结果，不抛出异常
            return await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetUsersAsync(int page, int pageSize, string? keyword, string? role)
        {
            try
            {
                var query = _userRepository.Query();

                // 关键字搜索
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(u =>
                        u.Username.ToLower().Contains(keyword) ||
                        u.Name.ToLower().Contains(keyword) ||
                        u.StudentNumber.ToLower().Contains(keyword) ||
                        u.Email.ToLower().Contains(keyword)
                    );
                }

                // 角色筛选
                if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, out UserRole userRole))
                {
                    query = query.Where(u => u.Role == userRole);
                }

                // 分页
                var skip = (page - 1) * pageSize;
                return await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表失败");
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser == null)
            {
                throw new Exception("用户不存在");
            }

            // 更新基本信息
            existingUser.Username = user.Username;
            existingUser.Name = user.Name;
            existingUser.Role = user.Role;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // 如果有更新密码
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existingUser.Salt = user.Salt;
                existingUser.PasswordHash = user.PasswordHash;
            }

            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new Exception("用户不存在");
                }

                // 先删除用户角色关联
                var userRoles = await _userRoleRepository.Query()
                    .Where(ur => ur.UserId == id)
                    .ToListAsync();

                if (userRoles != null && userRoles.Any())
                {
                    foreach (var userRole in userRoles)
                    {
                        await _userRoleRepository.DeleteAsync(userRole.Id);
                    }
                }

                // 再删除用户
                await _userRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败: {UserId}", id);
                throw new Exception("删除用户失败", ex);
            }
        }

        public async Task<bool> ValidatePasswordAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
            {
                return false;
            }

            var hashedPassword = HashPassword(password, user.Salt);
            return hashedPassword == user.PasswordHash;
        }

        public async Task ChangePasswordAsync(int userId, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            user.Salt = GenerateSalt();
            user.PasswordHash = HashPassword(newPassword, user.Salt);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetPasswordAsync(int userId, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("用户不存在");
                }

                // 生成新的盐值和密码哈希
                user.Salt = GenerateSalt();
                user.PasswordHash = HashPassword(newPassword, user.Salt);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置密码失败: {UserId}", userId);
                throw new Exception("重置密码失败", ex);
            }
        }

        public async Task UpdateUserStatusAsync(int userId, UserStatus status)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _userRoleRepository.Query()
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();

            return userRoles;
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
        {
            var userRoles = await _userRoleRepository.Query()
                .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            var permissions = userRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct();

            return permissions;
        }

        public async Task BatchUpdateStatusAsync(IEnumerable<int> userIds, UserStatus status)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            foreach (var userId in userIds)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.Status = status;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                }
            }
        }

        public async Task BatchResetPasswordAsync(IEnumerable<int> userIds, string newPassword)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("新密码不能为空", nameof(newPassword));
            }

            foreach (var userId in userIds)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null && user.Status == UserStatus.Active)
                {
                    user.Salt = GenerateSalt();
                    user.PasswordHash = HashPassword(newPassword, user.Salt);
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                }
            }
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(password)) return false;

            // 使用相同的盐值和密码生成哈希值进行比较
            var hashedPassword = HashPassword(password, user.Salt);
            return hashedPassword == user.PasswordHash;
        }

        public async Task<bool> CheckUsernameAvailableAsync(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;

            var existingUser = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.Username == username);
            return existingUser == null;
        }

        public async Task<int> GetTotalCountAsync(string? keyword, string? role)
        {
            try
            {
                var query = _userRepository.Query();

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    query = query.Where(u =>
                        u.Username.ToLower().Contains(keyword) ||
                        u.Name.ToLower().Contains(keyword) ||
                        u.StudentNumber.ToLower().Contains(keyword) ||
                        u.Email.ToLower().Contains(keyword)
                    );
                }

                if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, out UserRole userRole))
                {
                    query = query.Where(u => u.Role == userRole);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户总数失败");
                throw;
            }
        }

        private string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Concat(password, salt);
                var bytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task CreateStudentUserAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            // 检查学生是否已有关联的用户账号
            var existingUser = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.StudentNumber == student.StudentNumber);

            if (existingUser != null)
            {
                throw new Exception("该学生已有关联的用户账号");
            }

            // 创建用户账号
            var user = new User
            {
                Username = student.StudentNumber, // 使用学号作为用户名
                Name = student.Name,
                StudentNumber = student.StudentNumber,
                Role = UserRole.Student,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            // 使用学号作为初始密码
            var initialPassword = $"{student.StudentNumber}@123";
            user.Salt = GenerateSalt();
            user.PasswordHash = HashPassword(initialPassword, user.Salt);

            await _userRepository.AddAsync(user);

            // 分配学生角色
            await _userRoleRepository.AddAsync(new UserRoleMapping
            {
                UserId = user.Id,
                RoleId = (int)UserRole.Student,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}