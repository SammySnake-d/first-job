using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Interfaces;
using StudentManagement.Core.Models;

namespace StudentManagement.Infrastructure.Services
{
    public class LogService : ILogService
    {
        private readonly IRepository<SystemLog> _logRepository;

        public LogService(IRepository<SystemLog> logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task CreateLogAsync(SystemLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            await _logRepository.AddAsync(log);
        }

        public async Task<IEnumerable<SystemLog>> GetLogsAsync(int page, int pageSize, string keyword)
        {
            var query = _logRepository.Query();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(l => 
                    l.Username.Contains(keyword) ||
                    l.Action.Contains(keyword) ||
                    l.Module.Contains(keyword) ||
                    l.Description.Contains(keyword));
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string keyword)
        {
            var query = _logRepository.Query();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(l => 
                    l.Username.Contains(keyword) ||
                    l.Action.Contains(keyword) ||
                    l.Module.Contains(keyword) ||
                    l.Description.Contains(keyword));
            }

            return await query.CountAsync();
        }

        public async Task LogInfoAsync(string action, string message)
        {
            var log = new SystemLog
            {
                Action = action,
                Description = message,
                IsSuccess = true,
                CreatedAt = DateTime.UtcNow
            };
            await CreateLogAsync(log);
        }

        public async Task LogErrorAsync(string action, Exception ex)
        {
            var log = new SystemLog
            {
                Action = action,
                Description = ex.Message,
                ErrorMessage = ex.ToString(),
                IsSuccess = false,
                CreatedAt = DateTime.UtcNow
            };
            await CreateLogAsync(log);
        }

        public async Task<SystemLog> GetLogByIdAsync(int id)
        {
            return await _logRepository.GetByIdAsync(id);
        }
    }
} 