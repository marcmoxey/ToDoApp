using TodoAPI.Extensions;

namespace TodoAPI.Extension
{
    public static  class StandardServicesExtensions
    {
        public static void AddStandardServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.AddSwaggerServices();
        }
    }
}
