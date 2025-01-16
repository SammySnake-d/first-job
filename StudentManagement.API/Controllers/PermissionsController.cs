using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetPermissions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string keyword = null,
            [FromQuery] string module = null)
        {
            try
            {
                var query = _context.Permissions.AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(p => 
                        p.Name.Contains(keyword) || 
                        p.Code.Contains(keyword));
                }

                if (!string.IsNullOrEmpty(module))
                {
                    query = query.Where(p => p.Module == module);
                }

                var total = await query.CountAsync();
                var permissions = await query
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.Code)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Code,
                        p.Description,
                        p.Module,
                        p.Category
                    })
                    .ToListAsync();

                return Ok(new { data = permissions, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取权限列表失败", error = ex.Message });
            }
        }

        [HttpGet("modules")]
        public async Task<ActionResult<object>> GetModules()
        {
            try
            {
                var modules = await _context.Permissions
                    .Select(p => p.Module)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();

                return Ok(modules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取模块列表失败", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Permission>> CreatePermission(Permission permission)
        {
            try
            {
                if (await _context.Permissions.AnyAsync(p => p.Code == permission.Code))
                {
                    return BadRequest(new { message = "权限编码已存在" });
                }

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, permission);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "创建权限失败", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);

            if (permission == null)
            {
                return NotFound(new { message = "未找到权限" });
            }

            return permission;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, Permission permission)
        {
            if (id != permission.Id)
            {
                return BadRequest(new { message = "权限ID不匹配" });
            }

            try
            {
                var existingPermission = await _context.Permissions.FindAsync(id);
                if (existingPermission == null)
                {
                    return NotFound(new { message = "未找到权限" });
                }

                existingPermission.Name = permission.Name;
                existingPermission.Description = permission.Description;
                existingPermission.Module = permission.Module;
                existingPermission.Category = permission.Category;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新权限失败", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                var permission = await _context.Permissions.FindAsync(id);
                if (permission == null)
                {
                    return NotFound(new { message = "未找到权限" });
                }

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除权限失败", error = ex.Message });
            }
        }
    }
} 