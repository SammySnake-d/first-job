using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StudentManagement.Core.Interfaces;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Enums;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using StudentManagement.Core.DTOs;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUserService userService,
            ILogger<AuthController> logger,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userService = userService;
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "用户名和密码不能为空" });
                }

                var isValid = await _userService.ValidatePasswordAsync(request.Username, request.Password);
                if (!isValid)
                {
                    return BadRequest(new { message = "用户名或密码错误" });
                }

                var user = await _userService.GetUserByUsernameAsync(request.Username);

                // 获取用户权限
                var permissions = await _userService.GetUserPermissionsAsync(user.Id);
                var roles = await _userService.GetUserRolesAsync(user.Id);

                // 生成 JWT Token
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        name = user.Name,
                        role = user.Role,
                        permissions = permissions.Select(p => p.Code),
                        roles = roles.Select(r => r.Name),
                        token = token
                    },
                    message = "登录成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                return StatusCode(500, new { message = "登录失败" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await Task.CompletedTask;
                return Ok(new { message = "退出成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "退出登录失败");
                return StatusCode(500, new { message = "退出登录失败" });
            }
        }

        [HttpPost("forgot-password/verify")]
        public async Task<IActionResult> VerifyForgotPassword([FromBody] ForgotPasswordDto request)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    return BadRequest(new { message = "用户不存在" });
                }

                // 检查用户是否允许重置密码
                if (user.Status != UserStatus.Active)
                {
                    return BadRequest(new { message = "没有权限，请联系管理员" });
                }

                return Ok(new { message = "验证成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证用户失败");
                return StatusCode(500, new { message = "验证失败" });
            }
        }

        [HttpPost("forgot-password/reset")]
        public async Task<IActionResult> ResetForgottenPassword([FromBody] ResetForgottenPasswordDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "新密码不能为空" });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new { message = "两次输入的密码不一致" });
                }

                var user = await _userService.GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    return BadRequest(new { message = "用户不存在" });
                }

                // 检查用户是否允许重置密码
                if (user.Status != UserStatus.Active)
                {
                    return BadRequest(new { message = "没有权限，请联系管理员" });
                }

                await _userService.ResetPasswordAsync(user.Id, request.NewPassword);
                return Ok(new { message = "密码重置成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置密码失败");
                return StatusCode(500, new { message = "重置密码失败" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) ||
                    string.IsNullOrEmpty(request.NewPassword) ||
                    string.IsNullOrEmpty(request.ConfirmPassword))
                {
                    return BadRequest(new { message = "请填写所有必填字段" });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new { message = "两次输入的密码不一致" });
                }

                var user = await _userService.GetUserByUsernameAsync(request.Username);

                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                
                // 更新密码
                await _userService.ResetPasswordAsync(user.Id, request.NewPassword);

                // 记录日志
                var log = new SystemLog
                {
                    UserId = user.Id,
                    Action = "修改密码",
                    Description = $"用户 {user.Username} 修改了密码",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.Now
                };

                _logger.LogInformation(log.Description);

                return Ok(new { message = "密码修改成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改密码失败");
                return StatusCode(500, new { message = "修改密码失败，请稍后重试" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(StudentProfileDto), 200)]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "未找到用户信息" });
                }

                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                // 如果是学生，返回学生信息
                if (user.Role == UserRole.Student)
                {
                    var student = await _context.Students
                        .Include(s => s.Class)
                        .FirstOrDefaultAsync(s => s.StudentNumber == user.StudentNumber);

                    if (student == null)
                    {
                        return NotFound(new { message = "未找到学生信息" });
                    }

                    return Ok(new
                    {
                        id = student.Id,
                        studentNumber = student.StudentNumber,
                        name = student.Name,
                        gender = student.Gender,
                        className = student.Class?.Name,
                        grade = student.Class?.Grade,
                        email = student.Email,
                        contactNumber = student.ContactNumber,
                        status = student.Status,
                        enrollmentDate = student.EnrollmentDate
                    });
                }

                // 非学生用户返回用户信息
                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    name = user.Name,
                    role = user.Role,
                    email = user.Email,
                    phone = user.Phone,
                    status = user.Status,
                    createdAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取个人信息失败");
                return StatusCode(500, new { message = "获取个人信息失败" });
            }
        }

        // 添加生成 JWT Token 的方法
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("StudentNumber", user.StudentNumber ?? "")
            };

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Username { get; set; } = string.Empty;
    }

    public class ResetForgottenPasswordDto
    {
        public string Username { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "新密码不能为空")]
        [MinLength(6, ErrorMessage = "密码长度不能小于6位")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare("NewPassword", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}