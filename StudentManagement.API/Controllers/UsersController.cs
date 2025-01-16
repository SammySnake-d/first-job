using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagement.API.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Infrastructure.Data;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;
        private readonly ApplicationDbContext _context;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger,
            ApplicationDbContext context)
        {
            _userService = userService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? role = null)
        {
            try
            {
                var users = await _userService.GetUsersAsync(page, pageSize, keyword ?? string.Empty, role ?? string.Empty);
                var total = await _userService.GetTotalCountAsync(keyword ?? string.Empty, role ?? string.Empty);

                var userDtos = users.Select(u => new UserListDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Name = u.Name,
                    StudentNumber = u.StudentNumber,
                    Email = u.Email,
                    Role = u.Role,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                });

                return Ok(new
                {
                    data = userDtos,
                    total = total
                });
            }
            catch (Exception ex)
            {
                await LogError("GetUsers", ex);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                var userDto = new UserDetailDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    StudentNumber = user.StudentNumber,
                    Role = user.Role,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var existingUser = await _userService.GetUserByUsernameAsync(dto.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "用户名已存在" });
                }

                var user = new User
                {
                    Username = dto.Username,
                    Name = dto.Name,
                    StudentNumber = dto.StudentNumber ?? string.Empty,
                    Role = dto.Role,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userService.CreateUserAsync(user, dto.Password, new[] { (int)dto.Role });

                var userDto = new UserDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Name = createdUser.Name,
                    StudentNumber = createdUser.StudentNumber,
                    Role = createdUser.Role,
                    Status = createdUser.Status
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                if (user.Role == UserRole.SuperAdmin)
                {
                    if (dto.Username != user.Username)
                    {
                        return BadRequest(new { message = "超级管理员用户名不能修改" });
                    }
                    if (dto.Role != UserRole.SuperAdmin)
                    {
                        return BadRequest(new { message = "超级管理员角色不能修改" });
                    }
                }

                if (user.Role == UserRole.Student)
                {
                    return BadRequest(new { message = "学生信息需要在学生管理模块中修改" });
                }

                if (dto.Role == UserRole.Student || dto.Role == UserRole.SuperAdmin)
                {
                    return BadRequest(new { message = "不能将用户角色设置为学生或超级管理员" });
                }

                user.Username = dto.Username;
                user.Name = dto.Name;
                user.Role = dto.Role;
                user.UpdatedAt = DateTime.UtcNow;

                await _userService.UpdateUserAsync(user);
                return Ok(new { message = "更新成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.NewPassword))
                {
                    return BadRequest(new { message = "新密码不能为空" });
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return BadRequest(new { message = "两次输入的密码不一致" });
                }

                await _userService.ResetPasswordAsync(id, dto.NewPassword);
                return Ok(new { message = "密码重置成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                if (!Enum.IsDefined(typeof(UserStatus), dto.Status))
                {
                    return BadRequest(new { message = "Invalid status value" });
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                await _userService.UpdateUserStatusAsync(id, dto.Status);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("check-username")]
        public async Task<ActionResult<bool>> CheckUsername([FromQuery] string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                return Ok(user == null);
            }
            catch (Exception ex)
            {
                await LogError("CheckUsername", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("batch/status")]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchUpdateStatusDto model)
        {
            await _userService.BatchUpdateStatusAsync(model.UserIds, model.Status);
            return Ok();
        }

        [HttpPut("batch/reset-password")]
        public async Task<IActionResult> BatchResetPassword([FromBody] BatchResetPasswordDto model)
        {
            await _userService.BatchResetPasswordAsync(model.UserIds, model.NewPassword);
            return Ok();
        }

        private async Task LogInfo(string action, string description)
        {
            var log = new SystemLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = action,
                Module = "Users",
                Description = description,
                IsSuccess = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private async Task LogError(string action, Exception ex)
        {
            var log = new SystemLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = action,
                Module = "Users",
                Description = ex.Message,
                IsSuccess = false,
                ErrorMessage = ex.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            _context.SystemLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}