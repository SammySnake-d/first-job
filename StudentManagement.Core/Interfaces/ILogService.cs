using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using StudentManagement.Core.Models;

namespace StudentManagement.Core.Interfaces
{
    public interface ILogService
    {
        Task CreateLogAsync(SystemLog log);
        Task<IEnumerable<SystemLog>> GetLogsAsync(int page, int pageSize, string keyword);
        Task<int> GetTotalCountAsync(string keyword);
        Task LogInfoAsync(string action, string message);
        Task LogErrorAsync(string action, Exception ex);
        Task<SystemLog> GetLogByIdAsync(int id);
    }
} 