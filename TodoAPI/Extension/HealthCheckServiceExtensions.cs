namespace TodoAPI.Extension
{
    public static class HealthCheckServiceExtensions
    {
        public static void AddHealthCheckServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
           .AddNpgSql(builder.Configuration.GetConnectionString("postgreSQL"));

        }
    }
}
