using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Data;
using StudentManagement.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using OfficeOpenXml;

namespace StudentManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 添加 EPPlus License 设置
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var host = CreateHostBuilder(args).Build();

            // 生成swagger.json
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                
                // 获取SwaggerProvider
                var swaggerProvider = services.GetRequiredService<ISwaggerProvider>();
                var swaggerDoc = swaggerProvider.GetSwagger("v1");

                // 确保api-docs目录存在
                var apiDocsPath = Path.Combine(Directory.GetCurrentDirectory(), "api-docs");
                Directory.CreateDirectory(apiDocsPath);

                // 生成swagger.json文件
                var filePath = Path.Combine(apiDocsPath, "swagger.json");
                
                // 使用OpenAPI序列化
                using (var stream = File.Create(filePath))
                using (var writer = new StreamWriter(stream))
                {
                    var openApiWriter = new OpenApiJsonWriter(writer);
                    swaggerDoc.SerializeAsV3(openApiWriter);
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
