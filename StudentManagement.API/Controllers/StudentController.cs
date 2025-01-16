using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using StudentManagement.Infrastructure.Data;
using StudentManagement.Core.Entities;
using System.Linq;

namespace StudentManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("未接收到文件或文件为空");

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("只支持.xlsx格式文件");

            try
            {
                if (file.Length > 10485760) // 10MB
                    return BadRequest("文件大小不能超过10MB");

                // 先获取所有班级信息
                var allClasses = await _context.Classes.ToListAsync();
                Console.WriteLine($"系统中的班级: {string.Join(", ", allClasses.Select(c => $"{c.Grade}-{c.Name}班"))}");

                if (!allClasses.Any())
                {
                    return BadRequest("系统中没有班级信息，请先添加班级");
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                Console.WriteLine($"Excel文件总行数: {rowCount}");

                if (rowCount <= 1)
                    return BadRequest("Excel文件中没有数据");

                var students = new List<Student>();
                // 获取所有现有学号
                var existingStudentNumbers = await _context.Students
                    .Select(s => s.StudentNumber)
                    .ToListAsync();

                // 从第二行开始读取(跳过表头)
                for (int row = 2; row <= rowCount; row++)
                {
                    // 检查这一行是否所有单元格都为空
                    var isEmptyRow = true;
                    for (int col = 1; col <= 9; col++)
                    {
                        if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Value?.ToString()))
                        {
                            isEmptyRow = false;
                            break;
                        }
                    }

                    // 如果是空行，跳过
                    if (isEmptyRow)
                        continue;

                    try
                    {
                        // 获取班级名称
                        var classNameRaw = worksheet.Cells[row, 8].Value?.ToString();
                        Console.WriteLine($"第{row}行的班级名称: '{classNameRaw}'");

                        if (string.IsNullOrEmpty(classNameRaw))
                        {
                            var cellValue = worksheet.Cells[row, 8].Value;
                            Console.WriteLine($"第{row}行班级单元格的实际值: {cellValue?.GetType().Name ?? "null"} - {cellValue ?? "null"}");
                            return BadRequest($"第{row}行的班级名称为空");
                        }

                        // 解析班级名称
                        var parts = classNameRaw.Split('-');
                        if (parts.Length != 2)
                            return BadRequest($"第{row}行的班级名称格式不正确，应为'X年级-XXX班'格式");

                        var grade = parts[0];  // 如 "二年级"
                        var className = parts[1].TrimEnd('班');  // 如 "111a"

                        Console.WriteLine($"解析结果 - 年级: '{grade}', 班级: '{className}'");

                        // 查找匹配的班级
                        var matchedClass = allClasses.FirstOrDefault(c =>
                            c.Grade == grade &&
                            c.Name == className);

                        if (matchedClass == null)
                        {
                            // 记录可用班级信息到日志
                            var availableClasses = string.Join(", ", allClasses.Select(c => $"{c.Grade}-{c.Name}班"));
                            Console.WriteLine($"可用的班级: {availableClasses}");

                            // 返回更友好的错误信息
                            return BadRequest($"导入失败：班级 '{classNameRaw}' 不存在，请确保班级名称格式正确且班级已创建");
                        }

                        var student = new Student
                        {
                            StudentNumber = worksheet.Cells[row, 1].Value?.ToString(),
                            Name = worksheet.Cells[row, 2].Value?.ToString(),
                            Gender = worksheet.Cells[row, 3].Value?.ToString() == "男" ? "男" : "女",
                            DateOfBirth = GetExcelDateTime(worksheet.Cells[row, 4]),
                            Email = worksheet.Cells[row, 5].Value?.ToString(),
                            ContactNumber = worksheet.Cells[row, 6].Value?.ToString(),
                            Address = worksheet.Cells[row, 7].Value?.ToString(),
                            ClassId = matchedClass?.Id ?? throw new Exception($"导入错误：班级 '{classNameRaw}' 不存在"),
                            Status = GetStatus(worksheet.Cells[row, 9].Value?.ToString()),
                            EnrollmentDate = DateTime.Now
                        };

                        // 检查学号是否已存在
                        if (existingStudentNumbers.Contains(student.StudentNumber))
                        {
                            return BadRequest($"导入失败：学号 '{student.StudentNumber}' 已存在");
                        }

                        // 验证必填字段
                        if (string.IsNullOrEmpty(student.StudentNumber) ||
                            string.IsNullOrEmpty(student.Name) ||
                            string.IsNullOrEmpty(student.Gender) ||
                            string.IsNullOrEmpty(student.Email) ||
                            string.IsNullOrEmpty(student.ContactNumber))
                        {
                            return BadRequest($"第{row}行存在必填字段为空");
                        }

                        // 验证邮箱格式
                        if (!IsValidEmail(student.Email))
                        {
                            return BadRequest($"第{row}行的邮箱格式不正确");
                        }

                        // 验证手机号格式
                        if (!IsValidPhone(student.ContactNumber))
                        {
                            return BadRequest($"第{row}行的手机号格式不正确");
                        }

                        students.Add(student);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest($"处理第{row}行数据时出错: {ex.Message}");
                    }
                }

                // 批量添加生
                await _context.Students.AddRangeAsync(students);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"导入失败: {ex.GetType().Name} - {ex.Message}");
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.Class)
                    .ToListAsync();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("导入模板");

                // 添加表头
                worksheet.Cells[1, 1].Value = "学号*";
                worksheet.Cells[1, 2].Value = "姓名*";
                worksheet.Cells[1, 3].Value = "性别*";
                worksheet.Cells[1, 4].Value = "出生日期*";
                worksheet.Cells[1, 5].Value = "邮箱*";
                worksheet.Cells[1, 6].Value = "电话*";
                worksheet.Cells[1, 7].Value = "地址";
                worksheet.Cells[1, 8].Value = "班级*";
                worksheet.Cells[1, 9].Value = "状态*";

                // 添加必填说明
                worksheet.Cells[1, 1].AddComment("必填项", "系统");
                worksheet.Cells[1, 2].AddComment("必填项", "系统");
                worksheet.Cells[1, 3].AddComment("必填项，请从下拉列表选择", "系统");
                worksheet.Cells[1, 4].AddComment("必填项，格式：yyyy-MM-dd", "系统");
                worksheet.Cells[1, 5].AddComment("必填项", "系统");
                worksheet.Cells[1, 6].AddComment("必填项", "系统");
                worksheet.Cells[1, 8].AddComment("必填项，格式：二年级-111a班", "系统");
                worksheet.Cells[1, 9].AddComment("必填项，请从下拉列表选择", "系统");

                // 设置表头样式
                var headerRange = worksheet.Cells[1, 1, 1, 9];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // 添加数据
                int row = 2;
                foreach (var student in students)
                {
                    worksheet.Cells[row, 1].Value = student.StudentNumber;
                    worksheet.Cells[row, 2].Value = student.Name;
                    worksheet.Cells[row, 3].Value = student.Gender;
                    worksheet.Cells[row, 4].Value = student.DateOfBirth.Date;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "yyyy/mm/dd";
                    worksheet.Cells[row, 5].Value = student.Email;
                    worksheet.Cells[row, 6].Value = student.ContactNumber;
                    worksheet.Cells[row, 7].Value = student.Address;
                    worksheet.Cells[row, 8].Value = student.Class != null
                        ? $"{student.Class.Grade}-{student.Class.Name}班"
                        : "-";
                    worksheet.Cells[row, 9].Value = GetStatusText(student.Status);
                    row++;
                }

                // 设置数据验证
                var genderValidation = worksheet.DataValidations.AddListValidation(worksheet.Cells[$"C2:C{row}"].Address);
                genderValidation.ShowErrorMessage = true;
                genderValidation.ErrorTitle = "输入错误";
                genderValidation.Error = "请从下拉列表中选择性别";
                foreach (var gender in new[] { "男", "女" })
                {
                    genderValidation.Formula.Values.Add(gender);
                }

                var statusValidation = worksheet.DataValidations.AddListValidation(worksheet.Cells[$"I2:I{row}"].Address);
                statusValidation.ShowErrorMessage = true;
                statusValidation.ErrorTitle = "输入错误";
                statusValidation.Error = "请从下拉列表中选择状态";
                foreach (var status in new[] { "在读", "休学", "毕业", "退学" })
                {
                    statusValidation.Formula.Values.Add(status);
                }

                // 设置列宽
                worksheet.Columns.AutoFit();

                var content = package.GetAsByteArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "学生信息导入模板.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"导出失败: {ex.Message}");
            }
        }

        [HttpGet("template")]
        public IActionResult GetImportTemplate()
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("导入模板");

                // 添加表头和说明
                worksheet.Cells[1, 1].Value = "学号*";
                worksheet.Cells[1, 2].Value = "姓名*";
                worksheet.Cells[1, 3].Value = "性别*";
                worksheet.Cells[1, 4].Value = "出生日期*";
                worksheet.Cells[1, 5].Value = "邮箱*";
                worksheet.Cells[1, 6].Value = "电话*";
                worksheet.Cells[1, 7].Value = "地址";
                worksheet.Cells[1, 8].Value = "班级*";
                worksheet.Cells[1, 9].Value = "状态*";

                // 添加必填说明
                worksheet.Cells[1, 1].AddComment("必填项", "系统");
                worksheet.Cells[1, 2].AddComment("必填项", "系统");
                worksheet.Cells[1, 3].AddComment("必填项，请从下拉列表选择", "系统");
                worksheet.Cells[1, 4].AddComment("必填项，格式：yyyy-MM-dd", "系统");
                worksheet.Cells[1, 5].AddComment("必填项", "系统");
                worksheet.Cells[1, 6].AddComment("必填项", "系统");
                worksheet.Cells[1, 8].AddComment("必填项，格式：二年级-111a班", "系统");
                worksheet.Cells[1, 9].AddComment("必填项，请从下拉列表选择", "系统");

                // 设置表样式
                var headerRange = worksheet.Cells[1, 1, 1, 9];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // 添加示例数据
                worksheet.Cells[2, 1].Value = "222";  // 学号
                worksheet.Cells[2, 2].Value = "张三";  // 姓名
                worksheet.Cells[2, 3].Value = "男";   // 性别
                worksheet.Cells[2, 4].Value = DateTime.Parse("2024-12-10");  // 出生日期
                worksheet.Cells[2, 5].Value = "zhangsan@example.com";  // 邮箱
                worksheet.Cells[2, 6].Value = "13800138000";  // 电话
                worksheet.Cells[2, 7].Value = "北京市海淀区";  // 地址
                worksheet.Cells[2, 8].Value = "二年级-111a班";  // 班级 - 完整格式
                worksheet.Cells[2, 9].Value = "在读";  // 状态

                // 设置性别列的数据验证
                var genderValidation = worksheet.DataValidations.AddListValidation(worksheet.Cells["C2:C1000"].Address);
                genderValidation.ShowErrorMessage = true;
                genderValidation.ErrorTitle = "输入错误";
                genderValidation.Error = "请从下拉列表中选性别";
                foreach (var gender in new[] { "男", "女" })
                {
                    genderValidation.Formula.Values.Add(gender);
                }

                // 设置状态列的数据验证
                var statusValidation = worksheet.DataValidations.AddListValidation(worksheet.Cells["I2:I1000"].Address);
                statusValidation.ShowErrorMessage = true;
                statusValidation.ErrorTitle = "输入错误";
                statusValidation.Error = "请从拉列表中选择状态";
                foreach (var status in new[] { "在读", "休学", "毕业", "退学" })
                {
                    statusValidation.Formula.Values.Add(status);
                }

                // 修改日期格式设置
                worksheet.Cells["D2:D1000"].Style.Numberformat.Format = "yyyy/mm/dd";

                // 设置示例数据时确保日期格式正确
                worksheet.Cells[2, 4].Value = DateTime.Parse("2024-12-10");  // 存储为真实的日期值
                worksheet.Cells[2, 4].Style.Numberformat.Format = "yyyy/mm/dd";  // 显示格式

                // 设置列宽
                worksheet.Columns.AutoFit();

                var content = package.GetAsByteArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "学生信息导入模板.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest($"获取模板失败: {ex.Message}");
            }
        }

        // 辅助方法
        private string GetStatusText(string status)
        {
            var texts = new Dictionary<string, string>
            {
                { "Normal", "在读" },
                { "Suspended", "休学" },
                { "Graduated", "毕业" },
                { "Dropped", "退学" },
                { "Transferred", "转学" }
            };
            return texts.GetValueOrDefault(status, status);
        }

        private string GetStatus(string statusText)
        {
            var statuses = new Dictionary<string, string>
            {
                { "在读", "Normal" },
                { "休学", "Suspended" },
                { "毕业", "Graduated" },
                { "退学", "Dropped" },
                { "转学", "Transferred" }
            };
            return statuses.GetValueOrDefault(statusText, "Normal");
        }

        // 验证邮箱格式
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // 验证手机号格式
        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^1[3-9]\d{9}$");
        }

        private DateTime GetExcelDateTime(ExcelRange cell)
        {
            try
            {
                // 如果单元格为空，返回当前日期
                if (cell.Value == null)
                    return DateTime.Now;

                // 如果是数字（Excel内部日期格式），转换为DateTime
                if (cell.Value is double numericDate)
                    return DateTime.FromOADate(numericDate);

                // 尝试解析字符串格式的日期
                var dateStr = cell.Value.ToString();
                if (DateTime.TryParse(dateStr, out DateTime result))
                    return result;

                // 如果都失败了，返回当前日期
                return DateTime.Now;
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}