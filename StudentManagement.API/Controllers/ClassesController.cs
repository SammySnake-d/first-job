using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Entities;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/class")]
    public class ClassesController : ControllerBase
    {
        private readonly IRepository<Class> _classRepository;
        private readonly ApplicationDbContext _context;

        public ClassesController(IRepository<Class> classRepository, ApplicationDbContext context)
        {
            _classRepository = classRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Class>>> GetClasses()
        {
            var classes = await _classRepository.GetAllAsync();
            return Ok(classes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Class>> GetClass(int id)
        {
            var @class = await _classRepository.GetByIdAsync(id);

            if (@class == null)
            {
                return NotFound();
            }

            return @class;
        }

        [HttpPost]
        public async Task<ActionResult<Class>> CreateClass(Class @class)
        {
            if (await _context.Classes.AnyAsync(c => 
                c.Grade == @class.Grade && 
                c.Name == @class.Name))
            {
                return BadRequest(new { message = $"年级 '{@class.Grade}' 中已存在名称为 '{@class.Name}' 的班级" });
            }

            var createdClass = await _classRepository.AddAsync(@class);
            return CreatedAtAction(nameof(GetClass), new { id = createdClass.Id }, createdClass);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(int id, Class @class)
        {
            if (id != @class.Id)
            {
                return BadRequest(new { message = "无效的请求" });
            }

            var existingClass = await _context.Classes
                .FirstOrDefaultAsync(c => 
                    c.Grade == @class.Grade && 
                    c.Name == @class.Name && 
                    c.Id != id);
            if (existingClass != null)
            {
                return BadRequest(new { message = $"年级 '{@class.Grade}' 中已存在名称为 '{@class.Name}' 的班级" });
            }

            try
            {
                await _classRepository.UpdateAsync(@class);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClassExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var @class = await _classRepository.GetByIdAsync(id);
            if (@class == null)
            {
                return NotFound();
            }

            await _classRepository.DeleteAsync(id);
            return NoContent();
        }

        private async Task<bool> ClassExists(int id)
        {
            var @class = await _classRepository.GetByIdAsync(id);
            return @class != null;
        }
    }
} 