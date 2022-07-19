//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestAzure
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //初始化当前Host实例时候随机生成一个GUID，用于在此后的请求中判断当前时候是否被标记为健康，非健康，回收。
            var instance = Guid.NewGuid();
            //firstFailure来记录第一个失败请求所进入当前实例的时间，firstFailure保存在内存中
            DateTime? firstFailure = null;

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                webBuilder.Configure(configureApp =>
                configureApp.Run(async context =>
                {
                    //当请求URL为fail时候，认为设置返回状态为500，告诉App Service的Health Check功能，当前实例出现故障。
                    if (firstFailure == null && context.Request.Path.Value.ToLower().Contains("fail"))
                    {
                        firstFailure = DateTime.UtcNow;
                    }

                    if (firstFailure != null)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(
                           $"当前实例的GUID为 {instance}.\n" +
                            $"这个实例最早出现错误的时间是 {firstFailure.Value}.\n\n" +
                            $"根据文档的描述 https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check, 如果一个实例在一直保持unhealthy状态一小时，它将会被一个新实例所取代\n\n" +
                            $"According to https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check, \"If an instance remains unhealthy for one hour, it will be replaced with new instance.\".");
                    }
                    else
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync($"当前实例的GUID为 {instance}.\n" +
                            $"此实例报告显示一切工作正常.");
                    }
                })));



            //public static IHostBuilder CreateHostBuilder(string[] args) =>
            //    Host.CreateDefaultBuilder(args)
            //        .ConfigureWebHostDefaults(webBuilder =>
            //        {
            //            webBuilder.UseStartup<Startup>();
            //        });
        }
    }
}