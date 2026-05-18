using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using TodoLibrary.DataAccess;
using TodoLibrary.Services;

namespace TodoAPI.Extension
{
    public static class CustomServicesExtensions
    {
        public static void AddCustomService(this WebApplicationBuilder builder)
        {

            builder.Services.AddDbContext<TodoContext>(opts =>
            {
                //opts.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
                opts.UseNpgsql(builder.Configuration.GetConnectionString("postgreSQL"));

            });
            builder.Services.AddScoped<ITodoService, TodoService>();

        }
    }
}
