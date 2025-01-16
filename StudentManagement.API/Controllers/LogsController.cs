using Microsoft.AspNetCore.Mvc;
using StudentManagement.Core.Interfaces;
using StudentManagement.API.DTOs;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<LogListDto>>> GetLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null)
        {
            try
            {
                var logs = await _logService.GetLogsAsync(page, pageSize, keyword);
                var total = await _logService.GetTotalCountAsync(keyword);

                var logDtos = logs.Select(l => new LogListDto
                {
                    Id = l.Id,
                    Username = l.Username,
                    Action = l.Action,
                    Module = l.Module,
                    Description = l.Description,
                    IsSuccess = l.IsSuccess,
                    ErrorMessage = l.ErrorMessage,
                    CreatedAt = l.CreatedAt
                });

                return Ok(new { data = logDtos, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
} 