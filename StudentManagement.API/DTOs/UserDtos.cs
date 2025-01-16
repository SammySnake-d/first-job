using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudentManagement.Core.Enums;

namespace StudentManagement.API.DTOs
{
    public class UserDetailDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string StudentNumber { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateUserDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-20个字符之间")]
        public string Username { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "密码长度必须在6-20个字符之间")]
        public string Password { get; set; }

        [Required(ErrorMessage = "姓名不能为空")]
        public string Name { get; set; }

        public string StudentNumber { get; set; }

        [Required(ErrorMessage = "角色不能为空")]
        public UserRole Role { get; set; }
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; }

        [Required(ErrorMessage = "姓名不能为空")]
        public string Name { get; set; }

        [Required(ErrorMessage = "角色不能为空")]
        public UserRole Role { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "新密码不能为空")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "密码长度必须在6-20个字符之间")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "确认密码不能为空")]
        [Compare("NewPassword", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateStatusDto
    {
        public UserStatus Status { get; set; }
    }

    public class BatchUpdateStatusDto
    {
        public List<int> UserIds { get; set; }
        public UserStatus Status { get; set; }
    }

    public class BatchResetPasswordDto
    {
        public List<int> UserIds { get; set; }
        [Required(ErrorMessage = "新密码不能为空")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "密码长度必须在6-20个字符之间")]
        public string NewPassword { get; set; }
    }

    public class UserListDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string StudentNumber { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}