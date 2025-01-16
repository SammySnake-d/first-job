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
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetRoles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string keyword = null)
        {
            try
            {
                var query = _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(r => r.Name.Contains(keyword));
                }

                var total = await query.CountAsync();
                var roles = await query
                    .OrderBy(r => r.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.Name,
                        r.Description,
                        r.IsSystem,
                        Permissions = r.RolePermissions.Select(rp => new
                        {
                            rp.Permission.Id,
                            rp.Permission.Name,
                            rp.Permission.Code
                        })
                    })
                    .ToListAsync();

                return Ok(new { data = roles, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取角色列表失败", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole(Role role)
        {
            try
            {
                if (await _context.Roles.AnyAsync(r => r.Name == role.Name))
                {
                    return BadRequest(new { message = "角色名称已存在" });
                }

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "创建角色失败", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(new { message = "未找到角色" });
            }

            return role;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            if (id != role.Id)
            {
                return BadRequest(new { message = "角色ID不匹配" });
            }

            try
            {
                var existingRole = await _context.Roles.FindAsync(id);
                if (existingRole == null)
                {
                    return NotFound(new { message = "未找到角色" });
                }

                if (existingRole.IsSystem)
                {
                    return BadRequest(new { message = "系统预设角色不能修改" });
                }

                existingRole.Name = role.Name;
                existingRole.Description = role.Description;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新角色失败", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "未找到角色" });
                }

                if (role.IsSystem)
                {
                    return BadRequest(new { message = "系统预设角色不能删除" });
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除角色失败", error = ex.Message });
            }
        }

        [HttpPost("{id}/permissions")]
        public async Task<IActionResult> AssignPermissions(int id, [FromBody] List<int> permissionIds)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound(new { message = "未找到角色" });
                }

                if (role.IsSystem)
                {
                    return BadRequest(new { message = "系统预设角色的权限不能修改" });
                }

                // 移除现有权限
                _context.RolePermissions.RemoveRange(role.RolePermissions);

                // 添加新权限
                foreach (var permissionId in permissionIds)
                {
                    role.RolePermissions.Add(new RolePermission
                    {
                        RoleId = id,
                        PermissionId = permissionId,
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "权限分配成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "权限分配失败", error = ex.Message });
            }
        }
    }
} 