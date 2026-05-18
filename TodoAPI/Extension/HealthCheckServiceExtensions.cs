namespace TodoAPI.Extension
{
    public static class HealthCheckServiceExtensions
    {
        public static void AddHealthCheckServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
              .AddSqlServer(builder.Configuration.GetConnectionString("postgreSQL"));

        }
    }
}
