using System.Reflection;
using Microservices.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Application.Interfaces;
using UserService.Application.Mappings;
using UserService.Domain.Interfaces;
using UserService.Infrastructure;
using UserService.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var conn = builder.Configuration.GetConnectionString("SupportDeskConnection");
builder.Services.AddDbContext<UserServiceDbContext>(options => options.UseNpgsql(conn));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Application.Services.UserService>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(UserProfile).Assembly);

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        c.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.Run();
