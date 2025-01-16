using System.Threading.Tasks;
using System.Collections.Generic;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Enums;

namespace StudentManagement.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, string password, IEnumerable<int> roleIds);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersAsync(int page, int pageSize, string keyword, string role = null);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> ValidatePasswordAsync(string username, string password);
        Task ChangePasswordAsync(int userId, string newPassword);
        Task ResetPasswordAsync(int userId, string newPassword);
        Task UpdateUserStatusAsync(int userId, UserStatus status);
        Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
        Task BatchUpdateStatusAsync(IEnumerable<int> userIds, UserStatus status);
        Task BatchResetPasswordAsync(IEnumerable<int> userIds, string newPassword);
        Task CreateStudentUserAsync(Student student);
        Task<int> GetTotalCountAsync(string? keyword, string? role);
    }
} 