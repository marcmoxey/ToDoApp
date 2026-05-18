using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoAPI.Extension;
using TodoAPI.Extensions;
using TodoLibrary.DataAccess;
using TodoLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.AddStandardServices();
builder.AddAuthenticationServices();
builder.AddCustomService();
builder.AddHealthCheckServices();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.MapHealthChecks("/health").AllowAnonymous();
}

app.Run();
