
using EncryptRequest.Middleware;
using Microsoft.Extensions.Configuration;
using static EncryptRequest.Middleware.EncryptionMiddleware;

namespace EncryptRequest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
           // builder.Services.AddTransient<EncryptionMiddleware>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<EncryptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseMiddleware<EncryptionMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}
